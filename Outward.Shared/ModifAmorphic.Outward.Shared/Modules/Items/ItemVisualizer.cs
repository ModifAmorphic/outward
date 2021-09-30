using Localizer;
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

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(ItemPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(ItemPatches)
        };

        private readonly ConcurrentDictionary<string, int> _itemVisuals = new ConcurrentDictionary<string, int>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ItemVisualizer(Func<ResourcesPrefabManager> prefabManagerFactory, Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._prefabManagerFactory = prefabManagerFactory;

            ItemPatches.GetItemVisualBefore += ReplaceItemVisual;
        }

        public void RegisterItemVisual(int sourceItemID, string targetItemUID)
        {
            _itemVisuals.TryAdd(targetItemUID, sourceItemID);
        }

        private Transform ReplaceItemVisual((Item item, Transform itemVisual, bool special) args)
        {
            if (_itemVisuals.TryGetValue(args.item.UID, out var sourceItemID))
            {
                Logger.LogDebug($"Updating {args.item.DisplayName}'s ItemVisual to ItemID {sourceItemID}.");
                var itemPrefab = PrefabManager.GetItemPrefab(sourceItemID);
                var altItemVisual = itemPrefab.GetItemVisual(args.special);
                Logger.LogDebug($"{args.item.DisplayName}'s ItemVisual set to {altItemVisual.name}.");
                return altItemVisual;
            }
            return null;
        }
    }
}
