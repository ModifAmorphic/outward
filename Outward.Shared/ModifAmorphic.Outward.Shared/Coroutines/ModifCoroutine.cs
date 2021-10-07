using BepInEx;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Coroutines
{
    public class ModifCoroutine
    {
        protected IModifLogger Logger => _getLogger?.Invoke() ?? new NullLogger();
        private readonly Func<IModifLogger> _getLogger;
        protected const float DefaultTicSeconds = .25f;
        protected readonly BaseUnityPlugin _unityPlugin;

        public ModifCoroutine(BaseUnityPlugin unityPlugin, Func<IModifLogger> getLogger) => 
            (_unityPlugin, _getLogger) = (unityPlugin, getLogger);

        public IEnumerator InvokeAfter(Func<bool> condition, Action action, int timeoutSecs, float waitTicSecs = 0f, Func<bool> cancelCondition = null)
        {
            var maxWaits = 0;
            bool useStopwatch = false;
            var stopwatch = new Stopwatch();

            if (waitTicSecs != 0f)
                maxWaits = (int)(timeoutSecs / waitTicSecs);
            else
            {
                useStopwatch = true;
                stopwatch.Start();
            }

            int waits = 0;
            var isCanceled = cancelCondition?.Invoke() ?? false;
            while (!condition.Invoke() && !isCanceled &&
                (waits++ < maxWaits || (useStopwatch && stopwatch.Elapsed.TotalSeconds < timeoutSecs)) )
            {
                isCanceled = cancelCondition?.Invoke() ?? false;
                if (waitTicSecs > 0f)
                    yield return new WaitForSeconds(waitTicSecs);
                else
                    yield return null;
            }
            if (useStopwatch)
                stopwatch.Stop();

            if ((waits < maxWaits || (useStopwatch && stopwatch.Elapsed.TotalSeconds < timeoutSecs)) && !isCanceled)
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
            else if (isCanceled)
            {
                Logger.LogDebug($"{this.GetType().Name}::{nameof(InvokeAfter)}: Cancel condition {cancelCondition.Method.Name} was met. Canceling coroutine." +
                    $" Action not invoked: {action.Method.Name}.");
            }
            else
            {
                Logger.LogError($"{this.GetType().Name}::{nameof(InvokeAfter)}: Timed out after waiting {timeoutSecs} seconds for condition {condition.Method.Name} to be met." +
                    $" Action not invoked: {action.Method.Name}.");
            }
            
        }
        public IEnumerator InvokeAfter<T>(Func<bool> condition, Action<T> action, Func<T> valueFactory, int timeoutSecs, float waitTicSecs = 0f, Func<bool> cancelCondition = null)
        {
            var maxWaits = (int)(timeoutSecs / waitTicSecs);
            
            bool useStopwatch = false;
            var stopwatch = new Stopwatch();

            if (waitTicSecs != 0f)
                maxWaits = (int)(timeoutSecs / waitTicSecs);
            else
            {
                useStopwatch = true;
                stopwatch.Start();
            }

            var isCanceled = cancelCondition?.Invoke() ?? false;
            int waits = 0;
            while (!condition.Invoke() && !isCanceled &&
                (waits++ < maxWaits || (useStopwatch && stopwatch.Elapsed.TotalSeconds < timeoutSecs)))
            {
                isCanceled = cancelCondition?.Invoke() ?? false;
                if (waitTicSecs > 0f)
                    yield return new WaitForSeconds(waitTicSecs);
                else
                    yield return null;
            }
            if (useStopwatch)
                stopwatch.Stop();

            if ((waits < maxWaits || (useStopwatch && stopwatch.Elapsed.TotalSeconds < timeoutSecs)) && !isCanceled)
            {
                Logger.LogDebug($"{this.GetType().Name}::{nameof(InvokeAfter)}: Wait condition {condition.Method.Name} met." +
                    $" Invoking action {action.Method.Name}.");
                try
                {
                    action.Invoke(valueFactory.Invoke());
                }
                catch (Exception ex)
                {
                    Logger.LogException($"{this.GetType().Name}::{nameof(InvokeAfter)}:" +
                        $" Unexpected exception invoking Action {action.Method.Name}.", ex);
                }
            }
            else if (isCanceled)
            {
                Logger.LogDebug($"{this.GetType().Name}::{nameof(InvokeAfter)}: Cancel condition {cancelCondition.Method.Name} was met. Canceling coroutine." +
                    $" Action not invoked: {action.Method.Name}.");
            }
            else
            {
                Logger.LogError($"{this.GetType().Name}::{nameof(InvokeAfter)}: Timed out after waiting {timeoutSecs} seconds for condition {condition.Method.Name} to be met." +
                    $" Action not invoked: {action.Method.Name}.");
            }
        }

        public IEnumerator InvokeUntil(Func<bool> endCondition, Action action, float waitTicSecs = 0f, Action finalAction = null)
        {
            while (!endCondition.Invoke())
            {
                action.Invoke();
                if (waitTicSecs > 0f)
                    yield return new WaitForSeconds(waitTicSecs);
                else
                    yield return null;
            }
            if (finalAction != null)
            {
                Logger.LogDebug($"{this.GetType().Name}::{nameof(InvokeUntil)}: End condition {endCondition.Method.Name} met." +
                    $" Invoking final action {finalAction.Method.Name} and exiting Coroutine.");
                finalAction.Invoke();
            }
            else
                Logger.LogDebug($"{this.GetType().Name}::{nameof(InvokeUntil)}: End condition {endCondition.Method.Name} met." +
                    $" Exiting Coroutine.");
        }

        public void StartRoutine(IEnumerator routine)
        {
            _unityPlugin.StartCoroutine(routine);
        }
    }
}
