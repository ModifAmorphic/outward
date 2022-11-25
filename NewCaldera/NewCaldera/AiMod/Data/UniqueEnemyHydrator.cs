using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data
{
    internal class UniqueEnemyHydrator
    {
        Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();

        ProbablityTableData _probablityTableData;

        public UniqueEnemyHydrator(ProbablityTableData probablityTableData, Func<IModifLogger> getLogger)
        {
            _probablityTableData = probablityTableData;
            _getLogger = getLogger;
        }

        public void Hydrate(RegionalUniques regions)
        {
            var regionKeys = regions.RegionUniques.Keys.ToList();
            foreach (var region in regionKeys)
            {
                if (regions.RegionUniques[region] == null)
                    regions.RegionUniques[region] = new Dictionary<string, UniqueEnemy>();

                foreach (var unique in regions.RegionUniques[region].Values)
                {
                    HydrateDropTables(unique);
                }
            }
        }

        public void HydrateDropTables(UniqueEnemy unique)
        {
            if (unique.DropTables == null)
                unique.DropTables = new Dictionary<string, Models.Probablity.ConditionalChanceTable>();
            unique.DropTables.Clear();
            foreach (var tableName in unique.DropTableNames)
            {
                unique.DropTables.Add(tableName, _probablityTableData.GetChanceTable(tableName));
            }
        }
    }
}
