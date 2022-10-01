namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IActionUIProfile
    {
        string Name { get; set; }
        bool ActionSlotsEnabled { get; set; }
        bool DurabilityDisplayEnabled { get; set; }
        bool EquipmentSetsEnabled { get; set; }
        bool StashCraftingEnabled { get; set; }
        bool CraftingOutsideTownEnabled { get; set; }
        EquipmentSetsSettingsProfile EquipmentSetsSettingsProfile { get; set; }
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
}
