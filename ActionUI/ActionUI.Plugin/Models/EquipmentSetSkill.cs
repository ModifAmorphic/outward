using ModifAmorphic.Outward.ActionUI.Extensions;
using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Linq;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    public abstract class EquipmentSetSkill : Skill
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        [SerializeField]
        protected string _setName;
        [SerializeField]
        protected int _setID;
        [SerializeField]
        protected EquipSlots _iconSlot;

        protected InventoryService _inventoryService;

        //public Character Character { get; set; }
        public ItemDisplay ItemDisplay => m_refItemDisplay;

        protected override void OnAwake()
        {
            this.SetPrivateField<Skill, Skill.ActivationCondition[]>("m_additionalConditions", new Skill.ActivationCondition[0]);
            HasDynamicQuickSlotIcon = true;
            Logger.LogDebug("EquipmentSetSkill::OnAwake: " + m_name);
            base.OnAwake();

            SetIcon();
        }

        protected override void ChangeOwner(Character _newOwner)
        {
            base.ChangeOwner(_newOwner);
            if (_newOwner == null)
                return;

            SetIcon();
        }

        public void SetEquipmentSet(IEquipmentSet set, Character character)
        {
            _setName = set.Name;
            m_name = set.Name;
            _setID = set.SetID;
            ItemID = set.SetID;
            _iconSlot = set.IconSlot;
            RequiredItems = set.ToItemsRequired().ToArray();

            //if (transform.parent != character.Inventory.SkillKnowledge.transform)
            //{
            //    transform.SetParent(character.Inventory.SkillKnowledge.transform);
            //    ForceUpdateParentChange();
            //}

            SetIcon();
        }

        public void SetParent(Transform parentTransform)
        {
            if (transform.parent != parentTransform)
            {
                transform.SetParent(parentTransform);
                ForceUpdateParentChange();
            }
        }

        private void SetIcon()
        {
            var iconItemPrefab = GetIconOwnerPrefab();
            m_itemIcon = iconItemPrefab?.ItemIcon ?? ActionMenuResources.Instance.SpriteResources["EmptySlotIcon"];
            ItemIconPath = iconItemPrefab?.ItemIconPath;
            m_refItemDisplay?.RefreshIcon();
        }

        protected override bool OwnerHasAllRequiredItems(bool _tryingToActivate) => GetInventoryService().HasItems(GetEquipmentSet().GetEquipSlots());

        protected Item GetIconOwnerPrefab()
        {
            var iconSlot = GetEquipmentSet()?.GetIconEquipSlot();
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

        protected abstract IEquipmentSet GetEquipmentSet();

    }
}