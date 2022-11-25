using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character;
using ModifAmorphic.Outward.NewCaldera.AiMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Services
{
    internal class DeathService : IDisposable
    {
        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly RegionCyclesData _enemyCyclesData;
        private bool disposedValue;

        public DeathService(RegionCyclesData enemyCyclesData, Func<IModifLogger> getLogger)
        {
            (_enemyCyclesData, _getLogger) = (enemyCyclesData, getLogger);
            CharacterPatches.BeforeDie += RecordPlayerDeath;
            CharacterPatches.AfterDie += RecordEnemyDeath;
        }

        private void RecordPlayerDeath(Character character, bool loadedDead)
        {
            if (loadedDead)
                return;

            var area = AreaManager.Instance.GetCurrentAreaEnum();
            if (!_enemyCyclesData.TryGetRegionCycle(area, out var cycle))
                return;

            if (character.OwnerPlayerSys != null)
            {
                if (!cycle.IsCycleStarted())
                    return;

                var killers = character.GetPrivateField<Character, List<Pair<UID, float>>>("m_lastDealers");
                foreach (var pair in killers)
                {
                    if (cycle.UniqueUidIndex.TryGetValue(pair.Key, out var killerName))
                    {
                        Logger.LogDebug($"Player character '{character.name}' was killed by '{killerName}'. Recording death to current cycle.");

                        cycle.PlayerDeaths.Add(EnvironmentConditions.GameTimeF);
                        _enemyCyclesData.Save();
                        return;
                    }
                }
            }
        }

        private void RecordEnemyDeath(Character character, bool loadedDead)
        {
            if (loadedDead)
                return;

            var area = AreaManager.Instance.GetCurrentAreaEnum();
            if (!_enemyCyclesData.TryGetRegionCycle(area, out var cycle))
                return;

            if (!cycle.UniqueUidIndex.TryGetValue(character.UID, out var enemyName)
                    || !cycle.UniqueEnemies.TryGetValue(enemyName, out var enemy))
                return;

            if (!cycle.IsCycleStarted())
                cycle.StartCycle();

            Logger.LogDebug($"Recording '{character.name}' time of death: {EnvironmentConditions.GameTimeF}.");
            enemy.DiedAt = EnvironmentConditions.GameTimeF;
            _enemyCyclesData.Save();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CharacterPatches.AfterDie -= RecordEnemyDeath;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AIDeathService()
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
