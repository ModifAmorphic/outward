using ModifAmorphic.Outward.Logging;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.Data
{
    internal abstract class ModDirectory : IDirectoryHandler
    {
        private readonly Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();

        private DateTime _lastCheckExpiry = DateTime.MinValue;
        protected readonly string _modPath;
        protected abstract string _subModFolder { get; }
        protected virtual string _subModPath => Path.Combine(_modPath, _subModFolder);
        private string _lastSubModPath;

        public ModDirectory(string modPath, Func<IModifLogger> getLogger)
        {
            _modPath = modPath;
            _getLogger = getLogger;
        }

        public virtual string GetOrAddDir()
        {
            if ((_lastSubModPath != _subModPath || DateTime.Now > _lastCheckExpiry) && !Directory.Exists(_subModPath))
            {
                Directory.CreateDirectory(_subModPath);
                Logger.LogDebug($"{this.GetType().Name}: Created directory '{_subModPath}'.");
                _lastCheckExpiry = DateTime.Now.AddSeconds(10);
            }
            _lastSubModPath = _subModPath;
            return _subModPath;
        }

    }
}
