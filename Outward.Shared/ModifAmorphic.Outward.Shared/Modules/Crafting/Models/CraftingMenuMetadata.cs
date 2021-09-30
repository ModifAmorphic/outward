using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    internal class CraftingMenuMetadata
    {
        public Type MenuType { get; set; }
        public string TabName { get; set; }
        public string TabDisplayName { get; set; }
        public string TabButtonName { get; set; }
        public int TabOrderNo { get; set; }
        public string MenuName { get; set; }
        public string FooterName { get; set; }
        public int MenuScreenNo { get; set; } = -1;
        public GameObject MenuTab { get; set; }
        public GameObject MenuFooter { get; set; }
        public GameObject MenuDisplay { get; set; }

    }
}
