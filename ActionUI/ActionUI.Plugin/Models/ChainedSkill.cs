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

    internal enum IconTypes
    {
        StatusEffect,
        Item
    }

    internal class ChainedSkill : Skill
    {
        public ChainAction[] ChainSteps;

        public int[] StepIDs;
        public string[] StepUIDs;

        public string StatusEffectIcon;
        public int IconItemID;

        private int _step;
        private Item _stepItem;
        private Skill _skill;
        private Armor _armor;
        private Weapon _weapon;

        private ItemTypes _stepItemType;
        
        public IconTypes IconType;

        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public override Sprite QuickSlotIcon => _step != 0 || (_skill != null && _skill.InCooldown()) ? _stepItem?.QuickSlotIcon?? m_itemIcon : m_itemIcon;

        protected override void OnAwake()
        {
            HasDynamicQuickSlotIcon = true;
            RefreshChainSteps();

            if (IconType == IconTypes.StatusEffect)
            {
                m_itemIcon = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(StatusEffectIcon).StatusIcon;
                ItemIconPath = "Assets/Texture2D/tex_men_runicProtection.png";
            }
            else if (IconType == IconTypes.Item)
            {
                var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(IconItemID);
                m_itemIcon = prefab.ItemIcon;
                if (prefab is Equipment equipment && equipment.SummonedEquipment != null)
                    m_itemIcon = equipment.SummonedEquipment.StatusIcon;
                ItemIconPath = prefab.ItemIconPath;
            }

            base.OnAwake();
            _step = -1;
            if (m_ownerCharacter != null)
                SetNextChainItem();

        }

        public void SetChain(string name, int itemID, string statusEffect, IEnumerable<ChainAction> sortedSteps)
        {
            m_name = name;
            ItemID = itemID;
            IconType = IconTypes.StatusEffect;
            StatusEffectIcon = statusEffect;
            m_itemIcon = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(statusEffect).StatusIcon;
            ItemIconPath = "Assets/Texture2D/tex_men_runicProtection.png";
            m_refItemDisplay?.RefreshIcon();
            StepIDs = sortedSteps.Select(s => s.ItemID).ToArray();
            StepUIDs = sortedSteps.Select(s => s.ItemUID).ToArray();
            RefreshChainSteps();
        }

        public void SetChain(string name, int itemID, int iconItemID, IEnumerable<ChainAction> sortedSteps)
        {
            m_name = name;
            ItemID = itemID;
            IconItemID = iconItemID;
            StatusEffectIcon = string.Empty;
            IconType = IconTypes.Item;
            m_refItemDisplay?.RefreshIcon();
            StepIDs = sortedSteps.Select(s => s.ItemID).ToArray();
            StepUIDs = sortedSteps.Select(s => s.ItemUID).ToArray();
            RefreshChainSteps();
        }

        public int GetCurrentStepNo() => _step;
        public ChainAction GetCurrentStepIds() => ChainSteps[_step];
        public void SetNextStepNo() => _step = _step + 1 < ChainSteps.Length ? _step + 1 : 0;

        protected override void ChangeOwner(Character _newOwner)
        {
            var existingOwner = m_ownerCharacter;
            base.ChangeOwner(_newOwner);
            Logger.LogDebug($"{nameof(ChainedSkill)}::ChangeOwner from to '{m_ownerCharacter?.UID}' to '{_newOwner.UID}'.");
            if (m_ownerCharacter != null)
                StartCoroutine(SetNextAfterSkillsLearned());
        }

        protected override void QuickSlotUse()
        {
            if (_stepItem == null || _stepItem.IsPrefab)
            {
                m_ownerCharacter.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Quickslot_NoItemInventory", Global.SetTextColor(_stepItem.Name, Global.HIGHLIGHT)));
                return;
            }

            _stepItem.TryQuickSlotUse();
            //if (_stepItemType == ItemTypes.Skill)
            //{
            //    this.Cooldown = _skill.Cooldown;
            //    this.InvokePrivateMethod("StartCooldown");

            //}
            ScheduleSetNextStep();
        }

        protected override void UpdateProcessing()
        {
            base.UpdateProcessing();
            if (_stepItemType != ItemTypes.Skill || _skill == null)
            {
                
                return;
            }
            //Skips Skill.UpdateProcessing and invokes Item.UpdateProcessing
            //Logger.LogTrace($"InvokePrivateParentMethod for skill {this.name}, step skill{_skill.name}");
            //((Skill)this).InvokePrivateParentMethod<Skill, Item>("UpdateProcessing");

            if (this.QuickSlotIndex != -1 && this.m_ownerCharacter != null && this.m_ownerCharacter.IsLocalPlayer
                && _skill.QuickSlotIndex != -1 && _skill.OwnerCharacter != null && _skill.OwnerCharacter.IsLocalPlayer)
            {
                this.SetPrivateField<Skill, bool>("m_quickSlotHasBaseReq", _skill.HasBaseRequirements(false));
                this.SetPrivateField<Skill, bool>("m_quickSlotHasAddReq", _skill.HasAllAdditionalConditions(false));
            }

            var lastCastCharacter = _skill.GetPrivateField<Skill, Character>("m_lastCastCharacter");
            this.SetPrivateField<Skill, Character>("m_lastCastCharacter", lastCastCharacter);
            var inProgress = _skill.GetPrivateField<Skill, bool>("m_inProgress");
            this.SetPrivateField<Skill, bool>("m_inProgress", inProgress);
            var lastActivationTime = _skill.GetPrivateField<Skill, float>("m_lastActivationTime");
            this.SetPrivateField<Skill, float>("m_lastActivationTime", lastActivationTime);
            
            if (lastCastCharacter != null && lastCastCharacter.IsLocalPlayer && inProgress && Time.time - lastActivationTime >= _skill.Lifespan)
            {
                //_skill.InvokePrivateMethod("StartCooldown");
                this.InvokePrivateMethod("StartCooldown");
                if (_skill.PlayEndAnim)
                {
                    //_skill.InvokePrivateMethod("StopEffectCast", lastCastCharacter, true);
                    this.InvokePrivateMethod("StopEffectCast", lastCastCharacter, true);
                }
            }
            if (!NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                //_skill.InvokePrivateMethod("UpdateCooldownRatio");
                UpdateCooldownRatio(_skill);
            }
            else
                this.SetPrivateField<Skill, float>("m_lastUpdateCooldownTime", -999f);
        }

        private void UpdateCooldownRatio(Skill skill)
        {
            //skill.InvokePrivateMethod("ConvertCooldownProgress");
            var inProgress = skill.GetPrivateField<Skill, bool>("m_inProgress");
            this.SetPrivateField<Skill, bool>("m_inProgress", inProgress);
            var remainingCooldownTime = skill.GetPrivateField<Skill, float>("m_remainingCooldownTime");
            this.SetPrivateField<Skill, float>("m_remainingCooldownTime", remainingCooldownTime);

            if (inProgress || remainingCooldownTime <= 0.0)
                return;

            var lastUpdateCooldownTime = skill.GetPrivateField<Skill, float>("m_lastUpdateCooldownTime");
            this.SetPrivateField<Skill, float>("m_lastUpdateCooldownTime", lastUpdateCooldownTime);

            if (lastUpdateCooldownTime != -999.0)
            {
                remainingCooldownTime -= Time.time - lastUpdateCooldownTime;
                if (remainingCooldownTime < 0.0)
                    remainingCooldownTime = 0.0f;

                this.SetPrivateField<Skill, float>("m_remainingCooldownTime", remainingCooldownTime);
            }
            this.SetPrivateField<Skill, float>("m_lastUpdateCooldownTime", Time.time);
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

            Logger.LogDebug($"{nameof(ChainedSkill)}::SetNextChainItem() set next chain item to {item?.name}.");
        }

        protected bool TryGetNextStep(out Item item)
        {
            item = null;
            if (!TryGetSlotData(out var slotData))
            {
                Logger.LogDebug($"Could not find SlotDataService for PlayerID {this.m_ownerCharacter.OwnerPlayerSys.PlayerID}.");
                return false;
            }

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
            item.SetQuickSlot(1);
            Logger.LogDebug($"{nameof(ChainedSkill)}::TryGetNextStep: Got next step item {item?.name}.");
            return true;
        }

        protected bool TryGetSlotData(out SlotDataService slotData)
        {
            slotData = null;
            if (Psp.Instance.TryGetServicesProvider(this.m_ownerCharacter.OwnerPlayerSys.PlayerID, out var usp))
                return usp.TryGetService<SlotDataService>(out slotData);

            return false;
        }

        private void RefreshChainSteps()
        {
            ChainSteps = new ChainAction[StepIDs.Length];
            for (int i = 0; i < ChainSteps.Length; i++)
            {
                ChainSteps[i] = new ChainAction()
                {
                    ItemID = StepIDs[i],
                    ItemUID = StepUIDs[i],
                };
            }
        }

        private void CastStepItem()
        {
            _skill = null;
            _armor = null;
            _weapon = null;

            this.Lifespan = 0f;
            this.Cooldown = 0f;

            if (_stepItem is Skill)
            {
                _skill = (Skill)_stepItem;
                _stepItemType = ItemTypes.Skill;
                this.Lifespan = _skill.Lifespan;
                this.Cooldown = _skill.Cooldown;
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

        private IEnumerator SetNextAfterSkillsLearned()
        {
            while (m_ownerCharacter.Inventory?.SkillKnowledge == null)
            {
                yield return null;
            }
            SetNextChainItem();
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
