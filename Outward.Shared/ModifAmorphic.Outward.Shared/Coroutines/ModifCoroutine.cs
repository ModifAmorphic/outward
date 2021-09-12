using ModifAmorphic.Outward.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Coroutines
{
    public class ModifCoroutine
    {
        private IModifLogger Logger => _getLogger?.Invoke() ?? new NullLogger();
        private readonly Func<IModifLogger> _getLogger;

        public ModifCoroutine(Func<IModifLogger> getLogger) => _getLogger = getLogger;

        public IEnumerator InvokeAfter(Func<bool> condition, Action action, int timeoutSecs, float waitSecs = .25f)
        {
            var maxWaits = (int)(timeoutSecs / waitSecs);
            int waits = 0;
            while (!condition.Invoke() && waits++ < maxWaits)
            {
                yield return new WaitForSeconds(waitSecs);
            }
            if (waits < maxWaits)
            {
                Logger.LogDebug($"{this.GetType().Name}::{nameof(InvokeAfter)}: Wait condition {condition.Method.Name} met." +
                    $" Invoking action {action.Method.Name}.");
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogException($"{this.GetType().Name}::{nameof(InvokeAfter)}:" +
                        $" Unexpected exception invoking Action {action.Method.Name}.", ex);
                }
            }
            else
            {
                Logger.LogError($"{this.GetType().Name}::{nameof(InvokeAfter)}: Timed out after waiting {timeoutSecs} seconds for condition {condition.Method.Name} to be met." +
                    $" Action not invoked: {action.Method.Name}.");
            }
        }
    }
}
