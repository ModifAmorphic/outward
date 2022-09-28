using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.ActionUI.Extensions;
using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using System.Linq;
using UnityEngine;

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
