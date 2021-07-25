using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.SaveData.Data
{
    internal class StashPackSaveData
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashPackSaveData(Func<IModifLogger> getLogger) => (_getLogger) = (getLogger);
    }
}
