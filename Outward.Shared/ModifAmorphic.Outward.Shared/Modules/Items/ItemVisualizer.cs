using Localizer;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items.Models;
using ModifAmorphic.Outward.Modules.Items.Patches;
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

        private readonly IconService _iconService;
        //private IconService IconService => _iconService;

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
              typeof(ItemManagerPatches),
              typeof(ItemDisplayPatches),
              //typeof(VisualSlotPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>();

        private readonly ConcurrentDictionary<string, int> _itemVisuals = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Collection of a specific Item UID's icons and a means to get them.<br />
        /// <br />
        /// TKey: <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// <list type="number">
        /// <item>TValue: <see cref="string"/>: UID of the Item Icons are associated with.</item>
        /// <item>
        /// <see cref="ConcurrentDictionary{TKey, TValue}"/>: Keyed Collection of an Item's icons.
        /// <list type="number">
        /// <item>TKey: <see cref="string"/>: Name the icon was registered under.</item>
        /// <item>TValue: <see cref="string"/>: Physical file path to the icon's image file.</item>
        /// </list>
        /// </item>
        /// </list>
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _itemsIcons
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        private readonly HashSet<string> _displayIcons = new HashSet<string>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal ItemVisualizer(Func<ResourcesPrefabManager> prefabManagerFactory, Func<ItemManager> itemManagerFactory, IconService iconService, Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._prefabManagerFactory = prefabManagerFactory;
            _itemManagerFactory = itemManagerFactory;
            _iconService = iconService;

            ItemManagerPatches.GetVisualsByItemOverride += GetVisualsByItemOverride;
            ItemManagerPatches.GetSpecialVisualsByItemOverride += GetSpecialVisualsByItemOverride;
            ItemDisplayPatches.RefreshEnchantedIconAfter += ToggleCustomIcons;
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
        public void UnregisterItemVisual(string targetItemUID)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Unregistering target UID '{targetItemUID}'.");
            _itemVisuals.TryRemove(targetItemUID, out _);
        }
        public bool IsItemVisualRegistered(string itemUID) => _itemVisuals.ContainsKey(itemUID);

        public void RegisterAdditionalIcon(string itemUID, string iconName, string iconPath)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Registering new Icon {iconName} to Item UID '{itemUID}'. Icon file path: {iconPath}.");
            var itemIcons = _itemsIcons.GetOrAdd(itemUID, new ConcurrentDictionary<string, string>());
            _ = itemIcons.TryAdd(iconName, iconPath);
            if (!_displayIcons.Contains(iconName))
                _displayIcons.Add(iconName);
        }
        public void UnregisterAdditionalIcon(string itemUID, string iconName)
        {
            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(RegisterItemVisual)}(): Unregistering Icon {iconName} from Item UID '{itemUID}'.");
            if (_itemsIcons.TryGetValue(itemUID, out var itemIcons));
            {
                itemIcons.TryRemove(iconName, out _);
                if (itemIcons.Count == 0)
                    _itemsIcons.TryRemove(itemUID, out _);
            }
        }

        public bool IsAdditionalIconRegistered(string itemUID, string iconName) => _itemsIcons.TryGetValue(itemUID, out var icons) ? icons.ContainsKey(iconName) : false;
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

            var prefab = PrefabManager.GetItemPrefab(visualItemID);
            var prefabToggle = prefab.GetComponent<ToggleModelOnEnchant>();

            if (prefabToggle != null && item.GetComponent<ToggleModelOnEnchant>() == null)
            {
                var itemToggle = item.gameObject.AddComponent<ToggleModelOnEnchant>();
                itemToggle.ModelName = prefabToggle.ModelName;
                itemToggle.name = item.name;
                itemToggle.gameObject.SetActive(true);
            }

            ConfigureVisualToggles(item, visualItemID);

            return visual != null;
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
                var tmpItem = PrefabManager.GenerateItem(visualItemID.ToString());
                tmpItem.SetHolderUID(Global.GenerateUID());
                if (item.OwnerCharacter != null)
                    tmpItem.SetPrivateField<Item, Character>("m_ownerCharacter", item.OwnerCharacter);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): HasSpecialVisualPrefab --> Generated temporary Item and set it's owner to {tmpItem.OwnerCharacter} to retrieve ItemVisual. " +
                    $" Generated Item: {tmpItem.ItemID} - {tmpItem.DisplayName} ({tmpItem.UID})");
                visual = ItemManager.GetSpecialVisuals(tmpItem);
                
                UnityEngine.Object.Destroy(tmpItem.gameObject);
                //ItemManager.DestroyItem(tmpItem.UID);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): HasSpecialVisualPrefab --> ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                    $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            }
            else
            {
                visual = ItemManager.GetVisuals(visualItemID);
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(GetSpecialVisualsByItemOverride)}(): ItemVisual mapping found for {item.ItemID} - {item.DisplayName} ({item.UID}). Replaced " +
                        $"visuals with ItemID {visualItemID}'s ItemVisual - {visual?.name}");
            }
            if (visual != null)
            {
                visual.SetLinkedItem(item);
            }

            ConfigureVisualToggles(item, visualItemID);

            return visual != null;

        }

        private void ConfigureVisualToggles(Item item, int visualItemID)
        {
            var prefab = PrefabManager.GetItemPrefab(visualItemID);
            var prefabToggle = prefab.GetComponent<ToggleModelOnEnchant>();

            if (prefabToggle != null && item.GetComponent<ToggleModelOnEnchant>() == null)
            {
                var itemToggle = item.gameObject.AddComponent<ToggleModelOnEnchant>();
                itemToggle.ModelName = prefabToggle.ModelName;
                itemToggle.name = item.name;
                itemToggle.gameObject.SetActive(true);
            }

            var prefabSwapColor = prefab.GetComponent<SwapColor>();

            if (prefabSwapColor != null && item.GetComponent<SwapColor>() == null)
            {
                var itemSwapColor = item.gameObject.AddComponent<SwapColor>();
                itemSwapColor.DefaultColor = prefabSwapColor.DefaultColor;
                itemSwapColor.Palette = prefabSwapColor.Palette;
                itemSwapColor.name = item.name;
                itemSwapColor.gameObject.SetActive(true);
            }
        }

        private void ToggleCustomIcons(ItemDisplay itemDisplay, Item item)
        {
            var registeredItemIcons = new HashSet<string>();
            Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(ToggleCustomIcons)}(): Setting custom icons for Item {item?.ItemID} - {item?.DisplayName} ({item?.UID}) current ItemDisplay {itemDisplay.name}.");
            
            if (item != null && !string.IsNullOrEmpty(item.UID) && _itemsIcons.TryGetValue(item.UID, out var itemIcons))
            {
                foreach (var iconKvp in itemIcons)
                {
                    _iconService.GetOrAddIcon(itemDisplay, iconKvp.Key, iconKvp.Value, true);
                    registeredItemIcons.Add(iconKvp.Key);
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(ToggleCustomIcons)}(): Added icon {iconKvp.Key} to ItemDisplay {itemDisplay.name}.");
                }
            }

            foreach (var iconName in _displayIcons)
            {
                if (!registeredItemIcons.Contains(iconName))
                {
                    if (_iconService.TryDeactivateIcon(itemDisplay, iconName))
                    {
                        Logger.LogTrace($"{nameof(ItemVisualizer)}::{nameof(ToggleCustomIcons)}(): Deactivated icon {iconName} for ItemDisplay {itemDisplay.name}.");
                    }
                }
            }
        }
        #region POC Work
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
            if (string.IsNullOrEmpty(item.UID) || (!_itemVisuals.TryGetValue(item.UID, out var visualItemID) && !item.DisplayName.Contains("Virgin")))
            {
                return false;
            }

            //var vPreFab = PrefabManager.GetItemPrefab(visualItemID);

            //if (!vPreFab.HasSpecialVisualPrefab && !item.HasSpecialVisualPrefab)
            //{
            //    return false;
            //}

            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Starting PositionVisuals processing for {item.ItemID} - {item.DisplayName} ({item.UID}) and " +
                    $"visualItemID {visualItemID}.");

            //terrible idea again. doesn't work. ItemVisual gets linked/cached and doesn't change until unloaded
            //CopyItemVisual(vPreFab, item);

            //TODO unwind this mess and figure out how special visuals get called.
            var vs = VisualSlotWrapper.Wrap(visualSlot);
            var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);

            if (!Application.isPlaying)
            {
                vs.m_currentItem = item;
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Application.isPlaying == false.  m_currentItem set to item {item.ItemID} - {item.DisplayName} ({item.UID}).");
            }
            else
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Application.isPlaying == true for item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                if (vs.m_editorCurrentVisuals != null)
                {
                    vs.m_currentVisual = vs.m_editorCurrentVisuals;
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_editorCurrentVisuals != null. Set m_currentVisual to m_editorCurrentVisuals {vs.m_editorCurrentVisuals} for item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                }
                if (item == vs.m_currentItem)
                {
                    
                    if (!item.HasSpecialVisualPrefab)
                    {
                        Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): !item.HasSpecialVisualPrefab. Calling item.LinkVisuals({vs.m_currentVisual}, false). item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                        item.LinkVisuals(vs.m_currentVisual, _setParent: false);
                    }
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): item == m_currentItem.  Calling PositionVisuals() for item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                    vs.PositionVisuals();
                    return true;
                }
                if (vs.m_currentItem != null)
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): vs.m_currentItem != null. m_currentItem is [{vs.m_currentItem.ItemID} - {vs.m_currentItem.DisplayName} ({vs.m_currentItem.UID})]. Calling PutBackVisuals(). item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                    visualSlot.PutBackVisuals();
                }
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentItem [{vs.m_currentItem?.ItemID} - {vs.m_currentItem?.DisplayName} ({vs.m_currentItem?.UID})] being set to param item [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                vs.m_currentItem = item;
            }
            if (vs.m_currentVisual == null)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentVisual == null. item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                if (!item.HasSpecialVisualPrefab)
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): !item.HasSpecialVisualPrefab (No Special). m_currentVisual [{vs.m_currentVisual}] set to item.LoadedVisual [{item.LoadedVisual}]. " +
                        $"m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}] set to m_currentVisual [{vs.m_currentVisual}]. " +
                        $"item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                    vs.m_currentVisual = item.LoadedVisual;
                    vs.m_editorCurrentVisuals = vs.m_currentVisual;
                }
                else
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): item.HasSpecialVisualPrefab == true. m_currentVisual [{vs.m_currentVisual}]. m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}]. " +
                        $"item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                    vs.m_currentVisual = ItemManager.GetSpecialVisuals(item);
                    vs.m_editorCurrentVisuals = vs.m_currentVisual;
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): ItemManager.GetSpecialVisuals({item.ItemID} - {item.DisplayName} ({item.UID})). m_currentVisual now [{vs.m_currentVisual}] and m_editorCurrentVisuals [{vs.m_currentVisual}].");
                    if ((bool)vs.m_currentVisual)
                    {
                        Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentVisual.Show(). m_currentVisual [{vs.m_currentVisual}]. item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                        vs.m_currentVisual.Show();
                    }
                    if (item is Equipment)
                    {
                        Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): item is Equipment. IsEnchanted == {item.IsEnchanted}. Calling (item as Equipment).SetSpecialVisuals({vs.m_currentVisual}) item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                        (item as Equipment).SetSpecialVisuals(vs.m_currentVisual);
                    }
                }
            }
            if (Application.isPlaying)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Application.isPlaying == true. Setting m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}] to  m_currentVisual [{vs.m_currentVisual}].item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                vs.m_editorCurrentVisuals = vs.m_currentVisual;
            }
            else if (vs.m_currentItem != null)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentItem != null. Setting m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}] to  m_currentVisual [{vs.m_currentVisual}].  item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                vs.m_editorCurrentVisuals = vs.m_currentVisual;
            }

            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Calling PositionVisuals() then returning true.  item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
            vs.PositionVisuals();

            if (prefab.HasSpecialVisualPrefab && item is Equipment)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Potential postifix. IsEnchanted == {item.IsEnchanted}. Calling (item as Equipment).SetSpecialVisuals({vs.m_currentVisual}) item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                (item as Equipment).SetSpecialVisuals(vs.m_currentVisual);
            }

            return true;
        }
        private bool PositionVisualsOverrideOriginal(VisualSlot visualSlot, ref Item item)
        {
            if (string.IsNullOrEmpty(item.UID) || (!_itemVisuals.TryGetValue(item.UID, out var visualItemID) && !item.DisplayName.Contains("Virgin")))
            {
                return false;
            }

            //var vPreFab = PrefabManager.GetItemPrefab(visualItemID);

            //if (!vPreFab.HasSpecialVisualPrefab && !item.HasSpecialVisualPrefab)
            //{
            //    return false;
            //}

            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Starting PositionVisuals processing for {item.ItemID} - {item.DisplayName} ({item.UID}) and " +
                    $"visualItemID {visualItemID}.");

            //terrible idea again. doesn't work. ItemVisual gets linked/cached and doesn't change until unloaded
            //CopyItemVisual(vPreFab, item);

            //TODO unwind this mess and figure out how special visuals get called.
            var vs = VisualSlotWrapper.Wrap(visualSlot);

            if (!Application.isPlaying)
            {
                vs.m_currentItem = item;
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Application.isPlaying == false.  m_currentItem set to item {item.ItemID} - {item.DisplayName} ({item.UID}).");
            }
            else
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Application.isPlaying == true for item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                if (vs.m_editorCurrentVisuals != null)
                {
                    vs.m_currentVisual = vs.m_editorCurrentVisuals;
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_editorCurrentVisuals != null. Set m_currentVisual to m_editorCurrentVisuals {vs.m_editorCurrentVisuals} for item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                }
                if (item == vs.m_currentItem)
                {

                    if (!item.HasSpecialVisualPrefab)
                    {
                        Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): !item.HasSpecialVisualPrefab. Calling item.LinkVisuals({vs.m_currentVisual}, false). item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                        item.LinkVisuals(vs.m_currentVisual, _setParent: false);
                    }
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): item == m_currentItem.  Calling PositionVisuals() for item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                    vs.PositionVisuals();
                    return true;
                }
                if (vs.m_currentItem != null)
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): vs.m_currentItem != null. m_currentItem is [{vs.m_currentItem.ItemID} - {vs.m_currentItem.DisplayName} ({vs.m_currentItem.UID})]. Calling PutBackVisuals(). item: {item.ItemID} - {item.DisplayName} ({item.UID}).");
                    visualSlot.PutBackVisuals();
                }
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentItem [{vs.m_currentItem?.ItemID} - {vs.m_currentItem?.DisplayName} ({vs.m_currentItem?.UID})] being set to param item [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                vs.m_currentItem = item;
            }
            if (vs.m_currentVisual == null)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentVisual == null. item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                if (!item.HasSpecialVisualPrefab)
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): !item.HasSpecialVisualPrefab (No Special). m_currentVisual [{vs.m_currentVisual}] set to item.LoadedVisual [{item.LoadedVisual}]. " +
                        $"m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}] set to m_currentVisual [{vs.m_currentVisual}]. " +
                        $"item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                    vs.m_currentVisual = item.LoadedVisual;
                    vs.m_editorCurrentVisuals = vs.m_currentVisual;
                }
                else
                {
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): item.HasSpecialVisualPrefab == true. m_currentVisual [{vs.m_currentVisual}]. m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}]. " +
                        $"item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                    vs.m_currentVisual = ItemManager.GetSpecialVisuals(item);
                    vs.m_editorCurrentVisuals = vs.m_currentVisual;
                    Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): ItemManager.GetSpecialVisuals({item.ItemID} - {item.DisplayName} ({item.UID})). m_currentVisual now [{vs.m_currentVisual}] and m_editorCurrentVisuals [{vs.m_currentVisual}].");
                    if ((bool)vs.m_currentVisual)
                    {
                        Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentVisual.Show(). m_currentVisual [{vs.m_currentVisual}]. item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                        vs.m_currentVisual.Show();
                    }
                    if (item is Equipment)
                    {
                        Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): item is Equipment. IsEnchanted == {item.IsEnchanted}. Calling (item as Equipment).SetSpecialVisuals({vs.m_currentVisual}) item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                        (item as Equipment).SetSpecialVisuals(vs.m_currentVisual);
                    }
                }
            }
            if (Application.isPlaying)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Application.isPlaying == true. Setting m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}] to  m_currentVisual [{vs.m_currentVisual}].item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                vs.m_editorCurrentVisuals = vs.m_currentVisual;
            }
            else if (vs.m_currentItem != null)
            {
                Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): m_currentItem != null. Setting m_editorCurrentVisuals [{vs.m_editorCurrentVisuals}] to  m_currentVisual [{vs.m_currentVisual}].  item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
                vs.m_editorCurrentVisuals = vs.m_currentVisual;
            }

            Logger.LogDebug($"{nameof(ItemVisualizer)}::{nameof(PositionVisualsOverride)}(): Calling PositionVisuals() then returning true.  item: [{item.ItemID} - {item.DisplayName} ({item.UID})].");
            vs.PositionVisuals();

            return true;
        }
        #endregion
    }
}
