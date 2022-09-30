using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal class ArmorSetSkill : EquipmentSetSkill
    {
        private ArmorSet _armorSet => (ArmorSet)GetEquipmentSet();

        protected override void OnAwake()
        {

            base.OnAwake();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate) => _armorSet != null && GetInventoryService().HasItems(_armorSet.GetEquipSlots()) && !GetInventoryService().IsArmorSetEquipped(_armorSet);

        protected override void QuickSlotUse()
        {
            GetInventoryService().TryEquipArmorSet(_armorSet);
        }
        protected override IEquipmentSet GetEquipmentSet()
        {
            if (this.m_ownerCharacter == null)
                return null;

            var equipService = Psp.Instance.GetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID).GetService<IEquipmentSetService<ArmorSet>>();
            return equipService.GetEquipmentSet(_setName);
        }
    }
}
