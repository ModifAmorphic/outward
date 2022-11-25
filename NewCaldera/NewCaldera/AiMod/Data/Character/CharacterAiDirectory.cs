using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.Data;
using System;
using System.IO;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character
{
    internal class CharacterAiDirectory : ModDirectory, IDisposable
    {
        private bool disposedValue;

        protected override string _subModFolder => ModInfo.AiModName;

        public CharacterAiDirectory(string path, Func<IModifLogger> getLogger) : base(path, getLogger)
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CharacterAiDirectory()
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
