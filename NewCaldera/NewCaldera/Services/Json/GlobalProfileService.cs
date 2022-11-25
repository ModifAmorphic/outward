//using ModifAmorphic.Outward.Extensions;
//using ModifAmorphic.Outward.Logging;
//using ModifAmorphic.Outward.NewCaldera.DataModels.Global;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace ModifAmorphic.Outward.NewCaldera.Services
//{
//    public class GlobalProfileService : IDisposable, ISavableProfile
//    {
//        Func<IModifLogger> _getLogger;
//        private IModifLogger Logger => _getLogger.Invoke();

//        private GlobalProfile _cachedProfile;
//        private Random _random;
//        private bool disposedValue;

//        public string GlobalPath { get; private set; }
//        public string ProfilesPath { get; private set; }
//        public string GlobalFile => Path.Combine(GlobalPath, "global.json");

//        public GlobalProfileService(string globalPath, string profilesPath, Func<IModifLogger> getLogger)
//        {
//            (GlobalPath, ProfilesPath, _getLogger) = (globalPath, profilesPath, getLogger);
//            SaveManager.Instance.onSaveRetrieved += () => RemoveDeletedCharacterProfiles(SaveManager.Instance.CharacterSaves.Select(s => s.CharacterUID).ToList());
//            _random = new Random();
//        }

//        public GlobalProfile GetGlobalProfile() => GetOrCreateGlobalProfile();

//        public void Save() => SaveProfile(GetGlobalProfile());

//        public void AddOrUpdateItemID(int itemID, string characterUID)
//        {

//            GetGlobalProfile().CharacterItemIDs.AddOrUpdate(itemID, characterUID);
//            Save();
//        }

//        //public void RemoveItemID(int setID)
//        //{
//        //    GetGlobalProfile().CharacterItemIDs.TryRemove(setID, out var _);
//        //    Save();
//        //}

//        //public int GetNextItemID()
//        //{
//        //    int nextId = _random.Next(InventorySettings.MinItemID, InventorySettings.MaxItemID);
//        //    int attempt = 0;
//        //    int maxAttempts = 1000;
//        //    while (GetGlobalProfile().CharacterItemIDs.ContainsKey(nextId) && attempt < maxAttempts)
//        //    {
//        //        nextId = _random.Next(InventorySettings.MinItemID, InventorySettings.MaxItemID);
//        //        attempt++;
//        //    }
//        //    return nextId;
//        //}

//        private string GetOrAddGlobalDir()
//        {
//            if (!Directory.Exists(GlobalPath))
//                Directory.CreateDirectory(GlobalPath);

//            return GlobalPath;
//        }

//        private List<string> GetProfilesDirectories()
//        {
//            if (!Directory.Exists(ProfilesPath))
//                Directory.CreateDirectory(ProfilesPath);

//            var dirs = Directory.GetDirectories(ProfilesPath);
//            var names = new List<string>();
//            foreach (var dir in dirs)
//            {
//                names.Add(new DirectoryInfo(dir).Name);
//            }
//            return names;
//        }

//        public void RemoveDeletedCharacterProfiles(List<string> characterUIDs)
//        {
//            Logger.LogDebug("Attempting removal of delete character profiles.");
//            if (!Directory.Exists(ProfilesPath))
//                return;

//            var dirs = Directory.GetDirectories(ProfilesPath);
//            foreach (var dir in dirs)
//            {
//                var profileUID = new DirectoryInfo(dir).Name;
//                if (!characterUIDs.Any(uid => uid.Equals(profileUID, StringComparison.InvariantCultureIgnoreCase)))
//                {
//                    try
//                    {
//                        Logger.LogInfo($"Deleting profile for nonexistent character {profileUID}.");
//                        Directory.Delete(dir, true);
//                    }
//                    catch (Exception ex)
//                    {
//                        Logger.LogException($"Deleting of nonexistent character profile folder {profileUID} failed.", ex);
//                    }
//                }
//            }

//        }

//        private GlobalProfile GetOrCreateGlobalProfile()
//        {
//            if (_cachedProfile != null)
//                return _cachedProfile;

//            bool saveNeeded = false;

//            GlobalProfile profile;
//            if (File.Exists(GlobalFile))
//            {
//                var json = File.ReadAllText(GlobalFile);
//                profile = JsonConvert.DeserializeObject<GlobalProfile>(json);

//                var characterProfiles = GetProfilesDirectories();

//                //Remove sets for non existing characters
//                var characterSets = profile.CharacterEquipmentSets.Values.ToArray();
//                for (int i = 0; i < characterSets.Length; i++)
//                {
//                    if (!characterProfiles.Any(p => p.Equals(characterSets[i].CharacterUID, StringComparison.InvariantCultureIgnoreCase)))
//                    {
//                        profile.CharacterEquipmentSets.Remove(characterSets[i].SetID);
//                        saveNeeded = true;
//                    }
//                }
//            }
//            else
//            {
//                profile = new GlobalProfile();
//                saveNeeded = true;
//            }

//            if (saveNeeded)
//                SaveProfile(profile);

//            _cachedProfile = profile;

//            return profile;
//        }

//        private void SaveProfile(GlobalProfile profile)
//        {
//            _ = GetOrAddGlobalDir();
//            Logger.LogInfo($"Saving {nameof(GlobalProfile)} to '{GlobalFile}'.");
//            var newJson = JsonConvert.SerializeObject(profile, Formatting.Indented);
//            File.WriteAllText(GlobalFile, newJson);
//            _cachedProfile = null;
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {

//                }

//                _cachedProfile = null;
//                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//                // TODO: set large fields to null
//                disposedValue = true;
//            }
//        }

//        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//        // ~ProfileService()
//        // {
//        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//        //     Dispose(disposing: false);
//        // }

//        public void Dispose()
//        {
//            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
