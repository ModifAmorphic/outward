namespace ModifAmorphic.Outward.StashPacks
{
    internal static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.stashpacks";
        public const string ModName = "StashPacks";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "1.0.1";
#endif
        public const string MinimumConfigVersion = "1.0.1";
    }
}
