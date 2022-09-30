using System;
using UnityEngine;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionUI.Extensions
{
    public static class EventExtensions
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

        public static bool TryInvoke<T1, T2>(this UnityEvent<T1, T2> unityEvent, T1 eventArg1, T2 eventArg2)
        {
            try
            {
                unityEvent.Invoke(eventArg1, eventArg2);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        public static bool TryInvoke(this Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        public static bool TryInvoke<T>(this Action<T> action, T eventArg)
        {
            try
            {
                action.Invoke(eventArg);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        public static bool TryInvoke<T1, T2>(this Action<T1, T2> action, T1 eventArg1, T2 eventArg2)
        {
            try
            {
                action.Invoke(eventArg1, eventArg2);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        public static bool TryInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 eventArg1, T2 eventArg2, T3 eventArg3)
        {
            try
            {
                action.Invoke(eventArg1, eventArg2, eventArg3);
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
