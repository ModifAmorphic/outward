namespace ModifAmorphic.Outward.NewCaldera
{
    internal static class ModInfo
    {
        public const string ModId = "ModifAmorphic.Outward.NewCaldera";
        public const string ModName = "NewCaldera";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "1.0.0";
#endif
        public const string MinimumConfigVersion = "1.0.0";

        public const string BuildingsModName = "Buildings";
        public const string AiModName = "AI";
    }
}
