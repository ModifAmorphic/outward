using ModifAmorphic.Outward.ActionUI.Extensions;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    public class EquipmentSetSkill : Skill
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public IEquipmentSet EquipmentSet { get; protected set; }

        protected InventoryService _inventoryService;
        public Character Character { get; set; }
        public ItemDisplay ItemDisplay => m_refItemDisplay;

        protected override void OnAwake()
        {
            m_name = EquipmentSet.Name;
            this.SetPrivateField<Skill, Skill.ActivationCondition[]>("m_additionalConditions", new Skill.ActivationCondition[0]);
            HasDynamicQuickSlotIcon = true;
            RequiredItems = EquipmentSet.ToItemsRequired().ToArray();
            Logger.LogDebug("EquipmentSetSkill::OnAwake: " + m_name);

            base.OnAwake();

            transform.SetParent(Character.Inventory.SkillKnowledge.transform);
            ForceUpdateParentChange();

        }

        public void SetEquipmentSet(IEquipmentSet set)
        {
            EquipmentSet = set;
            var iconItemPrefab = GetIconOwnerPrefab();
            m_itemIcon = iconItemPrefab?.ItemIcon ?? ActionMenuResources.Instance.SpriteResources["EmptySlotIcon"];
            ItemIconPath = iconItemPrefab?.ItemIconPath;

            m_refItemDisplay?.RefreshIcon();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate) => GetInventoryService().HasItems(EquipmentSet.GetEquipSlots());

        protected Item GetIconOwnerPrefab()
        {
            var iconSlot = EquipmentSet.GetIconEquipSlot();
            if (iconSlot == null)
                return null;

            return ResourcesPrefabManager.Instance.GetItemPrefab(iconSlot.ItemID);
        }

        protected InventoryService GetInventoryService()
        {
            if (_inventoryService == null)
                _inventoryService = Psp.Instance.GetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID).GetService<InventoryService>();

            return _inventoryService;
        }
    }
}