using System;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    internal class CraftingMenuMetadata
    {
        public Type MenuType { get; set; }
        public string TabName { get; set; }
        public string TabDisplayName { get; set; }
        public string TabButtonName { get; set; }
        public MenuIcons MenuIcons { get; set; }
        public string OrderAfterTab { get; set; } = "btnCrafting";
        public string MenuName { get; set; }
        public string FooterName { get; set; }
        public int MenuScreenNo { get; set; } = -1;
        public CustomCraftingMenu MenuPanel { get; set; }
        public GameObject MenuTab { get; set; }
        public GameObject MenuFooter { get; set; }
        public GameObject MenuDisplay { get; set; }

        public CraftingMenuMetadata Clone()
        {
            return new CraftingMenuMetadata()
            {
                MenuType = MenuType,
                TabName = TabName,
                TabDisplayName = TabDisplayName,
                TabButtonName = TabButtonName,
                MenuIcons = MenuIcons,
                OrderAfterTab = OrderAfterTab,
                MenuName = MenuName,
                FooterName = FooterName,
                MenuScreenNo = MenuScreenNo,
                MenuPanel = MenuPanel,
                MenuTab = MenuTab,
                MenuFooter = MenuFooter,
                MenuDisplay = MenuDisplay
            };
        }

    }
}
