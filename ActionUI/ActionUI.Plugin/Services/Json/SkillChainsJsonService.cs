﻿using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.ActionUI.Services
{

    public class SkillChainsJsonService : JsonProfileService<SkillChainProfile>, ISkillChainService
    {
        protected override string FileName => "SkillChains.json";
        private SkillChainsService _skillChainsService => _getSkillChainsService.Invoke();
        private Func<SkillChainsService> _getSkillChainsService;
        private Func<SlotDataService> _getSlotDataService;
        private SlotDataService _slotData => _getSlotDataService.Invoke();

        private EventInfo _tmogEvent;
        private Delegate _tmogEventHandler;

        public event Action<SkillChain> OnNewChain;
        public event OnRenamedSkillChainDelegate OnRenamedChain;
        public event Action<SkillChain> OnDeletedChain;
        public event Action<SkillChainProfile> OnProfileChanged;

        internal SkillChainsJsonService(GlobalProfileService globalProfileService,
                                     ProfileService profileService,
                                     Func<SkillChainsService> getSkillChainsService,
                                     Func<SlotDataService> getSlotDataService,
                                     string characterUID,
                                     Func<IModifLogger> getLogger) : base(globalProfileService, profileService, characterUID, getLogger)
        {
            _getSkillChainsService = getSkillChainsService;
            _getSlotDataService = getSlotDataService;
            TransmorphicEventsEx.TryHookOnTransmogrified(this, OnTransmogrified, out _tmogEvent, out _tmogEventHandler);
        }

        protected override void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            base.RefreshCachedProfile(actionMenusProfile, suppressChangeEvent);
            OnProfileChanged?.TryInvoke(GetProfile());
        }

        public SkillChainProfile GetSkillChainProfile()
        {
            var profiles = GetProfile();
            return profiles;
        }

        public SkillChain GetSkillChain(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var profiles = GetProfile();
            var skillChain = profiles.SkillChains.FirstOrDefault(sc => sc.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return skillChain;
        }

        public SkillChain GetSkillChain(int itemID)
        {
            var profiles = GetProfile();
            var skillChain = profiles.SkillChains.FirstOrDefault(sc => sc.ItemID == itemID);
            return skillChain;
        }

        public void CreateSkillChain(string name)
        {
            SaveSkillChain(new SkillChain()
            {
                ItemID = this.GlobalProfileService.GetNextItemID(),
                Name = name
            });
        }

        public void RenameSkillChain(string existingName, string newName)
        {
            var skillChain = GetSkillChain(existingName);
            if (skillChain == null)
                throw new ArgumentException($"Skill Chain '{existingName}' not found.", nameof(existingName));

            skillChain.Name = newName;
            Save(skillChain);
            OnProfileChanged?.TryInvoke(GetProfile());
            try { OnRenamedChain?.Invoke(skillChain, existingName, newName); }
            catch (Exception ex) { Logger.LogException(ex); }
        }

        public void SaveSkillChain(SkillChain skillChain)
        {
            Logger.LogInfo($"Saving skill chain '{skillChain.Name}'.");
            Logger.LogDebug($"Skill chain '{skillChain.Name}' has {skillChain.ActionChain?.Count()} Actions. {skillChain.ActionChain?.Count(a => a.Value == null)} are null.");
            var chains = GetProfile().SkillChains;

            var removed = chains.RemoveAll(sc => sc.ItemID == skillChain.ItemID);

            chains.Add(skillChain);
            Save(skillChain);

            if (removed < 1)
                OnNewChain?.TryInvoke(skillChain);
        }

        private void Save(SkillChain skillChain)
        {
            GlobalProfileService.AddOrUpdateItemID(skillChain.ItemID, CharacterUID);
            Save();
            if (skillChain.ActionChain.Any())
                LearnSkillChain(skillChain);
        }

        public void DeleteSkillChain(int itemID)
        {
            var chains = GetProfile().SkillChains;
            var removedChain = GetSkillChain(itemID);
            var removed = chains.RemoveAll(s => s.ItemID == itemID);

            if (removed > 0)
            {
                GlobalProfileService.RemoveItemID(itemID);
                Save();
                ForgetSkillChain(itemID);
                OnDeletedChain?.Invoke(removedChain);
            }
        }

        private void LearnSkillChain(SkillChain skillChain)
        {
            Logger.LogDebug($"Learning Skill Chain '{skillChain.Name}'");
            _skillChainsService.AddOrUpdateChainedSkill(skillChain);
        }

        private void ForgetSkillChain(int itemID) => _skillChainsService.RemoveSkillChains(itemID);


        private void OnTransmogrified(int consumedItemID, string consumedItemUID, int transmogItemID, string transmogItemUID)
        {
            Logger.LogDebug($"SkillChainsJsonService::OnTransmogrified" +
                    $"(consumedItemID: {consumedItemID}, consumedItemUID: '{consumedItemUID}', transmogItemID: {transmogItemID}, transmogItemUID: '{transmogItemUID}')");
            var skillChains = GetProfile().SkillChains.ToList();
            foreach (var skillChain in skillChains)
            {
                bool saveSet = false;
                foreach (var chainAction in skillChain.ActionChain.Values)
                {

                    if (chainAction != null && chainAction.ItemID != 0 && !string.IsNullOrEmpty(chainAction.ItemUID))
                    {
                        if (chainAction.ItemID == consumedItemID && chainAction.ItemUID == consumedItemUID)
                        {
                            chainAction.ItemUID = transmogItemUID;
                            saveSet = true;
                            Logger.LogDebug($"Found and updated skill chain {skillChain.Name} ItemID {chainAction.ItemID}'s UID from '{consumedItemUID}' to '{transmogItemUID}'");
                        }
                    }
                }
                if (saveSet)
                {
                    SaveSkillChain(skillChain);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            TransmorphicEventsEx.TryUnhookTransmogrified(this, _tmogEvent, _tmogEventHandler);
            base.Dispose(disposing);
        }

        public List<ISlotAction> GetSlotActions(string chainName)
        {
            var actions = new List<ISlotAction>();
            var chain = GetSkillChain(chainName);

            foreach(var action in chain.ActionChain.Values)
            {
                if (_slotData.TryGetItemSlotAction(action.ItemID, action.ItemUID, false, out var slotAction))
                    actions.Add(slotAction);
            }

            return actions;
        }

        public ChainAction ConvertToChainAction(ISlotAction slotAction)
        {
            if (!(slotAction is IOutwardItem outwardItem))
                throw new ArgumentException($"Unexpected slot action provided. Expected type {nameof(IOutwardItem)}", nameof(slotAction));

            return new ChainAction()
            {
                ItemID = outwardItem.ActionItem.ItemID,
                ItemUID = outwardItem.ActionItem.UID
            };
        }
    }
}