using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.NewCaldera.Models
{
    internal class ItemLocalizations
    {
        public Dictionary<int, ItemLocalization> Items { get; set; } = new Dictionary<int, ItemLocalization>();
    }
    internal class ItemLocalization
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
