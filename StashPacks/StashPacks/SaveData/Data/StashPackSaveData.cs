using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.SaveData.Data
{
    internal class StashPackSaveData
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashPackSaveData(Func<IModifLogger> getLogger) => (_getLogger) = (getLogger);
    }
}
