using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.ActionMenus.Models;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public event Action<IHotbarProfileData> OnActiveProfileChanged;

        public HotbarProfileJsonService(string profilesPath, HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            (_settings, _getLogger) = (settings, getLogger);
            ProfilesPath = profilesPath;
        }
        public IHotbarProfileData Create(string name, HotbarsContainer hotbarsContainer)
        {
            var profileData = hotbarsContainer.ToProfileData(name);
            
            var json = JsonConvert.SerializeObject(profileData, Formatting.Indented);

            //var hotbarAssigns = hotbarsContainer.ToHotbarData().Select(h => (IHotbarSlotData<ISlotData>)h);
            //var profile = new HotbarProfileData()
            //{
            //    Name = name,
            //    Rows = hotbarsContainer.Controller.GetRowCount(),
            //    SlotsPerRow = hotbarsContainer.Controller.GetActionSlotsPerBar(),
            //    Hotbars = hotbarAssigns.ToList()
            //};
            //var json = JsonConvert.SerializeObject(profile);
            File.WriteAllText(Path.Combine(GetProfileDir(name), HotbarsConfigFile), json);

            OnActiveProfileChanged?.Invoke(profileData);

            return profileData;
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
                return GetProfile(name);
            }
            return null;
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
            File.WriteAllText(Path.Combine(GetProfileDir(profile.Name), HotbarsConfigFile), json);
        }

        //public IHotbarProfileData AddHotbar(string name) => AddHotbar(GetProfileData(name));

        public IHotbarProfileData AddHotbar(IHotbarProfileData profile)
        {
            int lastIndex = profile.Hotbars.Last().HotbarIndex;

            var profileClone = GetProfileData(profile.Name);

            var newBar = new HotbarSlotData()
            {
                HotbarIndex = lastIndex + 1
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
                //newBar.SlotsAssigned.Add(new SlotData()
                //{
                //    RewiredAction = slotData.RewiredAction,
                //    RewiredActionId = slotData.RewiredActionId,
                //    Config = slot.Config,
                //    SlotIndex = slot.SlotIndex
                //}
                //);
            }

            profile.Hotbars.Add(newBar);
            //SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }

        public IHotbarProfileData RemoveHotbar(IHotbarProfileData profile)
        {
            if (profile.Hotbars.Count > 1)
                profile.Hotbars.RemoveAt(profile.Hotbars.Count - 1);

            //SaveProfile(profile);
            OnActiveProfileChanged?.Invoke(profile);
            return profile;
        }

        private string GetProfileDir(string profileName)
        {
            string profileDir = Path.Combine(ProfilesPath, profileName);
            if (!Directory.Exists(profileDir))
                Directory.CreateDirectory(profileDir);

            return profileDir;
        }

        private ProfileData GetProfileData(string name)
        {
            string profileFile = Path.Combine(GetProfileDir(name), HotbarsConfigFile);

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
