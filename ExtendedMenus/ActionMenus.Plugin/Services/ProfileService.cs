using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    public class ProfileService : IActionMenusProfileService
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        ActionMenusProfile _activeProfile;


        public string ProfilesPath { get; private set; }
        public string ActiveProfileFile => Path.Combine(ProfilesPath, "profile.json");

        public event Action<IActionMenusProfile> OnActiveProfileChanged;

        public ProfileService(string profilesRootPath) => ProfilesPath = profilesRootPath;

        public IActionMenusProfile GetActiveProfile() => GetActiveActionMenusProfile();

        public ActionMenusProfile GetActiveActionMenusProfile()
        {
            if (_activeProfile != null)
                return _activeProfile;

            if (File.Exists(ActiveProfileFile))
            {
                var json = File.ReadAllText(ActiveProfileFile);
                _activeProfile = JsonConvert.DeserializeObject<ActionMenusProfile>(json);
                _activeProfile.Path = Path.Combine(ProfilesPath, _activeProfile.ActiveProfile);
            }

            return _activeProfile;
        }

        public IEnumerable<string> GetProfileNames()
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

        public void SetActiveProfile(string name)
        {
            if (_activeProfile != null && _activeProfile.ActiveProfile.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return;

            _ = GetOrAddProfileDir(name);

            _activeProfile = new ActionMenusProfile()
            {
                ActiveProfile = name
            };
            
            var newJson = JsonConvert.SerializeObject(_activeProfile, Formatting.Indented);
            File.WriteAllText(ActiveProfileFile, newJson);
            OnActiveProfileChanged?.Invoke(_activeProfile);
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
    }
}
