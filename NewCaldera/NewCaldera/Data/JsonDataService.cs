using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.Services;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.Data
{

    internal abstract class JsonDataService<T> : IDisposable, ISavableProfile where T : new()
    {

        Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();
        protected abstract string FileName { get; }
        protected readonly IDirectoryHandler _directoryHandler;
        public string ProfilePath => _directoryHandler.GetOrAddDir();

        protected T CachedData { get; set; }

        private bool disposedValue;

        public event Action<T> OnRefreshed;

        public JsonDataService(IDirectoryHandler directoryHandler, Func<IModifLogger> getLogger) =>
            (_directoryHandler, _getLogger) = (directoryHandler, getLogger);


        public virtual T GetData()
        {
            if (CachedData != null)
                return CachedData;

            CachedData = LoadData();
            if (CachedData == null)
                CachedData = new T();
            return CachedData;
        }

        public virtual void Save() => SaveData(GetData());

        public virtual void SaveNew(T data)
        {
            SaveData(data);
            ResetCache(true);
        }

        protected virtual T LoadData()
        {
            string dataFile = Path.Combine(ProfilePath, FileName);

            if (!File.Exists(dataFile))
            {
                Logger.LogDebug($"Data file '{dataFile}' not found.");
                return default;
            }
            Logger.LogDebug($"Loading {typeof(T)} file '{dataFile}'.");
            string json = File.ReadAllText(dataFile);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private void TryRefreshCachedData()
        {
            try
            {
                ResetCache(true);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed refresh of {typeof(T).Name} data.", ex);
            }
        }

        protected virtual void ResetCache(bool suppressChangeEvent = false)
        {
            CachedData = default;
            if (!suppressChangeEvent)
                OnRefreshed?.Invoke(GetData());
        }

        protected virtual void SaveData(T data)
        {
            var profileFile = Path.Combine(ProfilePath, FileName);
            Logger.LogInfo($"Saving {typeof(T).Name} to '{profileFile}'.");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(profileFile, json);
            ResetCache(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }
                CachedData = default;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
