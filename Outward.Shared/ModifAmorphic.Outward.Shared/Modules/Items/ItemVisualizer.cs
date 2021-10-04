using Localizer;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Modules.Items
{
    public class ItemVisualizer : IModifModule
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();
        private readonly Func<ResourcesPrefabManager> _prefabManagerFactory;
        private ResourcesPrefabManager PrefabManager => _prefabManagerFactory.Invoke();
        private readonly Func<ItemManager> _itemManagerFactory;
        private ItemManager ItemManager => _itemManagerFactory.Invoke();


        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            //typeof(ItemPatches),
            typeof(ItemManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(ItemPatches)
        };

        private readonly ConcurrentDictionary<string, int> _itemVisuals = new ConcurrentDictionary<string, int>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ItemVisualizer(Func<ResourcesPrefabManager> prefabManagerFactory, Func<ItemManager> itemManagerFactory, Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._prefabManagerFactory = prefabManagerFactory;
            _itemManagerFactory = itemManagerFactory;

            ItemManagerPatches.GetVisualsByItemOverride += GetVisualsByItemOverride;
            ItemManagerPatches.GetSpecialVisualsByItemOverride += GetSpecialVisualsByItemOverride;
        }

        /// <summary>
        /// Registers a UID to use the source Item IDs visuals.
        /// </summary>
        /// <param name="sourceItemID">The ItemID whose visuals should be used.</param>
        /// <param name="targetItemUID">The target item UID that should use the <paramref name="sourceItemID"/> item's visuals.</param>
        public void RegisterItemVisual(int sourceItemID, string targetItemUID)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Registering target UID '{targetItemUID}' to use ItemID {sourceItemID} visuals.");
            _itemVisuals.TryAdd(targetItemUID, sourceItemID);
        }

        private bool GetVisualsByItemOverride(Item item, out ItemVisual visual)
        {
            if (!_itemVisuals.TryGetValue(item.UID, out var visualItemID))
            {
                visual = null;
                Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): No custom ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}).");
                return false;
            }

            if (ItemManager.Instance != null)
            {
                visual = ItemManager.GetVisuals(visualItemID);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
                return true;
            }

            var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);
            var instantiateInfo = typeof(ItemManager).GetMethod("InstantiateVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var transform = instantiateInfo.Invoke(null, new object[] { prefab }) as Transform;
            
            visual = transform.GetComponent<ItemVisual>();

            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Tried to replace " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            return transform != null;
        }
        private bool GetSpecialVisualsByItemOverride(Item item, out ItemVisual visual)
        {
            if (!_itemVisuals.TryGetValue(item.UID, out var visualItemID))
            {
                visual = null;
                Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): No custom ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}).");
                return false;
            }

            var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);

            if (ItemManager.Instance != null)
            {
                visual = ItemManager.Instance.InvokePrivateMethod<ItemManager, ItemVisual>("Internal_GetSpecialVisuals", new object[] { prefab });
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
                return true;
            }

            var instantiateInfo = typeof(ItemManager).GetMethod("InstantiateSpecialVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var transform = instantiateInfo.Invoke(null, new object[] { prefab }) as Transform;

            visual = transform.GetComponent<ItemVisual>();
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Tried to replace " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");

            return transform != null;
        }
    }
}
