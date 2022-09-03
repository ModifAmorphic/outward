using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using Newtonsoft.Json;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    
    public class HotbarProfileJsonService : IHotbarProfileService
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private const string KeyboardMapFileSuffix = "_KeyboardMap.xml";
        public string HotbarsConfigFile = "Hotbars.json";

        ProfileService _profileService;

        private HotbarProfileData _hotbarProfile;

        public UnityEvent<IHotbarProfile> OnProfileChanged { get; } = new UnityEvent<IHotbarProfile>();

        public HotbarProfileJsonService(ProfileService profileService, Func<IModifLogger> getLogger)
        {
            (_profileService, _getLogger) = (profileService, getLogger);
            profileService.OnActiveProfileChanged.AddListener(RefreshCachedProfile);
        }

        private void RefreshCachedProfile(IActionMenusProfile obj)
        {
            _hotbarProfile = GetProfileData();
            OnProfileChanged?.TryInvoke(_hotbarProfile);
        }

        public IHotbarProfile GetProfile()
        {
            if (_hotbarProfile != null)
                return _hotbarProfile;

            _hotbarProfile = GetProfileData();
            return _hotbarProfile;
        }

        public void Save() => Save(GetProfile());

        public void SaveNew(IHotbarProfile hotbarProfile)
        {
            Save(hotbarProfile);
            _hotbarProfile = null;
        }

        private void Save(IHotbarProfile hotbarProfile)
        {
            var json = JsonConvert.SerializeObject(hotbarProfile, Formatting.Indented);
            var profileFile = Path.Combine(_profileService.GetActiveActionMenusProfile().Path, HotbarsConfigFile);

            File.WriteAllText(profileFile, json);
        }

        public void Update(HotbarsContainer hotbar)
        {
            GetProfile().Rows = hotbar.Controller.GetRowCount();
            GetProfile().SlotsPerRow = hotbar.Controller.GetActionSlotsPerRow();
            GetProfile().Hotbars = hotbar.ToHotbarSlotData(GetProfile().Hotbars.Cast<HotbarData>().ToArray());

            Save();
        }

        public IHotbarProfile AddHotbar()
        {
            int barIndex = GetProfile().Hotbars.Last().HotbarIndex + 1;

            var profileClone = GetProfileData();

            var newBar = new HotbarData()
            {
                HotbarIndex = barIndex,
                RewiredActionId = RewiredConstants.ActionSlots.HotbarNavActions[barIndex].id,
                RewiredActionName = RewiredConstants.ActionSlots.HotbarNavActions[barIndex].name,
            };

            Logger.LogDebug($"Adding new hotbar with index of {newBar.HotbarIndex}");

            foreach (var slot in profileClone.Hotbars.First().Slots)
            {
                var slotData = slot as SlotData;
                slotData.ItemUID = null;
                slotData.ItemID = -1;
                newBar.Slots.Add(slotData);
                Logger.LogDebug($"Added slot to new hotbar {newBar.HotbarIndex}.  Slot config: \n\t" +
                    $"RewiredActionId: {((ActionConfig)slotData.Config).RewiredActionId}\n\t" +
                    $"RewiredActionName: {((ActionConfig)slotData.Config).RewiredActionName}\n\t");
            }

            GetProfile().Hotbars.Add(newBar);
            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
            return GetProfile();
        }

        public IHotbarProfile RemoveHotbar()
        {
            if (GetProfile().Hotbars.Count > 1)
            {
                GetProfile().Hotbars.RemoveAt(GetProfile().Hotbars.Count - 1);
                Save();
                OnProfileChanged?.TryInvoke(GetProfile());
            }
            return GetProfile();
        }

        public IHotbarProfile AddRow()
        {
            GetProfile().Rows = GetProfile().Rows + 1;

            for (int b = 0; b < GetProfile().Hotbars.Count; b++)
            {
                for (int s = 0; s < GetProfile().SlotsPerRow; s++)
                {
                    int slotIndex = s + GetProfile().SlotsPerRow * (GetProfile().Rows - 1);

                    GetProfile().Hotbars[b].Slots.Add(
                        CreateSlotDataFrom(GetProfile().Hotbars[b].Slots[s], slotIndex));
                }
            }

            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
            return GetProfile();
        }
        public IHotbarProfile RemoveRow()
        {
            if (GetProfile().Rows <= 1)
                return GetProfile();

            GetProfile().Rows--;

            for (int b = 0; b < GetProfile().Hotbars.Count; b++)
            {
                int removeFrom = GetProfile().SlotsPerRow * GetProfile().Rows;
                int removeAmount = GetProfile().Hotbars[b].Slots.Count - removeFrom;

                Logger.LogDebug($"Reducing hotbar {b}'s rows to {GetProfile().Rows}. Removing {removeAmount} slots starting with slot index {removeFrom}.\n" +
                    $"\tremoveFrom = {GetProfile().SlotsPerRow} * {GetProfile().Rows}\n" +
                    $"\tremoveAmount = {GetProfile().Hotbars[b].Slots.Count} - {removeFrom}");

                GetProfile().Hotbars[b].Slots.RemoveRange(removeFrom, removeAmount);
            }

            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
            return GetProfile();
        }

        public IHotbarProfile AddSlot()
        {

            //for (int b = 0; b < profile.Hotbars.Count; b++)
            //{
            //    int slotIndex = profile.Hotbars[b].Slots.Count;
            //    for (int r = 0; r < profile.Rows; r++)
            //    {
            //        profile.Hotbars[b].Slots.Add(
            //            CreateFrom(profile.Hotbars[b].Slots.Last(), slotIndex + r));
            //    }
                
            //}
            for (int b = 0; b < GetProfile().Hotbars.Count; b++)
            {
                int slotIndex = GetProfile().Hotbars[b].Slots.Count;
                GetProfile().Hotbars[b].Slots.Add(
                        CreateSlotDataFrom(GetProfile().Hotbars[b].Slots.First(), slotIndex));

                var lastIndex = slotIndex - GetProfile().SlotsPerRow;

                for (int s = lastIndex; s > 0; s = s - GetProfile().SlotsPerRow)
                {
                    GetProfile().Hotbars[b].Slots.Insert(s,
                        CreateSlotDataFrom(GetProfile().Hotbars[b].Slots.First(), s));
                }
                ReindexSlots(GetProfile().Hotbars[b].Slots);
            }

            GetProfile().SlotsPerRow++;
            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
            return GetProfile();
        }

        public IHotbarProfile RemoveSlot()
        {
            if (GetProfile().SlotsPerRow <= 1)
                return GetProfile();

            for (int b = 0; b < GetProfile().Hotbars.Count; b++)
            {
                var lastIndex = GetProfile().Hotbars[b].Slots.Count - 1;
                for (int s = lastIndex; s > 0; s = s - GetProfile().SlotsPerRow)
                {
                    GetProfile().Hotbars[b].Slots.RemoveAt(s);
                }
                ReindexSlots(GetProfile().Hotbars[b].Slots);
            }

            GetProfile().SlotsPerRow--;
            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
            return GetProfile();
        }

        public IHotbarProfile SetCooldownTimer(bool showTimer, bool preciseTime)
        {
            bool profileChanged = false;

            foreach(var bar in GetProfile().Hotbars)
            {
                foreach(var slot in bar.Slots)
                {
                    if (slot.Config.ShowCooldownTime != showTimer || slot.Config.PreciseCooldownTime != preciseTime)
                    {
                        profileChanged = true;
                        slot.Config.ShowCooldownTime = showTimer;
                        slot.Config.PreciseCooldownTime = preciseTime;
                    }
                }
            }

            if (profileChanged)
            {
                Save();
                OnProfileChanged?.TryInvoke(GetProfile());
            }
            
            return GetProfile();
        }
        public IHotbarProfile SetCombatMode(bool combatMode)
        {
            if (GetProfile().CombatMode != combatMode)
            {
                GetProfile().CombatMode = combatMode;
                Save();
                OnProfileChanged?.TryInvoke(GetProfile());
            }

            return GetProfile();
        }

        public IHotbarProfile SetEmptySlotView(EmptySlotOptions option)
        {
            bool profileChanged = false;

            foreach (var bar in GetProfile().Hotbars)
            {
                foreach (var slot in bar.Slots)
                {
                    if (slot.Config.EmptySlotOption != option)
                    {
                        profileChanged = true;
                        slot.Config.EmptySlotOption = option;
                    }
                }
            }

            if (profileChanged)
            {
                Save();
                OnProfileChanged?.TryInvoke(GetProfile());
            }
            return GetProfile();
        }

        private void ReindexSlots(List<ISlotData> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SlotIndex = i;
                ((ActionConfig)slots[i].Config).RewiredActionId = RewiredConstants.ActionSlots.Actions[i].id;
                ((ActionConfig)slots[i].Config).RewiredActionName = RewiredConstants.ActionSlots.Actions[i].name;
            }
        }

        private SlotData CreateSlotDataFrom(ISlotData source, int slotIndex)
        {
            var config = new ActionConfig()
            {
                EmptySlotOption = source.Config.EmptySlotOption,
                ShowZeroStackAmount = source.Config.ShowZeroStackAmount,
                PreciseCooldownTime = source.Config.PreciseCooldownTime,
                ShowCooldownTime = source.Config.ShowCooldownTime,
            };

            config.RewiredActionName = RewiredConstants.ActionSlots.Actions[slotIndex].name;
            config.RewiredActionId = RewiredConstants.ActionSlots.Actions[slotIndex].id;

            return new SlotData()
            {
                SlotIndex = slotIndex,
                Config = config,
                ItemID = -1,
                ItemUID = null
            };
        }

        private HotbarProfileData GetProfileData()
        {
            string profileFile = Path.Combine(_profileService.GetActiveActionMenusProfile().Path, HotbarsConfigFile);

            if (!File.Exists(profileFile))
            {
                Logger.LogDebug($"Profile file '{profileFile}' not found.");
                return null;
            }
            Logger.LogDebug($"Loading profile file '{profileFile}'.");
            string json = File.ReadAllText(profileFile);
            return JsonConvert.DeserializeObject<HotbarProfileData>(json);
        }
    }
}
