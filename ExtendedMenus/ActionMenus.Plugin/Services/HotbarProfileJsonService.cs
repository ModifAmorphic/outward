using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ModifAmorphic.Outward.ActionMenus.Services
{
    
    public class HotbarProfileJsonService : IHotbarProfileDataService
    {
        private readonly HotbarSettings _settings;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public string ProfilesPath { get; private set; }
        public string ActiveProfileFile => Path.Combine(ProfilesPath, "profile.json");
        private const string KeyboardMapFileSuffix = "_KeyboardMap.xml";
        public string HotbarsConfigFile = "Hotbars.json";

        private IHotbarProfileData _activeProfile;

        public event Action<IHotbarProfileData> OnActiveProfileChanged;

        public HotbarProfileJsonService(string profilesPath, HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            (_settings, _getLogger) = (settings, getLogger);
            ProfilesPath = profilesPath;
        }

        public IHotbarProfileData GetProfile(string name) => GetProfileData(name);

        public bool TryGetActiveProfileName(out string name)
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);
            name = string.Empty;
            if (File.Exists(ActiveProfileFile))
            {
                var json = File.ReadAllText(ActiveProfileFile);
                var activeProfile = JsonConvert.DeserializeObject<ActiveProfileData>(json);

                name = activeProfile.ActiveProfile;
            }
            return !string.IsNullOrEmpty(name);
        }

        public IHotbarProfileData GetActiveProfile()
        {
            if (TryGetActiveProfileName(out var name))
            {
                if (_activeProfile == null || !_activeProfile.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    _activeProfile = GetProfile(name);
                }
            }
            else
                _activeProfile = null;

            return _activeProfile;
        }

        public void SetActiveProfile(string name)
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            if (TryGetActiveProfileName(out var activeName) && name.Equals(activeName, StringComparison.InvariantCultureIgnoreCase))
                return;

            var activeProfile = new ActiveProfileData();
            if (File.Exists(ActiveProfileFile))
            {
                var currentJson = File.ReadAllText(ActiveProfileFile);
                activeProfile = JsonConvert.DeserializeObject<ActiveProfileData>(currentJson);
            }
            activeProfile.ActiveProfile = name;
            var newJson = JsonConvert.SerializeObject(activeProfile, Formatting.Indented);
            File.WriteAllText(ActiveProfileFile, newJson);
            
            OnActiveProfileChanged?.Invoke(GetActiveProfile());
        }

        public IEnumerable<string> GetProfileNames()
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            var dirs = Directory.GetDirectories(ProfilesPath);
            var names = new List<string>();
            foreach(var dir in dirs)
            {
                if (File.Exists(Path.Combine(dir, HotbarsConfigFile)))
                    names.Add(new DirectoryInfo(dir).Name);
            }

            return names;
        }

        public void SaveProfile(IHotbarProfileData profile)
        {
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            var profileData = ((ProfileData)profile);
            var profileFile = Path.Combine(GetOrAddProfileDir(profile.Name), HotbarsConfigFile);

            File.WriteAllText(profileFile, json);

            if (TryGetActiveProfileName(out var activeName) && profile.Name.Equals(activeName, StringComparison.InvariantCultureIgnoreCase))
            {
                _activeProfile = GetProfile(activeName);
            }
        }

        public void UpdateProfile(HotbarsContainer hotbar, IHotbarProfileData profile)
        {
            profile.Rows = hotbar.Controller.GetRowCount();
            profile.SlotsPerRow = hotbar.Controller.GetActionSlotsPerRow();
            profile.Hotbars = hotbar.ToHotbarSlotData(profile.Hotbars.Cast<HotbarData>().ToArray());

            SaveProfile(profile);
        }

        public IHotbarProfileData AddHotbar(IHotbarProfileData profile)
        {
            int barIndex = profile.Hotbars.Last().HotbarIndex + 1;

            var profileClone = GetProfileData(profile.Name);

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

            profile.Hotbars.Add(newBar);
            SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }

        public IHotbarProfileData RemoveHotbar(IHotbarProfileData profile)
        {
            if (profile.Hotbars.Count > 1)
            {
                profile.Hotbars.RemoveAt(profile.Hotbars.Count - 1);
                SaveProfile(profile);
                OnActiveProfileChanged?.Invoke(profile);
            }
            return profile;
        }

        public IHotbarProfileData AddRow(IHotbarProfileData profile)
        {
            profile.Rows = profile.Rows + 1;

            for (int b = 0; b < profile.Hotbars.Count; b++)
            {
                for (int s = 0; s < profile.SlotsPerRow; s++)
                {
                    int slotIndex = s + profile.SlotsPerRow * (profile.Rows - 1);

                    profile.Hotbars[b].Slots.Add(
                        CreateSlotDataFrom(profile.Hotbars[b].Slots[s], slotIndex));
                }
            }

            SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }
        public IHotbarProfileData RemoveRow(IHotbarProfileData profile)
        {
            if (profile.Rows <= 1)
                return profile;
            
            profile.Rows--;

            for (int b = 0; b < profile.Hotbars.Count; b++)
            {
                int removeFrom = profile.SlotsPerRow * profile.Rows;
                int removeAmount = profile.Hotbars[b].Slots.Count - removeFrom;

                Logger.LogDebug($"Reducing hotbar {b}'s rows to {profile.Rows}. Removing {removeAmount} slots starting with slot index {removeFrom}.\n" +
                    $"\tremoveFrom = {profile.SlotsPerRow} * {profile.Rows}\n" +
                    $"\tremoveAmount = {profile.Hotbars[b].Slots.Count} - {removeFrom}");

                profile.Hotbars[b].Slots.RemoveRange(removeFrom, removeAmount);
            }

            SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }

        public IHotbarProfileData AddSlot(IHotbarProfileData profile)
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
            for (int b = 0; b < profile.Hotbars.Count; b++)
            {
                int slotIndex = profile.Hotbars[b].Slots.Count;
                profile.Hotbars[b].Slots.Add(
                        CreateSlotDataFrom(profile.Hotbars[b].Slots.First(), slotIndex));

                var lastIndex = slotIndex - profile.SlotsPerRow;

                for (int s = lastIndex; s > 0; s = s - profile.SlotsPerRow)
                {
                    profile.Hotbars[b].Slots.Insert(s,
                        CreateSlotDataFrom(profile.Hotbars[b].Slots.First(), s));
                }
                ReindexSlots(profile.Hotbars[b].Slots);
            }

            profile.SlotsPerRow++;
            SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }

        public IHotbarProfileData RemoveSlot(IHotbarProfileData profile)
        {
            if (profile.SlotsPerRow <= 1)
                return profile;

            for (int b = 0; b < profile.Hotbars.Count; b++)
            {
                var lastIndex = profile.Hotbars[b].Slots.Count - 1;
                for (int s = lastIndex; s > 0; s = s - profile.SlotsPerRow)
                {
                    profile.Hotbars[b].Slots.RemoveAt(s);
                }
                ReindexSlots(profile.Hotbars[b].Slots);
            }

            profile.SlotsPerRow--;
            SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }

        public IHotbarProfileData SetCooldownTimer(IHotbarProfileData profile, bool showTimer, bool preciseTime)
        {
            bool profileChanged = false;

            foreach(var bar in profile.Hotbars)
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
                SaveProfile(profile);
                OnActiveProfileChanged?.Invoke(profile);
            }
            
            return profile;
        }
        public IHotbarProfileData SetCombatMode(IHotbarProfileData profile, bool combatMode)
        {
            if (profile.CombatMode != combatMode)
            {
                profile.CombatMode = combatMode;
                SaveProfile(profile);
                OnActiveProfileChanged?.Invoke(profile);
            }

            return profile;
        }

        public IHotbarProfileData SetEmptySlotView(IHotbarProfileData profile, EmptySlotOptions option)
        {
            bool profileChanged = false;

            foreach (var bar in profile.Hotbars)
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
                SaveProfile(profile);
                OnActiveProfileChanged?.Invoke(profile);
            }
            return profile;
        }

        public string GetOrAddProfileDir(string profileName)
        {
            string profileDir = Path.Combine(ProfilesPath, profileName);
            if (!Directory.Exists(profileDir))
                Directory.CreateDirectory(profileDir);

            return profileDir;
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

        private ProfileData GetProfileData(string name)
        {
            string profileFile = Path.Combine(GetOrAddProfileDir(name), HotbarsConfigFile);

            if (!File.Exists(profileFile))
            {
                Logger.LogDebug($"Profile file '{profileFile}' not found.");
                return null;
            }
            Logger.LogDebug($"Loading profile file '{profileFile}'.");
            string json = File.ReadAllText(profileFile);
            return JsonConvert.DeserializeObject<ProfileData>(json);
        }
    }
}
