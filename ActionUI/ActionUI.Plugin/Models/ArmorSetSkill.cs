using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class ArmorSetSkill : EquipmentSetSkill
    {
        private ArmorSet _armorSet
        {
            get
            {
                if (TryGetEquipmentSet(out var equipmentSet))
                    return (ArmorSet)equipmentSet;
                return null;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate)
        {
            if (_armorSet == null)
                return false;

            if (TryGetEquipService(out var inventoryService) && TryGetEquipmentSet(out var equipmentSet))
                return inventoryService.HasItems(_armorSet.GetEquipSlots()) && !inventoryService.IsArmorSetEquipped(_armorSet);

            return false;
        }

        protected override void QuickSlotUse()
        {
            if (_armorSet == null)
                return;

            if (TryGetEquipService(out var inventoryService))
                inventoryService.TryEquipArmorSet(_armorSet);
        }

        protected override bool TryGetEquipmentSet(out IEquipmentSet equipmentSet)
        {
            equipmentSet = null;
            if (this.m_ownerCharacter == null)
                return false;

            if (Psp.Instance.TryGetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID, out var usp))
            {
                if (usp.TryGetService<IEquipmentSetService<ArmorSet>>(out var equipService))
                {
                    equipmentSet = equipService.GetEquipmentSet(_setName);
                    return equipmentSet != null;
                }
            }
            return false;
        }
    }
}
