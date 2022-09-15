using System;
using UnityEngine;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Extensions
{
    public static class UnityEventExtensions
    {
        public static bool TryInvoke(this UnityEvent unityEvent)
        {
            try
            {
                unityEvent.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        public static bool TryInvoke<T>(this UnityEvent<T> unityEvent, T eventArg)
        {
            try
            {
                unityEvent.Invoke(eventArg);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }
    }
}
