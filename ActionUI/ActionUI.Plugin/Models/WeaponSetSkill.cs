using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class WeaponSetSkill : EquipmentSetSkill
    {
        private WeaponSet _weaponSet => (WeaponSet)GetEquipmentSet();

        protected override void OnAwake()
        {

            base.OnAwake();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate) => _weaponSet != null && GetInventoryService().HasItems(_weaponSet.GetEquipSlots()) && !GetInventoryService().IsWeaponSetEquipped(_weaponSet);

        protected override void QuickSlotUse()
        {
            GetInventoryService().TryEquipWeaponSet(_weaponSet);
        }

        protected override IEquipmentSet GetEquipmentSet()
        {
            if (this.m_ownerCharacter == null)
                return null;

            var equipService = Psp.Instance.GetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID).GetService<IEquipmentSetService<WeaponSet>>();
            return equipService.GetEquipmentSet(_setName);
        }
    }
}
