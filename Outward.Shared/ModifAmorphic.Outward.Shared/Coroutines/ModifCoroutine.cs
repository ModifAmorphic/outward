﻿using ModifAmorphic.Outward.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Coroutines
{
    public class ModifCoroutine
    {
        protected IModifLogger Logger => _getLogger?.Invoke() ?? new NullLogger();
        private readonly Func<IModifLogger> _getLogger;
        protected const float DefaultTicSeconds = .25f; 

        public ModifCoroutine(Func<IModifLogger> getLogger) => _getLogger = getLogger;

        public IEnumerator InvokeAfter(Func<bool> condition, Action action, int timeoutSecs, float waitTicSecs = 0f, Func<bool> cancelCondition = null)
        {
            var maxWaits = (int)(timeoutSecs / waitTicSecs);
            int waits = 0;
            var isCanceled = cancelCondition?.Invoke() ?? false;
            while (!condition.Invoke() && waits++ < maxWaits && !isCanceled)
            {
                isCanceled = cancelCondition?.Invoke() ?? false;
                if (waitTicSecs > 0f)
                    yield return new WaitForSeconds(waitTicSecs);
                yield return null;
            }
            if (waits < maxWaits && !isCanceled)
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
            int waits = 0;
            var isCanceled = cancelCondition?.Invoke() ?? false;
            while (!condition.Invoke() && waits++ < maxWaits && !isCanceled)
            {
                isCanceled = cancelCondition?.Invoke() ?? false;
                if (waitTicSecs > 0f)
                    yield return new WaitForSeconds(waitTicSecs);
                yield return null;
            }
            if (waits < maxWaits && !isCanceled)
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
    }
}
