namespace ModifAmorphic.Outward.Unity.ActionUI
{
    internal static class DebugLogger
    {
        public static void Log(string message)
        {
#if DEBUGLOCAL || DEBUG
            DebugLogger.Log(message);
#endif
        }
    }
}
