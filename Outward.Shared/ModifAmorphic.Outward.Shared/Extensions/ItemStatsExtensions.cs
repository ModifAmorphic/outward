namespace ModifAmorphic.Outward.Extensions
{
    public static class ItemStatsExtensions
    {
        public static void SetBaseValue(this ItemStats itemStats, int baseValue) =>
            itemStats.SetPrivateField("m_baseValue", baseValue);

        public static void SetBaseMaxDurability(this ItemStats itemStats, int baseMaxDurability) =>
            itemStats.SetPrivateField("m_baseMaxDurability", baseMaxDurability);

        public static void SetRawWeight(this ItemStats itemStats, float rawWeight) =>
            itemStats.SetPrivateField("m_rawWeight", rawWeight);

        public static void SetEffectiveness(this ItemStats itemStats, float effectiveness) =>
            itemStats.SetPrivateField("m_effectiveness", effectiveness);

        public static void SetPreviousDurability(this ItemStats itemStats, float previousDurability) =>
            itemStats.SetPrivateField("m_previousDurability", previousDurability);
    }
}
