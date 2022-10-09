using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ModifAmorphic.Outward.ActionUI.Services
{

    public abstract class JsonProfileService<T> : IDisposable, ISavableProfile where T : new()
    {

        Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();

        protected abstract string FileName { get; }

        protected GlobalProfileService GlobalProfileService { get; set; }

        protected ProfileService ProfileService { get; set; }

        protected T CachedProfile { get; set; }

        protected string CharacterUID;

        private bool disposedValue;


        public JsonProfileService(GlobalProfileService globalProfileService, ProfileService profileService, string characterUID, Func<IModifLogger> getLogger)
        {
            (GlobalProfileService, ProfileService, CharacterUID, _getLogger) = (globalProfileService, profileService, characterUID, getLogger);
            profileService.OnActiveProfileSwitching += TrySaveCurrentProfile;
            profileService.OnActiveProfileSwitched += TryRefreshCachedProfile;
        }

        public virtual T GetProfile()
        {
            if (CachedProfile != null)
                return CachedProfile;

            CachedProfile = LoadProfile();
            if (CachedProfile == null)
                CachedProfile = new T();
            return CachedProfile;
        }

        public virtual void Save() => SaveProfile(GetProfile());

        public virtual void SaveNew(T profile)
        {
            SaveProfile(profile);
            CachedProfile = default;
        }

        protected virtual T LoadProfile()
        {
            string profileFile = Path.Combine(ProfileService.GetActiveActionUIProfile().Path, FileName);

            if (!File.Exists(profileFile))
            {
                Logger.LogDebug($"Profile file '{profileFile}' not found.");
                return default;
            }
            Logger.LogDebug($"Loading {typeof(T)} file '{profileFile}'.");
            string json = File.ReadAllText(profileFile);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private void TrySaveCurrentProfile(IActionUIProfile profile)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to save current {typeof(T).Name} data to profile '{profile?.Name}'.", ex);
            }
        }

        private void TryRefreshCachedProfile(IActionUIProfile profile)
        {
            try
            {
                RefreshCachedProfile(profile, true);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed refresh of current {typeof(T).Name} data for profile '{profile?.Name}'.", ex);
            }
        }

        protected virtual void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            Logger.LogDebug($"{typeof(T).Name}::RefreshCachedProfile Called for action menu profile {actionMenusProfile.Name}.");
            CachedProfile = default;
        }

        protected virtual void SaveProfile(T profile)
        {
            var activeProfile = ProfileService.GetActiveActionUIProfile();
            var profileFile = Path.Combine(activeProfile.Path, FileName);
            Logger.LogInfo($"Saving {typeof(T).Name} for profile {activeProfile.Name} to '{profileFile}'.");
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(profileFile, json);
            CachedProfile = default;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (ProfileService != null)
                    {
                        ProfileService.OnActiveProfileSwitching -= TrySaveCurrentProfile;
                        ProfileService.OnActiveProfileSwitched -= TryRefreshCachedProfile;
                    }
                }
                CachedProfile = default;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PositionsProfileJsonService()
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
