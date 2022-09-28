using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.ActionUI.Services
{

    public abstract class JsonProfileService<T>: IDisposable where T : new()
    {

        Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();

        protected abstract string FileName { get; }
        
        //public string PositionsFile = "UIPositions.json";

        protected ProfileService ProfileService { get; set; }

        protected T CachedProfile { get; set; }

        private bool disposedValue;

        //public virtual UnityEvent<T> OnProfileChanged { get; } = new UnityEvent<T>();

        public JsonProfileService(ProfileService profileService, Func<IModifLogger> getLogger)
        {
            (ProfileService, _getLogger) = (profileService, getLogger);
            //profileService.OnActiveProfileChanged.AddListener((profile) => RefreshCachedProfile(profile));
            profileService.OnActiveProfileSwitched.AddListener((profile) => RefreshCachedProfile(profile, true));
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

        protected virtual void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            Logger.LogDebug($"{typeof(T).Name}::RefreshCachedProfile Called for action menu profile {actionMenusProfile.Name}.");
            CachedProfile = default;
        }

        protected virtual void SaveProfile(T profile)
        {
            //RemoveOriginPositions(profile);
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            var activeProfile = ProfileService.GetActiveActionUIProfile();
            var profileFile = Path.Combine(activeProfile.Path, FileName);
            Logger.LogDebug($"Saving {typeof(T)} for profile {activeProfile.Name}");
            File.WriteAllText(profileFile, json);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_profileService.OnActiveProfileChanged.RemoveListener((profile) => RefreshCachedProfile(profile));
                    ProfileService.OnActiveProfileSwitched.RemoveListener((profile) => RefreshCachedProfile(profile, true));
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
