using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class ArmorSetSkill : EquipmentSetSkill
    {
        private ArmorSet _armorSet => (ArmorSet)EquipmentSet;

        protected override void OnAwake()
        {

            base.OnAwake();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate) => GetInventoryService().HasItems(EquipmentSet.GetEquipSlots()) && !_inventoryService.IsArmorSetEquipped(_armorSet);

        protected override void QuickSlotUse()
        {
            GetInventoryService().TryEquipArmorSet(_armorSet);
        }
    }
}
