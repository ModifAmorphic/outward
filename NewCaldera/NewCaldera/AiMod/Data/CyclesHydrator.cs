using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Data
{
    internal class CyclesHydrator
    {
        Func<IModifLogger> _getLogger;
        protected IModifLogger Logger => _getLogger.Invoke();

        //private readonly ProbablityTableData _probablityTableData;
        private readonly RegionalUniqueEnemyData _regionalEnemyData;
        //private readonly UniqueEnemyHydrator _uniqueEnemyHydrator;

        public CyclesHydrator(RegionalUniqueEnemyData regionalEnemyData, Func<IModifLogger> getLogger)
        {
            //_probablityTableData = probablityTableData;
            _regionalEnemyData = regionalEnemyData;
            //_uniqueEnemyHydrator = uniqueEnemyHydrator;
            _getLogger = getLogger;
        }

        public bool TryHydrate(RegionCycles cycles)
        {
            var regionUniques = _regionalEnemyData.GetData().RegionUniques;
            var regionKeys = regionUniques.Keys;
            bool wasHydrated = false;

            if (cycles.UniqueCycles == null)
            {
                cycles.UniqueCycles = new Dictionary<string, RegionCycle>();
                wasHydrated = true;
            }
            Logger.LogDebug($"Hydrating {regionUniques.Count} Region Cycles.");
            foreach (var region in regionKeys)
            {
                Logger.LogDebug($"Hydrating Region Cycle {region}.");
                if (!cycles.UniqueCycles.ContainsKey(region))
                {
                    cycles.UniqueCycles.Add(region, new RegionCycle());
                    wasHydrated = true;
                }

                if (cycles.UniqueCycles[region].UniqueEnemies == null)
                {
                    cycles.UniqueCycles[region].UniqueEnemies = new Dictionary<string, UniqueEnemy>();
                    wasHydrated = true;
                }

                foreach (var uniqueKvp in regionUniques[region])
                {
                    if (!cycles.UniqueCycles[region].UniqueEnemies.ContainsKey(uniqueKvp.Key))
                    {
                        cycles.UniqueCycles[region].UniqueEnemies.Add(uniqueKvp.Key, uniqueKvp.Value);
                        wasHydrated = true;
                    }

                    var uniqueEnemy = cycles.UniqueCycles[region].UniqueEnemies[uniqueKvp.Key];
                    if (uniqueEnemy.PreviousDeaths == null)
                        uniqueEnemy.PreviousDeaths = new List<float>();

                    //Hydrate drop tables
                    if (uniqueEnemy.DropTables == null)
                        uniqueEnemy.DropTables = new Dictionary<string, ConditionalChanceTable>();


                    var dropTables = cycles.UniqueCycles[region].UniqueEnemies[uniqueKvp.Key].DropTables;
#if DEBUG           
                    Logger.LogDebug($"Hydrating drop tables for enemy {uniqueKvp.Value.Name} in region {region}. Source Drop Tables: {uniqueKvp.Value.DropTables.Count()}. Target Drop Tables: {dropTables.Count()}.");
#endif
                    var missingDropTables = uniqueKvp.Value.DropTables.Where(dt => !dropTables.ContainsKey(dt.Key));
                    foreach (var dropTableKvp in missingDropTables)
                    {
                        dropTables.Add(dropTableKvp.Key, dropTableKvp.Value);
                        wasHydrated = true;
                    }
                }
                //hydrate indexes
                cycles.UniqueCycles[region].UniqueQuestIndex = cycles.UniqueCycles[region].UniqueEnemies
                                                                                          .Values
                                                                                          .Where(u => !string.IsNullOrEmpty(u.QuestEventUID))
                                                                                          .ToDictionary(u => u.QuestEventUID, u => u.Name);

                cycles.UniqueCycles[region].UniqueUidIndex = cycles.UniqueCycles[region].UniqueEnemies
                                                                                          .Values
                                                                                          .Where(u => !string.IsNullOrEmpty(u.UID))
                                                                                          .ToDictionary(u => u.UID, u => u.Name);

                cycles.UniqueCycles[region].SquadUidIndex = cycles.UniqueCycles[region].UniqueEnemies
                                                                                          .Values
                                                                                          .Where(u => !string.IsNullOrEmpty(u.SquadUID))
                                                                                          .ToDictionary(u => u.SquadUID, u => u.Name);

                if (cycles.UniqueCycles[region].PlayerDeaths == null)
                    cycles.UniqueCycles[region].PlayerDeaths = new List<float>();
            }

            return wasHydrated;
        }
    }
}
