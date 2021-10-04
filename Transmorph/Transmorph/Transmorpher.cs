using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Transmorph
{
    internal class Transmorpher
    {
        private readonly TransmorphConfigSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;
        private readonly ResourcesPrefabManager _resourcesPrefabManager;
        private readonly CustomCraftingModule _craftingModule;
        private readonly ItemVisualizer _itemVisualizer;

        public Transmorpher(BaseUnityPlugin baseUnityPlugin, CustomCraftingModule craftingModule, ItemVisualizer itemVisualizer, TransmorphConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _craftingModule, _itemVisualizer, _settings, _getLogger) = (baseUnityPlugin, craftingModule, itemVisualizer, settings, getLogger);
        }
        
        private Transform _parentTransform;
        //public void SetTransmorph(Transmorph transmorph)
        //{
        //    _itemVisualizer.RegisterItemVisual(transmorph.SourceItemID, transmorph.ItemUID);

        //    var sourceItem = _resourcesPrefabManager.GetItemPrefab(transmorph.SourceItemID);
        //}
        //public void SetTransmorph(int itemID, string targetUID)
        //{
        //    _itemVisualizer.RegisterItemVisual(itemID, targetUID);
        //}

        public void SaveTransmorph(Item sourceItem, Item targetItem)
        {
            var xMorph = new Transmorph()
            {
                ItemUID = targetItem.UID,
                SourceItemID = sourceItem.ItemID
            };
        }
        internal class Transmorph
        {
            public string ItemUID { get; set; }
            public int SourceItemID { get; set; }
        }
    }
}
