using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.UI.Services
{

    public class PositionsProfileJsonService : IPositionsProfileService, IDisposable
    {

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        public string PositionsFile = "UIPositions.json";

        private ProfileService _profileService;

        private PositionsProfile _positionsProfile;
        private bool disposedValue;

        public UnityEvent<PositionsProfile> OnProfileChanged { get; } = new UnityEvent<PositionsProfile>();

        public PositionsProfileJsonService(ProfileService profileService, Func<IModifLogger> getLogger)
        {
            (_profileService, _getLogger) = (profileService, getLogger);
            profileService.OnActiveProfileChanged.AddListener((profile) => RefreshCachedProfile(profile));
            profileService.OnActiveProfileSwitched.AddListener((profile) => RefreshCachedProfile(profile, true));
        }

        public PositionsProfile GetProfile()
        {
            if (_positionsProfile != null)
                return _positionsProfile;

            _positionsProfile = LoadProfile() ?? new PositionsProfile();
            return _positionsProfile;
        }

        public void Save() => SaveProfile(GetProfile());

        public void SaveNew(PositionsProfile positionsProfile)
        {
            SaveProfile(positionsProfile);
            _positionsProfile = null;
        }

        public void AddOrUpdate(UIPositions position)
        {
            GetProfile().AddOrReplacePosition(position);
            Save();
            OnProfileChanged.TryInvoke(GetProfile());
        }

        public void Remove(UIPositions position)
        {
            GetProfile().RemovePosition(position);
            Save();
            OnProfileChanged.TryInvoke(GetProfile());
        }

        private PositionsProfile LoadProfile()
        {
            string profileFile = Path.Combine(_profileService.GetActiveActionUIProfile().Path, PositionsFile);

            if (!File.Exists(profileFile))
            {
                Logger.LogDebug($"Profile file '{profileFile}' not found.");
                return null;
            }
            Logger.LogDebug($"Loading profile file '{profileFile}'.");
            string json = File.ReadAllText(profileFile);
            return JsonConvert.DeserializeObject<PositionsProfile>(json);
        }

        private void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            Logger.LogDebug($"PositionsProfileJsonService::RefreshCachedProfile Called for action menu profile {actionMenusProfile.Name}.");
            _positionsProfile = null;
            OnProfileChanged.TryInvoke(GetProfile());
        }

        private void SaveProfile(PositionsProfile positonsProfile)
        {
            RemoveOriginPositions(positonsProfile);
            var json = JsonConvert.SerializeObject(positonsProfile, Formatting.Indented);
            var activeProfile = _profileService.GetActiveActionUIProfile();
            var profileFile = Path.Combine(activeProfile.Path, PositionsFile);
            Logger.LogDebug($"Saving positions for profile {activeProfile.Name}");
            File.WriteAllText(profileFile, json);
        }

        private void RemoveOriginPositions(PositionsProfile positonsProfile)
        {
            var removals = positonsProfile.Positions.Where(p => p.ModifiedPosition == p.OriginPosition).ToList();
            removals.ForEach(p => positonsProfile.RemovePosition(p));
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
                _positionsProfile = null;
                _positionsProfile = null;
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
