using Localizer;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items.Models;
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
              typeof(ItemManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>();

        private readonly ConcurrentDictionary<string, int> _itemVisuals = new ConcurrentDictionary<string, int>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ItemVisualizer(Func<ResourcesPrefabManager> prefabManagerFactory, Func<ItemManager> itemManagerFactory, Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._prefabManagerFactory = prefabManagerFactory;
            _itemManagerFactory = itemManagerFactory;

            ItemManagerPatches.GetVisualsByItemOverride += GetVisualsByItemOverride;
            ItemManagerPatches.GetSpecialVisualsByItemOverride += GetSpecialVisualsByItemOverride;
            //VisualSlotPatches.PositionVisualsOverride += PositionVisualsOverride;
        }


        /// <summary>
        /// Registers a UID to use the source Item IDs visuals.
        /// </summary>
        /// <param name="visualItemID">The ItemID whose visuals should be used.</param>
        /// <param name="targetItemUID">The target item UID that should use the <paramref name="visualItemID"/> item's visuals.</param>
        public void RegisterItemVisual(int visualItemID, string targetItemUID)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Registering target UID '{targetItemUID}' to use ItemID {visualItemID} visuals.");
            _itemVisuals.TryAdd(targetItemUID, visualItemID);
        }
        public bool IsItemVisualRegistered(string itemUID) => _itemVisuals.ContainsKey(itemUID);

        private bool GetVisualsByItemOverride(Item item, out ItemVisual visual)
        {
            if (!_itemVisuals.TryGetValue(item.UID, out var visualItemID))
            {
                visual = null;
                Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): No custom ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}).");
                return false;
            }

            //this doesn't seem necessary.
            item.SetPrivateField<Item, ItemVisual>("m_loadedVisual", null);
            visual = ItemManager.GetVisuals(visualItemID);
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            return visual != null;

            //return TryGetItemVisual(item, visualItemID, out visual);

            //if (ItemManager.Instance != null)
            //{
            //    visual = ItemManager.GetVisuals(visualItemID);
            //    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
            //        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            //    return true;
            //}

            //var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);
            //var instantiateInfo = typeof(ItemManager).GetMethod("InstantiateVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            //var transform = instantiateInfo.Invoke(null, new object[] { prefab }) as Transform;

            //visual = transform?.GetComponent<ItemVisual>();

            //Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Tried to replace " +
            //        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            //return transform != null;
        }
        //TODO: This isn't working exactly right. Virgin armor loads green enchanted.
        private bool GetSpecialVisualsByItemOverride(Item item, out ItemVisual visual)
        {
            if (!_itemVisuals.TryGetValue(item.UID, out var visualItemID))
            {
                visual = null;
                Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): No custom ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}).");
                return false;
            }
            item.SetPrivateField<Item, ItemVisual>("m_loadedVisual", null);
            //Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): Trying to get visual for {item.ItemID} - {item.DisplayName} ({item.UID}). Visual ItemID - {visualItemID}");
            var prefab = PrefabManager.GetItemPrefab(visualItemID);
            if (prefab.HasSpecialVisualPrefab)
            {
                visual = ItemManager.GetSpecialVisuals(prefab);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): HasSpecialVisualPrefab --> ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            }
            else
            {
                visual = ItemManager.GetVisuals(visualItemID);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            }

            return visual != null;

            //return TryGetItemVisual(item, visualItemID, out visual);

            //var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);

            //if (ItemManager.Instance != null)
            //{
            //    visual = ItemManager.Instance.InvokePrivateMethod<ItemManager, ItemVisual>("Internal_GetSpecialVisuals", new object[] { prefab });
            //    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
            //        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            //    return true;
            //}

            //var instantiateInfo = typeof(ItemManager).GetMethod("InstantiateSpecialVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            //var transform = instantiateInfo.Invoke(null, new object[] { prefab }) as Transform;

            //visual = transform?.GetComponent<ItemVisual>();
            //Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Tried to replace " +
            //        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");

            //return transform != null;
        }

        private bool TryUnregisterItemVisual(Item containedItem, out (string UID, int VisualItemID) visualMap)
        {
            if (_itemVisuals.TryRemove(containedItem.UID, out int visualItemID))
            {
                visualMap = (containedItem.UID, visualItemID);
                containedItem.SetPrivateField<Item, ItemVisual>("m_loadedVisual", null);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Unregistered target UID '{containedItem.UID}' from ItemID {visualItemID} visuals.\n" +
                    $"\tm_loadedVisual: {containedItem.GetLoadedVisual()}");

                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Visuals: \n" +
                    $"\tLoadedVisual: {containedItem.LoadedVisual}" +
                    $"\tCurrentVisual: {containedItem.CurrentVisual}");
                return true;
            }

            visualMap = default;
            return false;
        }

        private bool TryGetItemVisual(Item item, int visualItemID, out ItemVisual itemVisual)
        {
            var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);
            //var visualItem = UnityEngine.Object.Instantiate(prefab, item.transform.parent);
            //visualItem.hideFlags = HideFlags.HideAndDontSave;
            //if (item.OwnerCharacter != null)
            //    visualItem.OnContainerChangedOwner(item.OwnerCharacter);

            item.SetPrivateField<Item, ItemVisual>("m_loadedVisual", null);
            
            //Handle Non Special Visuals
            //if (!visualItem.HasSpecialVisualPrefab)
            //{
                if (ItemManager.Instance != null)
                {
                    itemVisual = ItemManager.GetVisuals(visualItemID);
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(TryGetItemVisual)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                        $"visuals with ItemID {visualItemID}'s ItemVisual - {itemVisual?.name}");
                    
                    return true;
                }

                var instantiateInfo = typeof(ItemManager).GetMethod("InstantiateVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var transform = instantiateInfo.Invoke(null, new object[] { prefab }) as Transform;

                itemVisual = transform?.GetComponent<ItemVisual>();

                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(TryGetItemVisual)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Tried to replace " +
                        $"visuals with ItemID {visualItemID}'s ItemVisual - {itemVisual?.name}");
                return transform != null;
            //}

            //Handle Special Visual Prefabs

            if (ItemManager.Instance != null)
            {
                itemVisual = ItemManager.Instance.InvokePrivateMethod<ItemManager, ItemVisual>("Internal_GetSpecialVisuals", new object[] { prefab });
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(TryGetItemVisual)}(): HasSpecialVisualPrefab --> ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {itemVisual?.name}");
                return true;
            }

            var instantiateSpecInfo = typeof(ItemManager).GetMethod("InstantiateSpecialVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var specTransform = instantiateSpecInfo.Invoke(null, new object[] { prefab }) as Transform;

            itemVisual = specTransform?.GetComponent<ItemVisual>();
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(TryGetItemVisual)}(): HasSpecialVisualPrefab --> ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Tried to replace " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {itemVisual?.name}");

            return specTransform != null;

        }


        private void CopyItemVisual(Item source, Item target)
        {
            var targetActive = target.gameObject.activeSelf;
            target.gameObject.SetActive(false);
            var prefabName = source.GetVisualPrefabName();

            target.SetSpecialVisualPrefabDefaultPath(source.GetSpecialVisualPrefabDefaultPath())
                .SetVisualPrefabPath(source.GetVisualPrefabPath())
                .SetSpecialVisualPrefabFemalePath(source.GetSpecialVisualPrefabFemalePath())
                .SetVisualPrefabName(prefabName)
                .SetPrivateField<Item, ItemVisual>("m_loadedVisual", null);
            //.SetPrivateField<Item, ItemVisual>("m_loadedVisual", 
            //                                  source.GetPrivateField<Item, ItemVisual>("m_loadedVisual"));

            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(CopyItemVisual)}: Set target {target.UID} {target.ItemID} - {target.DisplayName} to {source.ItemID} - {source.DisplayName} Visuals: \n" +
                $"\tsource.GetSpecialVisualPrefabDefaultPath(): {source.GetSpecialVisualPrefabDefaultPath()} | target.GetSpecialVisualPrefabDefaultPath(): {target.GetSpecialVisualPrefabDefaultPath()}\n" +
                   $"\tsource.GetVisualPrefabPath(): {source.GetVisualPrefabPath()} | target.GetVisualPrefabPath(): {target.GetVisualPrefabPath()}\n" +
                   $"\tsource.GetSpecialVisualPrefabFemalePath(): {source.GetSpecialVisualPrefabFemalePath()} | target.GetSpecialVisualPrefabFemalePath(): {target.GetSpecialVisualPrefabFemalePath()}\n" +
                   $"\tsource.GetVisualPrefabName(): {source.GetVisualPrefabName()} | target.GetVisualPrefabName(): {target.GetVisualPrefabName()}\n" 
                   );

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

            //target.LoadedVisual.SetTransformParent(target.transform);
            target.gameObject.SetActive(targetActive);
        }
        private bool PositionVisualsOverride(VisualSlot visualSlot, ref Item item)
        {
            if (string.IsNullOrEmpty(item.UID) || !_itemVisuals.TryGetValue(item.UID, out var visualItemID))
            {
                return false;
            }

            var vPreFab = PrefabManager.GetItemPrefab(visualItemID);

            if (!vPreFab.HasSpecialVisualPrefab && !item.HasSpecialVisualPrefab)
            {
                return false;
            }

            //terrible idea again. doesn't work. ItemVisual gets linked/cached and doesn't change until unloaded
            //CopyItemVisual(vPreFab, item);

            //TODO unwind this mess and figure out how special visuals get called.
            var vs = VisualSlotWrapper.Wrap(visualSlot);

            if (!Application.isPlaying)
            {
                vs.m_currentItem = item;
            }
            else
            {
                if (vs.m_editorCurrentVisuals != null)
                {
                    vs.m_currentVisual = vs.m_editorCurrentVisuals;
                }
                if (item == vs.m_currentItem)
                {
                    if (!item.HasSpecialVisualPrefab)
                    {
                        item.LinkVisuals(vs.m_currentVisual, _setParent: false);
                    }
                    vs.PositionVisuals();
                    //return;
                }
                if (vs.m_currentItem != null)
                {
                    visualSlot.PutBackVisuals();
                }
                vs.m_currentItem = item;
            }
            if (vs.m_currentVisual == null)
            {
                if (!item.HasSpecialVisualPrefab)
                {
                    vs.m_currentVisual = item.LoadedVisual;
                    vs.m_editorCurrentVisuals = vs.m_currentVisual;
                }
                else
                {
                    vs.m_currentVisual = ItemManager.GetSpecialVisuals(item);
                    vs.m_editorCurrentVisuals = vs.m_currentVisual;
                    if ((bool)vs.m_currentVisual)
                    {
                        vs.m_currentVisual.Show();
                    }
                    if (item is Equipment)
                    {
                        (item as Equipment).SetSpecialVisuals(vs.m_currentVisual);
                    }
                }
            }
            if (Application.isPlaying)
            {
                vs.m_editorCurrentVisuals = vs.m_currentVisual;
            }
            else if (vs.m_currentItem != null)
            {
                vs.m_editorCurrentVisuals = vs.m_currentVisual;
            }
            vs.PositionVisuals();

            return true;
        }
    }
}
