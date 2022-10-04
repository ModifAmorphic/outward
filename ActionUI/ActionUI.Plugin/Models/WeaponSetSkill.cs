using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class WeaponSetSkill : EquipmentSetSkill
    {
        private WeaponSet _weaponSet
        {
            get
            {
                if (TryGetEquipmentSet(out var weaponSet))
                    return (WeaponSet)weaponSet;
                return null;
            }
        }

        protected override void OnAwake()
        {

            base.OnAwake();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate)
        {
            if (_weaponSet == null)
                return false;

            if (TryGetEquipService(out var equipService) && TryGetEquipmentSet(out var equipmentSet))
                return equipService.HasItems(_weaponSet.GetEquipSlots()) && !equipService.IsWeaponSetEquipped(_weaponSet);

            return false;
        }

        protected override void QuickSlotUse()
        {
            if (_weaponSet == null)
                return;

            if (TryGetEquipService(out var inventoryService))
                inventoryService.TryEquipWeaponSet(_weaponSet);
        }

        protected override bool TryGetEquipmentSet(out IEquipmentSet equipmentSet)
        {
            equipmentSet = null;
            if (this.m_ownerCharacter == null)
                return false;

            if (Psp.Instance.TryGetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID, out var usp))
            {
                if (usp.TryGetService<IEquipmentSetService<WeaponSet>>(out var equipService))
                {
                    equipmentSet = equipService.GetEquipmentSet(_setName);
                    return equipmentSet != null;
                }
            }
            return false;
        }
    }
}
