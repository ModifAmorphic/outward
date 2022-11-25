using ModifAmorphic.Outward.NewCaldera.AiMod.Models.Probablity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies
{
    internal class UniqueEnemy
    {
        public string SquadUID { get; set; }
        public string UID { get; set; }
        public string QuestEventUID { get; set; }
        public string Name { get; set; }
        public float? DiedAt { get; set; }
        public HashSet<string> DropTableNames { get; set; }
        [JsonIgnore]
        public Dictionary<string, ConditionalChanceTable> DropTables { get; set; } = new Dictionary<string, ConditionalChanceTable>();
        public List<float> PreviousDeaths { get; set; }

        public bool IsDead() => DiedAt != null && DiedAt <= EnvironmentConditions.GameTimeF;
        public void ResetDeaths()
        {
            if (IsDead())
            {
                PreviousDeaths.Add((float)DiedAt);
                DiedAt = 0;
            }
        }
    }
}
