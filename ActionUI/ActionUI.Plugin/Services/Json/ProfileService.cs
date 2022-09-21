using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.UI.DataModels;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.UI.Services
{
    public class ProfileService : IActionUIProfileService, IDisposable
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        ActionUIProfile _activeProfile;
        private bool disposedValue;

        public string ProfilesPath { get; private set; }
        public string ProfilesFile => Path.Combine(ProfilesPath, "profile.json");

        public UnityEvent<IActionUIProfile> OnNewProfile { get; } = new UnityEvent<IActionUIProfile>();
        public UnityEvent<IActionUIProfile> OnActiveProfileChanged { get; } = new UnityEvent<IActionUIProfile>();
        public UnityEvent<IActionUIProfile> OnActiveProfileSwitched { get; } = new UnityEvent<IActionUIProfile>();

        public ProfileService(string profilesRootPath, Func<IModifLogger> getLogger) => (ProfilesPath, _getLogger) = (profilesRootPath, getLogger);

        public IActionUIProfile GetActiveProfile() => GetActiveActionUIProfile();

        public ActionUIProfile GetActiveActionUIProfile()
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

            bool isNewProfile = false;
            if (profile != null)
            {
                profile.Name = name;
            }
            else
            {
                profile = new ActionUIProfile()
                {
                    Name = name,
                    ActionSlotsEnabled = true,
                    DurabilityDisplayEnabled = true,
                    Path = Path.Combine(ProfilesPath, name),
                };
                profiles.Profiles.Add(profile);
                isNewProfile = true;
            }

            profiles.ActiveProfile = name;

            SaveProfiles(profiles);

            if (isNewProfile)
                OnActiveProfileChanged.TryInvoke(GetActiveActionUIProfile());
            else
                OnActiveProfileSwitched.TryInvoke(GetActiveActionUIProfile());
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

        public void Save() => SaveProfile(GetActiveActionUIProfile());

        public void SaveNew(IActionUIProfile profile)
        {
            SaveProfile(profile, false);
            OnNewProfile.TryInvoke(profile);
        }

        public void SaveProfile(IActionUIProfile profile, bool raiseEvent = true)
        {
            var profiles = GetOrCreateProfiles();
            _ = GetOrAddProfileDir(profile.Name);
            var profileIndex = profiles.Profiles.FindIndex(p => p.Name.Equals(profile.Name, StringComparison.InvariantCultureIgnoreCase));
            if (profileIndex == -1)
            {
                profiles.Profiles.Add((ActionUIProfile)profile);
            }
            else
            {
                profiles.Profiles[profileIndex] = (ActionUIProfile)profile;
            }
            profiles.ActiveProfile = profile.Name;
            SaveProfiles(profiles);
            Logger.LogDebug($"Saved profile '{profile.Name}'.");
            _activeProfile = null;

            if (raiseEvent)
                OnActiveProfileChanged.TryInvoke(GetActiveActionUIProfile());
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentNullException(nameof(newName));


            var profile = GetActiveActionUIProfile();

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

        private ActionUIProfiles GetOrCreateProfiles()
        {
            var profileNames = GetProfileDirectories();
            bool saveNeeded = false;

            ActionUIProfiles profiles;
            if (File.Exists(ProfilesFile))
            {
                var json = File.ReadAllText(ProfilesFile);
                profiles = JsonConvert.DeserializeObject<ActionUIProfiles>(json);
                var removeProfiles = new List<int>();
                for (int i = 0; i < profiles.Profiles.Count; i++)
                {
                    if (!profileNames.Any(n => n.Equals(profiles.Profiles[i].Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        removeProfiles.Add(i);
                        saveNeeded = true;
                    }
                }
                foreach (var i in removeProfiles)
                    profiles.Profiles.RemoveAt(i);
            }
            else
                profiles = new ActionUIProfiles();

            foreach (var name in profileNames)
            {
                if (!profiles.Profiles.Any(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    profiles.Profiles.Add(new ActionUIProfile()
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

        private void SaveProfiles(ActionUIProfiles profiles)
        {
            if (!Directory.Exists(ProfilesPath))
                Directory.CreateDirectory(ProfilesPath);

            var newJson = JsonConvert.SerializeObject(profiles, Formatting.Indented);
            File.WriteAllText(ProfilesFile, newJson);
            _activeProfile = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnNewProfile.RemoveAllListeners();
                    OnActiveProfileChanged.RemoveAllListeners();
                    OnActiveProfileSwitched.RemoveAllListeners();
                }

                _activeProfile = null;
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ProfileService()
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
