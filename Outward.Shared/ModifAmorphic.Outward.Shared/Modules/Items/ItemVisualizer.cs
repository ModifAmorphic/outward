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
        private readonly ConcurrentDictionary<string, bool> _blockRecurse = new ConcurrentDictionary<string, bool>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ItemVisualizer(Func<ResourcesPrefabManager> prefabManagerFactory, Func<ItemManager> itemManagerFactory, Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._prefabManagerFactory = prefabManagerFactory;
            _itemManagerFactory = itemManagerFactory;
            //ItemPatches.LoadVisualsOverride += (Item item) => LoadVisualsOverride(ref item);
            //ItemPatches.GetItemVisualBefore += (args) => TryReplaceItemVisuals(args.item);
            ItemManagerPatches.GetVisualsByItemOverride += GetVisualsByItemOverride;
            ItemManagerPatches.GetSpecialVisualsByItemOverride += GetSpecialVisualsByItemOverride;
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

        public bool RecurseCheckFailed(string uid)
        {
            if (_blockRecurse.ContainsKey(uid))
            {
                Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(RecurseCheckFailed)}(): UID {uid} failed recurse check. ItemVisuals are likely already processing.");
                return true;
            }
            Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(RecurseCheckFailed)}(): UID {uid} passed recurse check. Ok to being ItemVisuals processing. Blocking future calls for UID until complete.");
            _blockRecurse.TryAdd(uid, default);

            return false;
        }

        public void RegisterItemVisual(int sourceItemID, string targetItemUID)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Registering target UID '{targetItemUID}' to use ItemID {sourceItemID} visuals.");
            _itemVisuals.TryAdd(targetItemUID, sourceItemID);

            //if (_itemVisuals.TryAdd(targetItemUID, sourceItemID))
            //    TryReplaceItemVisuals(sourceItemID, targetItemUID);
        }
        public void RegisterItemVisual(int sourceItemID, Item targetItem)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Registering target {targetItem.DisplayName} ({targetItem.UID}) to use ItemID {sourceItemID} visuals.");
            _itemVisuals.TryAdd(targetItem.UID, sourceItemID);

            //if (_itemVisuals.TryAdd(targetItem.UID, sourceItemID))
            //    TryReplaceItemVisuals(targetItem);
        }


        private bool TryReplaceItemVisuals(Item item)
        {
            if (RecurseCheckFailed(item.UID))
                return false;

            try
            {
                //(Item item, bool special) = (args.item, args.special);
                if (_itemVisuals.TryGetValue(item.UID, out var sourceItemID))
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(TryReplaceItemVisuals)}(item): Updating {item.UID}'s ItemVisual to ItemID {sourceItemID}.");
                    var source = GetPrefabVisualized(sourceItemID);
                    if (source == null)
                        return false;

                    CopyItemVisual(source, item);
                    UnityEngine.Object.Destroy(source);
                    return true;
                }
                return false;
            }
            finally
            {
                _blockRecurse.TryRemove(item.UID, out _);
            }
        }
       
        private bool TryReplaceItemVisuals(int sourceItemID, string targetUID)
        {
            if (RecurseCheckFailed(targetUID))
                return false;
            try
            {

                if (PrefabManager == null || ItemManager == null)
                    return false;

                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(ItemVisualizer)}(): Updating {targetUID}'s ItemVisual to ItemID {sourceItemID}.");

                var source = GetPrefabVisualized(sourceItemID);
                var target = ItemManager.GetItem(targetUID);
                //UnityEngine.Object.DontDestroyOnLoad(target);
                if (source == null || target == null)
                    return false;

                CopyItemVisual(source, target);
                UnityEngine.Object.Destroy(source);

                return true;
            }
            finally
            {
                _blockRecurse.TryRemove(targetUID, out _);
            }
        }
        private Item GetPrefabVisualized(int itemID)
        {
            var prefab = UnityEngine.Object.Instantiate(PrefabManager.GetItemPrefab(itemID));
            prefab.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            //var prefabVisual = prefab.GetItemVisual(prefab.HasSpecialVisualPrefab);
            UnityEngine.Object.DontDestroyOnLoad(prefab);
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetPrefabVisualized)}: Got prefab {prefab.DisplayName}'s.  prefab.LoadedVisual is {prefab.LoadedVisual}");

            //if (!prefab.HasSpecialVisualPrefab)
            //{
            //    if (prefab.LoadedVisual == null)
            //    {
            //        var currentVisual = ItemManager.GetVisuals(prefab);
            //        currentVisual?.Show();
            //    }
            //}
            //else
            //{
            //    var specialVisual = ItemManager.GetSpecialVisuals(prefab);
            //    if (specialVisual != null)
            //    {
            //        specialVisual.Show();
            //    }
            //    if (prefab is Equipment equipment)
            //    {
            //        if (prefab is Armor)
            //            equipment.SetSpecialVisuals(specialVisual as ArmorVisuals);
            //        else
            //            equipment.SetSpecialVisuals(specialVisual);
            //    }
            //}
            return prefab;
        }
        private void CopyItemVisual(Item source, Item target)
        {
            var targetActive = target.gameObject.activeSelf;
            target.gameObject.SetActive(false);
            target.SetSpecialVisualPrefabDefaultPath(source.GetSpecialVisualPrefabDefaultPath())
                .SetVisualPrefabPath(source.GetVisualPrefabPath())
                .SetSpecialVisualPrefabFemalePath(source.GetSpecialVisualPrefabFemalePath())
                .SetVisualPrefabName(source.GetVisualPrefabName())
                .SetPrivateField<Item, ItemVisual>("m_loadedVisual", source.LoadedVisual);

            //Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(CopyItemVisual)}: Set target LoadedVisual to source ItemVisual.\n" +
            //    $"\tsource.LoadedVisual: {source.LoadedVisual}\n" +
            //       $"\ttarget.m_loadedVisual: {target.GetLoadedVisual()}\n" +
            //       $"\ttarget.LoadedVisual: {target.LoadedVisual}");

            //if (target is Armor && source is Armor sArmor)
            //{
            //    target.SetLoadedVisual(source.LoadedVisual as ArmorVisuals);
            //    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(CopyItemVisual)}: Set target LoadedVisual to source ArmorVisuals. sArmor.LoadedVisual: {sArmor.LoadedVisual}. " +
            //        $"target.LoadedVisual: {target.LoadedVisual}. target.LoadedVisual is ArmorVisuals ? {target.LoadedVisual is ArmorVisuals}");
            //}
            //else
            //{
            //    target.SetLoadedVisual(source.LoadedVisual);
            //    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(CopyItemVisual)}: Set target LoadedVisual to source ItemVisual. source.LoadedVisual: {source.LoadedVisual}. " +
            //        $"target.LoadedVisual: {target.LoadedVisual}");
            //}
            
            target.LoadedVisual.SetTransformParent(target.transform);
            target.gameObject.SetActive(targetActive);
        }
        //private bool LoadVisualsOverride(ref Item item)
        //{
        //    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(LoadVisualsOverride)}(item): Trying to replace {item.UID}'s ItemVisuals.");
        //    if (!TryReplaceItemVisuals(item))
        //        return false;

        //    item.InvokePrivateMethod<Item>("SendOnProcessVisual");
        //    return true;
        //}
        //private Transform ReplaceItemVisual((Item item, Transform itemVisual, bool special) args)
        //{
        //    (Item item, Transform itemVisual, bool special) = (args.item, args.itemVisual, args.special);
        //    if (_itemVisuals.TryGetValue(item.UID, out var sourceItemID))
        //    {
        //        Logger.LogDebug($"Updating {item.DisplayName}'s ItemVisual to ItemID {sourceItemID}.");
        //        var itemPrefab = PrefabManager.GetItemPrefab(sourceItemID);
        //        var altItemVisual = itemPrefab.GetItemVisual(special);
        //        CopyItemVisuals(item, itemPrefab);
        //        Logger.LogDebug($"{item.DisplayName}'s ItemVisual set to {altItemVisual.name}.");
        //        //return item.LoadedVisual;
        //    }
        //    //return null;
        //}
    }
}
