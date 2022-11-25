using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies
{
    internal class RegionCycle
    {
        public string OutsideRegion { get; set; }
        public float CycleStartTime { get; set; } = -1f;
        public float CycleExpiry { get; set; } = float.MaxValue;
        public Dictionary<string, UniqueEnemy> UniqueEnemies { get; set; }

        /// <summary>
        /// key = <see cref="UniqueEnemy.QuestEventUID"/>, value = <see cref="UniqueEnemy.Name"/>
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> UniqueQuestIndex { get; set; }
        /// <summary>
        /// key = <see cref="UniqueEnemy.UID"/>, value = <see cref="UniqueEnemy.Name"/>
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> UniqueUidIndex { get; set; }
        /// <summary>
        /// key = <see cref="UniqueEnemy.SquadUID"/>, value = <see cref="UniqueEnemy.Name"/>
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> SquadUidIndex { get; set; }

        public List<float> PlayerDeaths { get; set; }

        public int GetUniquesKilledAmount() => UniqueEnemies.Values.Count(u => u.IsDead());

        public int GetPlayerDeathsAmount() => PlayerDeaths?.Count(p => p <= EnvironmentConditions.GameTimeF) ?? 0;

        public bool IsCycleStarted()
        {
            return CycleStartTime > -1 && UniqueEnemies.Values.Any(u => u.IsDead());
        }

        public bool IsCycleExpired()
        {
            return (UniqueEnemies.Values.Any(u => u.IsDead())) && EnvironmentConditions.GameTimeF > CycleExpiry;
        }
        public void StartCycle()
        {
            CycleStartTime = EnvironmentConditions.GameTimeF;
            CycleExpiry = EnvironmentConditions.GameTimeF + (float)(24 * 7);
            PlayerDeaths?.Clear();
            if (UniqueEnemies == null)
                return;
            foreach (UniqueEnemy uniqueEnemy in UniqueEnemies.Values)
            {
                uniqueEnemy.ResetDeaths();
            }
        }
        public void ResetCycle()
        {
            CycleStartTime = -1f;
            CycleExpiry = float.MaxValue;
            PlayerDeaths?.Clear();
            if (UniqueEnemies == null)
                return;
            foreach(UniqueEnemy uniqueEnemy in UniqueEnemies.Values)
            {
                uniqueEnemy.ResetDeaths();
            }
        }
    }
}
