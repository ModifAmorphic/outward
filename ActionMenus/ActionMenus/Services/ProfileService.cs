using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    public class ProfileService : IActionMenusProfileService
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        ActionMenusProfile _activeProfile;


        public string ProfilesPath { get; private set; }
        public string ProfilesFile => Path.Combine(ProfilesPath, "profile.json");

        public UnityEvent<IActionMenusProfile> OnNewProfile { get; } = new UnityEvent<IActionMenusProfile>();
        public UnityEvent<IActionMenusProfile> OnActiveProfileChanged { get; } = new UnityEvent<IActionMenusProfile>();

        public ProfileService(string profilesRootPath, Func<IModifLogger> getLogger) => (ProfilesPath, _getLogger) = (profilesRootPath, getLogger);

        public IActionMenusProfile GetActiveProfile() => GetActiveActionMenusProfile();

        public ActionMenusProfile GetActiveActionMenusProfile()
        {
            if (_activeProfile != null)
                return _activeProfile;

            var profiles = GetOrCreateProfiles();
            if (profiles.Profiles.Any())
            {
                _activeProfile = profiles.Profiles.FirstOrDefault(p => p.Name.Equals(profiles.ActiveProfile, StringComparison.OrdinalIgnoreCase));
                if (_activeProfile == default)
                {
                    _activeProfile = profiles.Profiles.First();
                }
                _activeProfile.Path = Path.Combine(ProfilesPath, _activeProfile.Name);
            }

            return _activeProfile;
        }

        public IEnumerable<string> GetProfileNames()
        {
            var profiles = GetOrCreateProfiles();
            return profiles.Profiles.Select(p => p.Name);
        }

        public void SetActiveProfile(string name)
        {
            if (_activeProfile != null && _activeProfile.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return;

            Logger.LogInfo($"Setting active profile to profile '{name}'.");
            var profiles = GetOrCreateProfiles();
            if (profiles.ActiveProfile.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return;
            
            var profile = profiles.Profiles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (profile != null)
                profile.Name = name;
            else
            {
                profile = new ActionMenusProfile()
                {
                    Name = name,
                    ActionSlotsEnabled = true,
                    DurabilityDisplayEnabled = true,
                    Path = Path.Combine(ProfilesPath, name),
                };
                profiles.Profiles.Add(profile);
            }
            
            profiles.ActiveProfile = name;
            
            SaveProfiles(profiles);

            OnActiveProfileChanged.TryInvoke(GetActiveActionMenusProfile());
        }

        public string GetOrAddProfileDir(string profileName)
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            string profileDir = Path.Combine(ProfilesPath, profileName);
            if (!Directory.Exists(profileDir))
                Directory.CreateDirectory(profileDir);

            return profileDir;
        }

        public void Save() => SaveProfile(GetActiveActionMenusProfile());

        public void SaveNew(IActionMenusProfile profile)
        {
            SaveProfile(profile, false);
            OnNewProfile.TryInvoke(profile);
        }

        public void SaveProfile(IActionMenusProfile profile, bool raiseEvent = true)
        {
            var profiles = GetOrCreateProfiles();
            _ = GetOrAddProfileDir(profile.Name);
            var profileIndex = profiles.Profiles.FindIndex(p => p.Name.Equals(profile.Name, StringComparison.InvariantCultureIgnoreCase));
            if (profileIndex == -1)
            {
                profiles.Profiles.Add((ActionMenusProfile)profile);
            }
            else
            {
                profiles.Profiles[profileIndex] = (ActionMenusProfile)profile;
            }
            profiles.ActiveProfile = profile.Name;
            SaveProfiles(profiles);
            Logger.LogDebug($"Saved profile '{profile.Name}'.");
            _activeProfile = null;

            if (raiseEvent)
                OnActiveProfileChanged.TryInvoke(GetActiveActionMenusProfile());
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentNullException(nameof(newName));


            var profile = GetActiveActionMenusProfile();

            Logger.LogInfo($"Renaming profile from {profile.Name} to {newName}");
            profile.Name = newName;

            Logger.LogDebug($"Profile directory '{profile.Path}' {(Directory.Exists(profile.Path) ? "" : "does not ")}exist.");
            //If renaming an unsaved profile, then just save it with the new name.
            if (!Directory.Exists(profile.Path))
            {
                SaveProfile(profile);
                
                return;
            }
            string newDir = Path.Combine(ProfilesPath, newName);
            Logger.LogDebug($"Moving profile folder from '{profile.Path}' to '{newDir}'.");
            Directory.Move(profile.Path, newDir);
            SaveProfile(profile);

        }

        private List<string> GetProfileDirectories()
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            var dirs = Directory.GetDirectories(ProfilesPath);
            var names = new List<string>();
            foreach (var dir in dirs)
            {
                names.Add(new DirectoryInfo(dir).Name);
            }
            return names;
        }

        private ActionMenuProfiles GetOrCreateProfiles()
        {
            var profileNames = GetProfileDirectories();
            bool saveNeeded = false;

            ActionMenuProfiles profiles;
            if (File.Exists(ProfilesFile))
            {
                var json = File.ReadAllText(ProfilesFile);
                profiles = JsonConvert.DeserializeObject<ActionMenuProfiles>(json);
                var removeProfiles = new List<int>();
                for (int i = 0; i < profiles.Profiles.Count; i++)
                {
                    if (!profileNames.Any(n => n.Equals(profiles.Profiles[i].Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        removeProfiles.Add(i);
                        saveNeeded = true;
                    }
                }
                foreach(var i in removeProfiles)
                    profiles.Profiles.RemoveAt(i);
            }
            else
                profiles = new ActionMenuProfiles();

            foreach (var name in profileNames)
            {
                if (!profiles.Profiles.Any(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    profiles.Profiles.Add(new ActionMenusProfile()
                    {
                        ActionSlotsEnabled = true,
                        DurabilityDisplayEnabled = true,
                        Name = name,
                    });
                    saveNeeded = true;
                }
            }

            if (string.IsNullOrEmpty(profiles.ActiveProfile) && profiles.Profiles.Any())
                profiles.ActiveProfile = profiles.Profiles.First().Name;

            if (saveNeeded)
                SaveProfiles(profiles);

            return profiles;
        }

        private void SaveProfiles(ActionMenuProfiles profiles)
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            var newJson = JsonConvert.SerializeObject(profiles, Formatting.Indented);
            File.WriteAllText(ProfilesFile, newJson);
            _activeProfile = null;
        }
    }
}
