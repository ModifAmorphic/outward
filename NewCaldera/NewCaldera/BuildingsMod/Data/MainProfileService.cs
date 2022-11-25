using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.Services;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.BuildingsMod.Data
{
    public class MainProfileService : IDisposable, ISavableProfile
    {
        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private bool _versionEvaluated = false;
        private bool disposedValue;

        public string ProfilePath { get; private set; }
        public string ProfileFile => Path.Combine(ProfilePath, "NewCaldera.json");

        private object _mainProfileCache;

        //public GlobalProfileService GlobalProfileService { get; private set; }

        public MainProfileService(string characterProfilePath, Func<IModifLogger> getLogger)
        {
            (ProfilePath, _getLogger) = (characterProfilePath, getLogger);
            _ = GetOrAddProfileDir();
        }

        public object GetMainProfile()
        {

            if (_mainProfileCache != null)
                return _mainProfileCache;

            _mainProfileCache = GetOrCreateMainProfile();


            if (_mainProfileCache != null)
            {
                //set defaults if needed
            }

            return _mainProfileCache;
        }



        public string GetOrAddProfileDir()
        {
            if (!Directory.Exists(ProfilePath))
                Directory.CreateDirectory(ProfilePath);

            return ProfilePath;
        }

        public void ExpireCache() => _mainProfileCache = null;

        private object GetOrCreateMainProfile()
        {
            object profile;
            if (File.Exists(ProfileFile))
            {
                var json = File.ReadAllText(ProfileFile);
                profile = JsonConvert.DeserializeObject<object>(json);
            }
            else
            {
                profile = new object();
            }

            return profile;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
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

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
