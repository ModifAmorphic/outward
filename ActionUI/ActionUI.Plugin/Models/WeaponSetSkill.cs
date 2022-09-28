using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class WeaponSetSkill : EquipmentSetSkill
    {
        private WeaponSet _weaponSet => (WeaponSet)EquipmentSet;

        protected override void OnAwake()
        {

            base.OnAwake();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate) => GetInventoryService().HasItems(EquipmentSet.GetEquipSlots()) && !_inventoryService.IsWeaponSetEquipped(_weaponSet);

        protected override void QuickSlotUse()
        {
            GetInventoryService().TryEquipWeaponSet(_weaponSet);
        }
    }
}
