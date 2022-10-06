using ModifAmorphic.Outward.ActionUI.DataModels.Global;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public class GlobalProfileService : IDisposable
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private GlobalProfile _cachedProfile;
        private Random _random;
        private bool disposedValue;

        public string GlobalPath { get; private set; }
        public string ProfilesPath { get; private set; }
        public string GlobalFile => Path.Combine(GlobalPath, "global.json");

        public GlobalProfileService(string globalPath, string profilesPath, Func<IModifLogger> getLogger)
        {
            (GlobalPath, ProfilesPath, _getLogger) = (globalPath, profilesPath, getLogger);
            _random = new Random();
        }

        public GlobalProfile GetGlobalProfile() => GetOrCreateGlobalProfile();

        public void Save() => SaveProfile(_cachedProfile);

        public void AddOrUpdateEquipmentSet(IEquipmentSet set, string characterUID)
        {
            var setType = set is ArmorSet ? EquipmentSetTypes.Armor : EquipmentSetTypes.Weapon;

            GetGlobalProfile().CharacterEquipmentSets.AddOrUpdate(
                set.SetID,
                new CharacterEquipmentSet()
                {
                    SetID = set.SetID,
                    CharacterUID = characterUID,
                    EquipmentSetType = setType,
                    Name = set.Name
                });
            Save();
        }

        public CharacterEquipmentSet RemoveEquipmentSet(int setID)
        {
            GetGlobalProfile().CharacterEquipmentSets.TryRemove(setID, out var set);
            Save();
            return set;
        }

        public int GetNextEquipmentSetID() 
        { 
            int nextId = _random.Next(InventorySettings.MinSetItemID, InventorySettings.MaxSetItemID);
            int attempt = 0;
            int maxAttempts = 1000;
            while (GetGlobalProfile().CharacterEquipmentSets.ContainsKey(nextId) && attempt < maxAttempts)
            {
                nextId = _random.Next(InventorySettings.MinSetItemID, InventorySettings.MaxSetItemID);
                attempt++;
            }
            return nextId;
        }

        private string GetOrAddGlobalDir()
        {
            if (!Directory.Exists(GlobalPath))
                Directory.CreateDirectory(GlobalPath);

            return GlobalPath;
        }

        private List<string> GetProfilesDirectories()
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

        private GlobalProfile GetOrCreateGlobalProfile()
        {
            if (_cachedProfile != null)
                return _cachedProfile;

            bool saveNeeded = false;

            GlobalProfile profile;
            if (File.Exists(GlobalFile))
            {
                var json = File.ReadAllText(GlobalFile);
                profile = JsonConvert.DeserializeObject<GlobalProfile>(json);

                var characterProfiles = GetProfilesDirectories();

                //Remove sets for non existing characters
                var characterSets = profile.CharacterEquipmentSets.Values.ToArray();
                for (int i = 0; i < characterSets.Length; i++)
                {
                    if (!characterProfiles.Any(p => p.Equals(characterSets[i].CharacterUID, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        profile.CharacterEquipmentSets.Remove(characterSets[i].SetID);
                        saveNeeded = true;
                    }
                }
            }
            else
            {
                profile = new GlobalProfile();
                saveNeeded = true;
            }

            if (saveNeeded)
                SaveProfile(profile);

            _cachedProfile = profile;

            return profile;
        }

        private void SaveProfile(GlobalProfile profile)
        {
            _ = GetOrAddGlobalDir();
            Logger.LogInfo($"Saving {nameof(GlobalProfile)} to '{GlobalFile}'.");
            var newJson = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(GlobalFile, newJson);
            _cachedProfile = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                _cachedProfile = null;
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
