namespace ModifAmorphic.Outward.RespecPotions
{
    internal static class ModInfo
    {
        public const string ModId = "modifamorphic.outward.respecpotions";
        public const string ModName = "RespecPotions";
#if PIPELINE
        public const string ModVersion = "${PACKAGE_VERSION}";
#else
        public const string ModVersion = "0.0.1";
#endif
        public const string MinimumConfigVersion = "0.0.1";

        public const int ItemStartID = -13010000;
    }
}
