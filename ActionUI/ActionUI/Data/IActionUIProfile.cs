namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IActionUIProfile
    {
        string Name { get; set; }
        bool ActionSlotsEnabled { get; set; }
        bool DurabilityDisplayEnabled { get; set; }
        bool EquipmentSetsEnabled { get; set; }
        EquipmentSetsSettingsProfile EquipmentSetsSettingsProfile { get; set; }
        StashSettingsProfile StashSettingsProfile { get; set; }

    }

    public class EquipmentSetsSettingsProfile
    {
        public bool ArmorSetsInCombatEnabled { get; set; }
        public bool SkipWeaponAnimationsEnabled { get; set; }
        public bool StashEquipEnabled { get; set; }
        public bool StashEquipAnywhereEnabled { get; set; }
        public bool StashUnequipEnabled { get; set; }
        public bool StashUnequipAnywhereEnabled { get; set; }
    }

    public class StashSettingsProfile
    {
        public bool CharInventoryEnabled { get; set; }
        public bool CharInventoryAnywhereEnabled { get; set; }
        public bool MerchantEnabled { get; set; }
        public bool MerchantAnywhereEnabled { get; set; }
        public bool CraftingInventoryEnabled { get; set; }
        public bool CraftingInventoryAnywhereEnabled { get; set; }
        public bool PreservesFoodEnabled { get; set; }
        public int PreservesFoodAmount { get; set; }
    }
}
