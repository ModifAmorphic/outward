using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.ActionUI.Extensions;
using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.ActionUI.Services
{

    public class HotbarProfileJsonService : IHotbarProfileService, IDisposable
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public string HotbarsConfigFile = "Hotbars.json";

        private ProfileService _profileService;

        private HotbarProfileData _hotbarProfile;
        private bool disposedValue;

        public event System.Action<IHotbarProfile, HotbarProfileChangeTypes> OnProfileChanged;

        public HotbarProfileJsonService(ProfileService profileService, Func<IModifLogger> getLogger)
        {
            (_profileService, _getLogger) = (profileService, getLogger);
            //profileService.OnActiveProfileChanged.AddListener((profile) => RefreshCachedProfile(profile));
            profileService.OnActiveProfileSwitched.AddListener((profile) => RefreshCachedProfile(profile, true));
        }

        private void RefreshCachedProfile(IActionUIProfile obj, bool suppressChangedEvent = false)
        {
            _hotbarProfile = GetProfileData();
            if (!suppressChangedEvent)
                OnProfileChanged?.TryInvoke(_hotbarProfile, HotbarProfileChangeTypes.ProfileRefreshed);
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
            var profileFile = Path.Combine(_profileService.GetActiveActionUIProfile().Path, HotbarsConfigFile);

            Logger.LogDebug($"Saving Hotbar profile to file '{profileFile}'.");

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
            OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.HotbarAdded);
            return GetProfile();
        }

        public IHotbarProfile RemoveHotbar()
        {
            if (GetProfile().Hotbars.Count > 1)
            {
                GetProfile().Hotbars.RemoveAt(GetProfile().Hotbars.Count - 1);
                Save();
                OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.HotbarRemoved);
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
            OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.RowAdded);
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
            OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.RowRemoved);
            return GetProfile();
        }

        public IHotbarProfile AddSlot()
        {
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
            OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.SlotAdded);
            return GetProfile();
        }

        public IHotbarProfile RemoveSlot()
        {
            if (GetProfile().SlotsPerRow <= 1)
                return GetProfile();

            for (int b = 0; b < GetProfile().Hotbars.Count; b++)
            {
                for (int r = GetProfile().Rows; r > 0; r--)
                {
                    int lastSlotInRow = r * GetProfile().SlotsPerRow - 1;
                    GetProfile().Hotbars[b].Slots.RemoveAt(lastSlotInRow);
                }
                ReindexSlots(GetProfile().Hotbars[b].Slots);
            }

            GetProfile().SlotsPerRow--;
            Save();
            OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.SlotRemoved);
            return GetProfile();
        }

        public IHotbarProfile SetCooldownTimer(bool showTimer, bool preciseTime)
        {
            bool profileChanged = false;

            foreach (var bar in GetProfile().Hotbars)
            {
                foreach (var slot in bar.Slots)
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
                OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.CooldownTimer);
            }

            return GetProfile();
        }
        public IHotbarProfile SetCombatMode(bool combatMode)
        {
            if (GetProfile().CombatMode != combatMode)
            {
                GetProfile().CombatMode = combatMode;
                Save();
                OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.CombatMode);
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
                OnProfileChanged?.TryInvoke(GetProfile(), HotbarProfileChangeTypes.EmptySlotView);
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
            string profileFile = Path.Combine(_profileService.GetActiveActionUIProfile().Path, HotbarsConfigFile);

            if (!File.Exists(profileFile))
            {
                Logger.LogDebug($"Profile file '{profileFile}' not found.");
                return null;
            }
            Logger.LogDebug($"Loading profile file '{profileFile}'.");
            string json = File.ReadAllText(profileFile);
            return JsonConvert.DeserializeObject<HotbarProfileData>(json);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _profileService.OnActiveProfileChanged.RemoveListener((profile) => RefreshCachedProfile(profile));
                    _profileService.OnActiveProfileSwitched.RemoveListener((profile) => RefreshCachedProfile(profile, true));
                }
                _hotbarProfile = null;
                _profileService = null;
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HotbarProfileJsonService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
