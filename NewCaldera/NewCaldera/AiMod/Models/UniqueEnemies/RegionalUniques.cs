using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.AiMod.Models.UniqueEnemies
{
    internal class RegionalUniques
    {
        public Dictionary<string, Dictionary<string, UniqueEnemy>> RegionUniques { get; set; } = new Dictionary<string, Dictionary<string, UniqueEnemy>>();
    }
}
