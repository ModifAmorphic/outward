using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.NewCaldera.AiMod.Data.Character;
using ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies;
using ModifAmorphic.Outward.NewCaldera.AiMod.Patches;
using NodeCanvas.Tasks.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Services
{
    internal class AISpawnManager : IDisposable
    {
        private readonly Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger.Invoke();

        private readonly RegionCyclesData _enemyCyclesData;
        private bool disposedValue;

        private AreaManager.AreaEnum CurrentArea;

        public AISpawnManager(RegionCyclesData enemyCyclesData, Func<IModifLogger> getLogger)
        {
            (_enemyCyclesData, _getLogger) = (enemyCyclesData, getLogger);
            SetObjectActivePatches.TryBeforeOnExecute = TryActivateUniqueEnemy;
            GameObjectToggleOnQuestEventPatches.AfterRefreshState += TryActivateUniqueEnemy;
            NetworkLevelLoader.Instance.onSceneLoadingDone += OnSceneLoaded;
        }

        private void OnSceneLoaded()
        {
            CurrentArea = AreaManager.Instance.GetCurrentAreaEnum();
            if (!_enemyCyclesData.TryGetRegionCycle(CurrentArea, out var cycle))
                return;

            if (cycle.IsCycleExpired())
                cycle.ResetCycle();
        }

        private void TryActivateUniqueEnemy(GameObjectToggleOnQuestEvent goToggleQuestEvent, QuestEventReference listenedEvent, QuestEventData eventData)
        {
            if (!_enemyCyclesData.TryGetRegionCycle(CurrentArea, out var cycle))
                return;

            if (cycle.UniqueQuestIndex.TryGetValue(listenedEvent.EventUID, out var uniqueName))
            {
                Logger.LogDebug($"Got name {uniqueName} for EventUID {listenedEvent.EventUID}.");
                if (cycle.UniqueEnemies.TryGetValue(uniqueName, out var unique))
                {
                    Logger.LogDebug($"Found {uniqueName}.");
                    if (!unique.IsDead())
                    {
                        Logger.LogDebug($"{uniqueName} is alive.");
                        if (goToggleQuestEvent.ObjectToToggle != null)
                        {
                            var enemyCharacter = goToggleQuestEvent.ObjectToToggle.GetComponentInChildren<Character>(true);
                            if (enemyCharacter != null && enemyCharacter.UID == unique.UID)
                            {
                                if (!goToggleQuestEvent.ObjectToToggle.activeSelf)
                                {
                                    RecordPreviousDeath(unique);
                                    goToggleQuestEvent.ObjectToToggle.SetActive(true);
                                    Logger.LogDebug($"Activated '{goToggleQuestEvent.ObjectToToggle.name}'.");
                                }
                                
                                DisableCharacterWeaponsDrop(enemyCharacter);
                            }
                        }
                    }
                }
            }
        }

        private bool TryActivateUniqueEnemy(SetObjectActive setObjectActive, bool setActive)
        {
            if (!_enemyCyclesData.TryGetRegionCycle(CurrentArea, out var cycle))
                return false;

            var aiSquad = setObjectActive.agent.GetComponent<AISquad>();
            if (aiSquad != null)
            {
                if (aiSquad.AliveMemberCount() < 1)
                    return false;

                if (cycle.SquadUidIndex.TryGetValue(aiSquad.UID, out var uniqueName) && cycle.UniqueEnemies.TryGetValue(uniqueName, out var unique))
                {
                    if (!setActive)
                        RecordPreviousDeath(unique);

                    if (!unique.IsDead())
                    {
                        Logger.LogDebug($"{uniqueName} is alive.");
                        if (!aiSquad.gameObject.activeSelf)
                            aiSquad.gameObject.SetActive(true);
                        var character = aiSquad.GetComponentsInChildren<Character>().FirstOrDefault(c => c.UID == unique.UID);
                        DisableCharacterWeaponsDrop(character);
                    }

                    return true;
                }
            }
            else
            {
                var character = setObjectActive.agent.GetComponent<Character>();
                if (character != null && cycle.UniqueUidIndex.TryGetValue(character.UID, out var uniqueName) && cycle.UniqueEnemies.TryGetValue(uniqueName, out var unique) && !unique.IsDead())
                {
                    if (!setActive)
                        RecordPreviousDeath(unique);

                    Logger.LogDebug($"{uniqueName} is alive.");

                    if (!character.gameObject.activeSelf)
                        character.gameObject.SetActive(true);
                    DisableCharacterWeaponsDrop(character);
                    return true;
                }
            }
            
            return false;
        }

        private void DisableCharacterWeaponsDrop(Character character)
        {
            var lootableOnDeath = character.GetComponent<LootableOnDeath>();
            if (lootableOnDeath != null)
                lootableOnDeath.DropWeapons = false;
        }

        private void RecordPreviousDeath(UniqueEnemy uniqueEnemy)
        {
            if (uniqueEnemy.PreviousDeaths == null || uniqueEnemy.PreviousDeaths.Count == 0)
            {
                uniqueEnemy.PreviousDeaths = new List<float> { EnvironmentConditions.GameTimeF };
                _enemyCyclesData.Save();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SetObjectActivePatches.TryBeforeOnExecute = null;
                    GameObjectToggleOnQuestEventPatches.AfterRefreshState -= TryActivateUniqueEnemy;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AISpawnManager()
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
