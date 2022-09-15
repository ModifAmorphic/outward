using System.Collections.Generic;

namespace ModifAmorphic.Outward.Models
{
    public class GuaranteedDropTable
    {
        public string GameObjectName { get; set; }
        public string ItemGenatorName { get; set; }
        public List<CustomGuaranteedDrop> CustomGuaranteedDrops { get; set; } = new List<CustomGuaranteedDrop>();
    }
}
