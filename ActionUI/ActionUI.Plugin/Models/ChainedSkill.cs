using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal enum ItemTypes
    {
        Item,
        Armor,
        Weapon,
        Skill
    }
    internal class ChainedSkill : Skill
    {
        [SerializeField]
        public ChainAction[] ChainSteps;

        private int _step;
        private Item _stepItem;
        private Skill _skill;
        private Armor _armor;
        private Weapon _weapon;

        private ItemTypes _stepItemType;

        public override Sprite QuickSlotIcon => _stepItem.QuickSlotIcon;

        protected override void OnAwake()
        {
            HasDynamicQuickSlotIcon = true;
            base.OnAwake();
            _step = -1;
            SetNextChainItem();
        }

        public void SetChain(string name, int itemID, IEnumerable<ChainAction> sortedSteps)
        {
            m_name = name;
            ItemID = itemID;
            ChainSteps = sortedSteps.ToArray();
        }
        public int GetCurrentStepNo() => _step;
        public ChainAction GetCurrentStepIds() => ChainSteps[_step];
        public void SetNextStepNo() => _step = _step + 1 < ChainSteps.Length ? _step + 1 : 0;

        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);


        protected override void QuickSlotUse()
        {
            if (_stepItem == null || _stepItem.IsPrefab)
            {
                m_ownerCharacter.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(_stepItem.Name, Global.HIGHLIGHT)));
                return;
            }

            _stepItem.TryQuickSlotUse();
            ScheduleSetNextStep();
        }

        protected void ScheduleSetNextStep()
        {
            if (_stepItemType == ItemTypes.Skill)
            {
                StartCoroutine(SetNextAfterCooldown(_skill));
            }
            else if (_stepItemType == ItemTypes.Weapon)
            {
                StartCoroutine(SetNextAfterEquipped(_weapon));
            }
            else
            {
                SetNextChainItem();
            }
        }

        private void SetNextChainItem()
        {
            if (!TryGetNextStep(out var item))
                return;

            _stepItem = item;
            CastStepItem();
        }

        protected bool TryGetNextStep(out Item item)
        {
            item = null;
            if (!TryGetSlotData(out var slotData))
                return false;

            SetNextStepNo();
            var ids = GetCurrentStepIds();
            if (!slotData.TryFindOwnedItem(ids.ItemID, ids.ItemUID, out item))
            {
                Logger.LogDebug($"Did not find an owned item with ItemID {ids.ItemID} for character {m_ownerCharacter?.UID}.");
                if (!slotData.TryFindPrefab(ids.ItemID, out item))
                {
                    Logger.LogWarning($"Invalid Skill Chain. No prefab found for ItemID {ids.ItemID}.");
                    return false;
                }
            }
            return true;
        }

        protected bool TryGetSlotData(out SlotDataService slotData)
        {
            slotData = null;
            if (Psp.Instance.TryGetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID, out var usp))
                return usp.TryGetService<SlotDataService>(out slotData);

            return false;
        }

        private void CastStepItem()
        {
            _skill = null;
            _armor = null;
            _weapon = null;

            if (_stepItem is Skill)
            {
                _skill = (Skill)_stepItem;
                _stepItemType = ItemTypes.Skill;
            }
            else if (_stepItem is Armor)
            {
                _armor = (Armor)_stepItem;
                _stepItemType = ItemTypes.Armor;
            }
            else if (_stepItem is Weapon)
            {
                _weapon = (Weapon)_stepItem;
                _stepItemType = ItemTypes.Weapon;
            }
            else
            {
                _stepItemType = ItemTypes.Item;
            }
        }

        private IEnumerator SetNextAfterCooldown(Skill skill)
        {
            while (skill.InCooldown())
            {
                yield return new WaitForSeconds(skill.GetCooldownSecondsRemaining());
            }
            SetNextChainItem();
        }

        private IEnumerator SetNextAfterEquipped(Weapon weapon)
        {
            while (m_ownerCharacter != null && m_ownerCharacter.IsHandlingWeapon)
            {
                yield return null;
            }

            if (m_ownerCharacter != null)
                SetNextChainItem();
        }

    }

}
