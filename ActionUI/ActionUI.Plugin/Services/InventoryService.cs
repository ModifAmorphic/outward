using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public class InventoryService : IDisposable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private CharacterInventory _characterInventory => _character.Inventory;
        private CharacterEquipment _characterEquipment => _character.Inventory.Equipment;
        private readonly ProfileManager _profileManager;
        private IActionUIProfile _profile => _profileManager.ProfileService.GetActiveProfile();
        private EquipmentSetsSettingsProfile _equipSetsProfile => _profileManager.ProfileService.GetActiveProfile().EquipmentSetsSettingsProfile;
        private EquipmentSetMenu _equipmentSetsMenus;
        private readonly LevelCoroutines _coroutines;

        public bool GetIsStashEquipEnabled() => _equipSetsProfile.StashEquipEnabled && (GetIsAreaStashEnabled() || _equipSetsProfile.StashEquipAnywhereEnabled);
        public bool GetIsStashUnequipEnabled() => _equipSetsProfile.StashUnequipEnabled && (GetIsAreaStashEnabled() || _equipSetsProfile.StashUnequipAnywhereEnabled);

        private bool _firstSetsAddedComplete;
        private bool _equipmentSetsEnabled;

        private bool disposedValue;

        private Dictionary<EquipmentSlot.EquipmentSlotIDs, EquipSlots> OutwardSlotIDsXRef = new Dictionary<EquipmentSlot.EquipmentSlotIDs, EquipSlots>()
        {
            { EquipmentSlot.EquipmentSlotIDs.Helmet, EquipSlots.Head},
            { EquipmentSlot.EquipmentSlotIDs.Chest, EquipSlots.Chest},
            { EquipmentSlot.EquipmentSlotIDs.Foot, EquipSlots.Feet},
            { EquipmentSlot.EquipmentSlotIDs.RightHand, EquipSlots.RightHand},
            { EquipmentSlot.EquipmentSlotIDs.LeftHand, EquipSlots.LeftHand},
        };
        private Dictionary<EquipSlots, EquipmentSlot.EquipmentSlotIDs> ActionUiEquipSlotsXRef = new Dictionary<EquipSlots, EquipmentSlot.EquipmentSlotIDs>()
        {
            { EquipSlots.Head, EquipmentSlot.EquipmentSlotIDs.Helmet },
            { EquipSlots.Chest, EquipmentSlot.EquipmentSlotIDs.Chest },
            { EquipSlots.Feet, EquipmentSlot.EquipmentSlotIDs.Foot },
            { EquipSlots.RightHand, EquipmentSlot.EquipmentSlotIDs.RightHand },
            { EquipSlots.LeftHand, EquipmentSlot.EquipmentSlotIDs.LeftHand },
        };

        public InventoryService(Character character, ProfileManager profileManager, EquipmentSetMenu equipmentSetsMenus, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _equipmentSetsMenus = equipmentSetsMenus;
            _coroutines = coroutines;
            _getLogger = getLogger;
        }

        public void Start()
        {
            _equipmentSetsEnabled = _profile.EquipmentSetsEnabled;
            CharacterInventoryPatches.AfterInventoryIngredients += AddStashIngredients;
            EquipmentMenuPatches.AfterShow += ShowEquipmentSetMenu;
            EquipmentMenuPatches.AfterOnHide += HideEquipmentSetMenu;
            ItemDisplayPatches.AfterSetReferencedItem += AddEquipmentSetIcon;
            NetworkLevelLoaderPatches.MidLoadLevelAfter += ShowHideSkillsMenu;
            _profileManager.ProfileService.OnActiveProfileChanged.AddListener(ProfileChanged);
            _profileManager.ProfileService.OnActiveProfileSwitched.AddListener(ProfileSwitched);

            var armorService = (ArmorSetsJsonService)_profileManager.ArmorSetService;
            var weaponService = (WeaponSetsJsonService)_profileManager.WeaponSetService;

            if (_profile.EquipmentSetsEnabled)
            {
                AddEquipmentSets<ArmorSetSkill>(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets);
                AddEquipmentSets<WeaponSetSkill>(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets);
            }
            _firstSetsAddedComplete = true;
        }

        private Dictionary<EquipSlots, EquipSkillPreview> _cachedSkillPreviews = new Dictionary<EquipSlots, EquipSkillPreview>();
        public EquipSkillPreview GetEquipSkillPreview(IEquipmentSet set)
        {
            if (_cachedSkillPreviews.TryGetValue(set.IconSlot, out var skillPreview))
                return skillPreview;

            _cachedSkillPreviews.Add(set.IconSlot, new EquipSkillPreview(_characterEquipment.GetMatchingSlot(ActionUiEquipSlotsXRef[set.IconSlot])));
            return _cachedSkillPreviews[set.IconSlot];
        }
        private void ClearSkillPreviewCache()
        {
            var skillPreviews = _cachedSkillPreviews.Values.ToArray();
            for (int i = 0; i < skillPreviews.Length; i++)
                skillPreviews[i].Dispose();

            _cachedSkillPreviews.Clear();
        }

        private void ShowHideSkillsMenu(NetworkLevelLoader networkLevelLoader)
        {
            if (_character?.CharacterUI == null)
                return;
            //Creates ItemDisplays for equipment set skills.
            _coroutines.InvokeAfterLevelAndPlayersLoaded(networkLevelLoader,
                () =>
                {
                    _character.CharacterUI.ShowMenu(CharacterUI.MenuScreens.Skills);
                    _character.CharacterUI.HideMenu(CharacterUI.MenuScreens.Skills);
                }, 120);
        }

        private void HideEquipmentSetMenu(EquipmentMenu obj)
        {
            if (obj.LocalCharacter.UID != _character.UID || (!_profileManager.ProfileService.GetActiveProfile().EquipmentSetsEnabled && !_equipmentSetsMenus.gameObject.activeSelf))
                return;

            _equipmentSetsMenus.Hide();
        }

        private void ShowEquipmentSetMenu(EquipmentMenu equipMenu)
        {

            if (equipMenu.LocalCharacter.UID != _character.UID || !_profileManager.ProfileService.GetActiveProfile().EquipmentSetsEnabled)
                return;

            if (_equipmentSetsMenus.transform.parent != equipMenu.transform)
                _equipmentSetsMenus.transform.SetParent(equipMenu.transform);
            _equipmentSetsMenus.Show();

            _coroutines.DoNextFrame(() =>
            {
                var mainMenuTransform = _character.CharacterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel") as RectTransform;
                var menuBg = mainMenuTransform.Find("Background").GetComponent<Image>();
                var refCorners = new Vector3[4];
                ((RectTransform)menuBg.transform).GetWorldCorners(refCorners);
                var topLeft = refCorners[1];

                Logger.LogDebug($"(x={topLeft.x}, y={topLeft.x})");

                var equipRectTransform = _equipmentSetsMenus.transform as RectTransform;
                equipRectTransform.anchorMax = new Vector2(1, 1);
                equipRectTransform.anchorMin = new Vector2(1, 1);
                //var xPos = topLeft.x - equipRectTransform.rect.width + 30;
                //var yPos = topLeft.y + 10;
                var xPos = topLeft.x; // + 20;
                var yPos = topLeft.y; // + 13;
                equipRectTransform.position = new Vector3(xPos, yPos);
                equipRectTransform.anchoredPosition = new Vector3(equipRectTransform.anchoredPosition.x + 25, equipRectTransform.anchoredPosition.y);
                equipRectTransform.GetComponent<Image>().material = menuBg.material;
            });
            //var equipXform = _equipmentSetsMenus.transform as RectTransform;
            //var charMenus = _character.CharacterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel") as RectTransform;
            //var xpos = charMenus.anchoredPosition.x - equipXform.rect.width + 30;
            //var ypos = charMenus.anchoredPosition.y + 10;
            //equipXform.anchoredPosition = new Vector2(xpos, ypos);
            //var menuBg = charMenus.Find("Background").GetComponent<Image>();
            //equipXform.GetComponent<Image>().material = menuBg.material;

        }

        private void AddStashIngredients(CharacterInventory characterInventory, Character character, Tag craftingStationTag, ref DictionaryExt<int, CompatibleIngredient> sortedIngredients)
        {
            if (!_profileManager.ProfileService.GetActiveProfile().StashCraftingEnabled
                || character.Stash == null
                || character.UID != _character.UID
                || !GetIsAreaStashEnabled())
                return;

            var stashItems = character.Stash.GetContainedItems().ToList();

            var inventoryIngredients = characterInventory.GetType().GetMethod("InventoryIngredients", BindingFlags.NonPublic | BindingFlags.Instance);
            inventoryIngredients.Invoke(characterInventory, new object[] { craftingStationTag, sortedIngredients, stashItems });
        }

        public bool GetIsAreaStashEnabled() => TryGetCurrentAreaEnum(out var area) && InventorySettings.StashAreas.Contains(area);
        private bool TryGetCurrentAreaEnum(out AreaManager.AreaEnum area)
        {
            area = default;
            var sceneName = AreaManager.Instance?.CurrentArea?.SceneName;
            if (string.IsNullOrEmpty(sceneName))
                return false;

            area = (AreaManager.AreaEnum)AreaManager.Instance.GetAreaIndexFromSceneName(sceneName);
            return true;
        }

        private void ProfileChanged(IActionUIProfile profile)
        {
            Logger.LogDebug($"{nameof(InventoryService)}::{nameof(ProfileChanged)}: Profile changed or switched. Profile is '{profile.Name}'. EquipmentSetsEnabled == {profile.EquipmentSetsEnabled}.");

            if (_equipmentSetsEnabled != profile.EquipmentSetsEnabled)
            {
                _equipmentSetsEnabled = profile.EquipmentSetsEnabled;

                if (_equipmentSetsEnabled && _firstSetsAddedComplete)
                {
                    AddEquipmentSets<ArmorSetSkill>(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets, true);
                    AddEquipmentSets<WeaponSetSkill>(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets, true);
                }
                else
                {
                    ClearSkillPreviewCache();
                    RemoveEquipmentSets(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets);
                    RemoveEquipmentSets(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets);
                }
            }
        }

        private void ProfileSwitched(IActionUIProfile profile)
        {
            _equipmentSetsEnabled = profile.EquipmentSetsEnabled;

            ClearSkillPreviewCache();
            RemoveUnknownEquipmentSets<ArmorSetSkill>(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets);
            RemoveUnknownEquipmentSets<WeaponSetSkill>(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets);
            
            if (_equipmentSetsEnabled && _firstSetsAddedComplete)
            {
                AddEquipmentSets<ArmorSetSkill>(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets, true);
                AddEquipmentSets<WeaponSetSkill>(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets, true);
            }
            else
            {
                RemoveEquipmentSets(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets);
                RemoveEquipmentSets(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets);
            }
            
        }

        private void AddEquipmentSets<T>(IEnumerable<IEquipmentSet> sets, bool learnSets = false) where T : EquipmentSetSkill
        {
            Logger.LogDebug($"Adding {sets?.Count()} {typeof(T).Name} prefabs for character {_character.name}.");
            //bool needsSave = false;
            foreach (IEquipmentSet set in sets)
            {
                //int setId = set.SetID;
                var skill = AddOrGetEquipmentSetSkillPrefab<T>(set);
                if (learnSets)
                    AddOrUpdateEquipmentSetSkill(skill, set);
                //if (setId != set.SetID)
                //    needsSave = true;
                if (skill.ItemDisplay != null)
                    AddEquipmentSetIcon(skill.ItemDisplay, skill);
            }
            //return needsSave;
        }

        public T AddOrGetEquipmentSetSkillPrefab<T>(IEquipmentSet equipmentSet) where T : EquipmentSetSkill
        {
            if (equipmentSet.SetID == 0)
                throw new ArgumentException($"{nameof(IEquipmentSet.SetID)} must not be zero.", nameof(equipmentSet));

            if (ResourcesPrefabManager.Instance.ContainsItemPrefab(equipmentSet.SetID.ToString()))
                return (T)ResourcesPrefabManager.Instance.GetItemPrefab(equipmentSet.SetID.ToString());

            var skillGo = new GameObject(equipmentSet.Name.Replace(" ", string.Empty));

            skillGo.SetActive(false);
            T skill = skillGo.AddComponent<T>();
            skill.SetEquipmentSet(equipmentSet, _character);
            //skill.Character = _character;
            skill.IsPrefab = true;
            //skillGo.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(skillGo);
            var itemPrefabs = GetItemPrefabs();
            itemPrefabs.Add(skill.ItemIDString, skill);
            //skillGo.SetActive(true);

            Logger.LogDebug($"AddOrGetEquipmentSetSkillPrefab: Created skill prefab {skill.name}.");
            return skill;
        }

        public void AddOrUpdateEquipmentSetSkill(EquipmentSetSkill skillPrefab, IEquipmentSet equipmentSet)
        {
            var existingSkill = _characterInventory.SkillKnowledge.transform.GetComponentsInChildren<Skill>().FirstOrDefault(s => s.ItemID == skillPrefab.ItemID);

            Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: {_characterInventory.SkillKnowledge.GetLearnedItems().Count} skills are known to character {_character.name}. existingSkill is {existingSkill?.name}");
            if (!_characterInventory.SkillKnowledge.IsItemLearned(skillPrefab) && existingSkill == null)
            {
                //var skill = (EquipmentSetSkill)ResourcesPrefabManager.Instance.GenerateItem(skillPrefab.ItemIDString);
                var skill = UnityEngine.Object.Instantiate(skillPrefab, _characterInventory.SkillKnowledge.transform);
                skill.IsPrefab = false;
                if (!skill.gameObject.activeSelf)
                    skill.gameObject.SetActive(true);

                Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: Added skill {skillPrefab.name} to character {_character.name}.");
                ////skill.SetParent(_characterInventory.SkillKnowledge.transform);
                ////Logger.LogDebug($"LearnEquipmentSetSkill: Skill {skill.name} parent is {skill.transform.parent?.name}.");
                ////skill.transform.parent = _characterInventory.SkillKnowledge.transform;
                ////Logger.LogDebug($"LearnEquipmentSetSkill: Skill {skill.name} parent set to {skill.transform.parent.name}.");
                //_coroutines.DoNextFrame(() =>
                //{
                //    //_characterInventory.SkillKnowledge.AddItem(skill);
                //    Logger.LogDebug($"LearnEquipmentSetSkill: Next Frame - Skill {skill.name} parent is {skill.transform.parent.name}.");
                //    //skill.ForceUpdateParentChange();
                //    Logger.LogDebug($"LearnEquipmentSetSkill: Next Frame - Skill {skill.name} parent set to {skill.transform.parent.name}.");
                //    Logger.LogDebug($"LearnEquipmentSetSkill: Character {_character.name} learned new Skill {skill.name}.");
                //});
            }
            else
            {
                var learnedSkill = (EquipmentSetSkill)_characterInventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(s => s.ItemID == equipmentSet.SetID) ?? (EquipmentSetSkill)existingSkill;
                learnedSkill.SetEquipmentSet(equipmentSet, _character);
                Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: Character {_character.name} had existing EquipmentSetSkill {learnedSkill.name} updated with equipmentSet {equipmentSet.SetID} - {equipmentSet.Name}.");
            }
        }

        public void RemoveEquipmentSets(IEnumerable<IEquipmentSet> sets)
        {
            var removeSets = sets.ToList();
            for (int i = 0; i < removeSets.Count; i++)
                RemoveEquipmentSet(removeSets[i].SetID);
        }

        public void RemoveUnknownEquipmentSets<T>(IEnumerable<IEquipmentSet> knownSets) where T : EquipmentSetSkill
        {
            Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveUnknownEquipmentSets)}: Removing any unknown {typeof(T).Name} sets.");
            var setSkills = _characterInventory.SkillKnowledge.GetLearnedItems().Where(s => s is T).Cast<T>().ToArray();

            for (int i = 0; i < setSkills.Length; i++)
            {
                if (!knownSets.Any(s => s.SetID == setSkills[i].ItemID))
                    RemoveEquipmentSet(setSkills[i].ItemID);
            }
        }

        public void RemoveEquipmentSet(int setID)
        {
            if (!_characterInventory.SkillKnowledge.IsItemLearned(setID))
                return;

            var setSkill = _characterInventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(l => l.ItemID == setID);

            if (setSkill != null)
            {
                setSkill.transform.parent = null;
                setSkill.gameObject.Destroy();
                Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveEquipmentSet)}: Destroyed set skill with SetID == {setID}.");
            }

            _characterInventory.SkillKnowledge.RemoveItem(setID);

            if (ResourcesPrefabManager.Instance.ContainsItemPrefab(setID.ToString()))
            {
                var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(setID);
                GetItemPrefabs().Remove(setID.ToString());
                prefab.gameObject.Destroy();

                Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveEquipmentSet)}: Destroyed set prefab with SetID == {setID}.");
            }

            
        }

        private Dictionary<string, Item> GetItemPrefabs()
        {
            var field = typeof(ResourcesPrefabManager).GetField("ITEM_PREFABS", BindingFlags.NonPublic | BindingFlags.Static);
            return (Dictionary<string, Item>)field.GetValue(null);
        }

        private void AddEquipmentSetIcon(ItemDisplay itemDisplay, Item item)
        {
            //if (itemDisplay.GetComponentsInChildren<Image>().Any(i => i.name == "imgEquipmentSet"))
            //    return;

            var existingIcons = itemDisplay.GetComponentsInChildren<Image>().Where(i => i.name == "imgEquipmentSet").ToArray();

            if (!(item is EquipmentSetSkill setSkill))
            {
                for (int i = 0; i < existingIcons.Length; i++)
                    UnityEngine.Object.Destroy(existingIcons[i].gameObject);
                return;
            }
            else if (existingIcons.Any())
                return;

            var enchantedGo = itemDisplay.transform.Find("imgEnchanted").gameObject;
            var equipmentSetIconGo = UnityEngine.Object.Instantiate(enchantedGo, itemDisplay.transform);
            var existingImage = equipmentSetIconGo.GetComponent<Image>();
            UnityEngine.Object.DestroyImmediate(existingImage);

            equipmentSetIconGo.name = "imgEquipmentSet";
            var newImage = equipmentSetIconGo.AddComponent<Image>();
            //newImage.name = "EquipmentSetIcon";
            newImage.sprite = ActionMenuResources.Instance.SpriteResources["EquipmentSetIcon"];
            equipmentSetIconGo.SetActive(true);

            Logger.LogDebug($"Added EquipmentSetIcon {equipmentSetIconGo.name} to ItemDisplay {itemDisplay.gameObject.name}.");
        }

        public bool HasItems(IEnumerable<EquipSlot> slots) => slots.All(e => HasItem(e));

        public bool HasItem(EquipSlot equipSlot) => equipSlot == null
                        || _characterInventory.OwnsItem(equipSlot.UID)
                        || (_equipSetsProfile.StashEquipEnabled && GetIsAreaStashEnabled() && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any())
                        || (_equipSetsProfile.StashEquipEnabled && _equipSetsProfile.StashEquipAnywhereEnabled && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any());

        public ArmorSet GetEquippedAsArmorSet(string name)
        {
            var armorSet = new ArmorSet() { Name = name };

            var slots = _characterEquipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).ToList();

            foreach (var slot in slots)
            {
                var equipSlot = ToEquipmentSlot(slot);
                if (equipSlot.Slot == EquipSlots.Head)
                    armorSet.Head = equipSlot;
                else if (equipSlot.Slot == EquipSlots.Chest)
                    armorSet.Chest = equipSlot;
                else if (equipSlot.Slot == EquipSlots.Feet)
                    armorSet.Feet = equipSlot;
            }
            return armorSet;
        }

        public bool TryEquipWeaponSet(WeaponSet weaponSet)
        {
            //prevent additional sets from being queued
            if (_castQueueProcessing || _stashQueueProcessing)
                return false;

            var equippedSlots = _characterEquipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).ToList();
            var equipFromStash = GetIsStashEquipEnabled();
            var unequipToStash = GetIsStashUnequipEnabled();
            bool isRightEquipped;
            bool isLeftEquipped;
            if (weaponSet == null)
            {
                isRightEquipped = TryEquipSlot(null, EquipSlots.RightHand, equipFromStash, unequipToStash);
                isLeftEquipped = TryEquipSlot(null, EquipSlots.LeftHand, equipFromStash, unequipToStash);
            }
            else
            {
                isRightEquipped = TryEquipSlot(weaponSet.RightHand, EquipSlots.RightHand, equipFromStash, unequipToStash);
                isLeftEquipped = TryEquipSlot(weaponSet.LeftHand, EquipSlots.LeftHand, equipFromStash, unequipToStash);
            }

            bool isEquipped = isRightEquipped && isLeftEquipped;
            if (!isEquipped)
                _character.CharacterUI.ShowInvalidActionNotification(weaponSet.GetNotificationObject());

            ProcessEquipQueues();
            return isEquipped;

        }

        public bool TryEquipArmorSet(ArmorSet armorSet)
        {
            //prevent additional sets from being queued
            if (_castQueueProcessing || _stashQueueProcessing)
                return false;

            if (_character.InCombat && !_equipSetsProfile.ArmorSetsInCombatEnabled)
            {
                _character.CharacterUI.ShowInvalidActionNotification(armorSet.GetNotificationObject());
                return false;
            }

            var equipFromStash = GetIsStashEquipEnabled();
            var unequipToStash = GetIsStashUnequipEnabled();

            var equippedSlots = _characterEquipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).ToList();
            if (armorSet == null)
            {
                foreach (var slot in equippedSlots)
                {
                    TryEquipSlot(null, OutwardSlotIDsXRef[slot.SlotType], equipFromStash, unequipToStash);
                }
                ProcessEquipQueues();
                return true;
            }

            bool isHeadEquipped;
            bool isChestEquipped;
            bool isFeetEquipped;

            isHeadEquipped = TryEquipSlot(armorSet.Head, EquipSlots.Head, equipFromStash, unequipToStash);
            isChestEquipped = TryEquipSlot(armorSet.Chest, EquipSlots.Chest, equipFromStash, unequipToStash);
            isFeetEquipped = TryEquipSlot(armorSet.Feet, EquipSlots.Feet, equipFromStash, unequipToStash);

            bool isEquipped = isHeadEquipped && isChestEquipped && isFeetEquipped;

            if (!isEquipped)
                _character.CharacterUI.ShowInvalidActionNotification(armorSet.GetNotificationObject());
            ProcessEquipQueues();
            return isEquipped;
        }

        public bool IsArmorSetEquipped(ArmorSet armorSet)
            => IsEquipSlotEquipped(armorSet.Head, EquipmentSlot.EquipmentSlotIDs.Helmet) &&
                IsEquipSlotEquipped(armorSet.Chest, EquipmentSlot.EquipmentSlotIDs.Chest) &&
                IsEquipSlotEquipped(armorSet.Feet, EquipmentSlot.EquipmentSlotIDs.Foot);

        public bool IsWeaponSetEquipped(WeaponSet weaponSet)
            => IsEquipSlotEquipped(weaponSet.RightHand, EquipmentSlot.EquipmentSlotIDs.RightHand) &&
                IsEquipSlotEquipped(weaponSet.LeftHand, EquipmentSlot.EquipmentSlotIDs.LeftHand);


        private bool IsEquipSlotEquipped(EquipSlot equipSlot, EquipmentSlot.EquipmentSlotIDs slotID)
        {
            if (equipSlot == null)
            {
                if (_characterEquipment.HasItemEquipped(slotID))
                    return false;
            }
            else if (!_characterEquipment.HasItemEquipped(slotID))
                return false;
            else if (_characterEquipment.GetMatchingSlot(slotID).EquippedItemUID != equipSlot.UID)
                return false;

            return true;
        }

        private bool TryEquipSlot(EquipSlot equipSlot, EquipSlots slotType, bool equipFromStash, bool unequipToStash)
        {
            var slotID = ActionUiEquipSlotsXRef[slotType];
            var slot = _characterEquipment.GetMatchingSlot(slotID);
            var disableStashUnequip = !string.IsNullOrEmpty(slot.EquippedItemUID)
                && !_profileManager.ArmorSetService.IsContainedInSet(slot.EquippedItemUID)
                && !_profileManager.WeaponSetService.IsContainedInSet(slot.EquippedItemUID);

            var performAnimation = !_equipSetsProfile.SkipWeaponAnimationsEnabled && (slotType == EquipSlots.LeftHand || slotType == EquipSlots.RightHand);

            //Item is already equipped
            if (equipSlot != null && slot.EquippedItemUID == equipSlot.UID)
                return true;

            //If equipSlot is empty, unequip any equipped item from the slot
            if (equipSlot == null)
            {
                //Nothing equipped, nothing to unequip
                if (!slot.HasItemEquipped)
                    return true;

                Action unequipAction;
                if (unequipToStash && !disableStashUnequip)
                {
                    unequipAction = () =>
                    {
                        Logger.LogDebug($"Unequipping item UID {equipSlot.UID} from slot {slotID} to stash.");
                        _characterInventory.UnequipItem(slot.EquippedItem, performAnimation, _character.Stash);
                    };
                }
                else
                {
                    unequipAction = () =>
                    {
                        Logger.LogDebug($"Unequipping item UID {equipSlot.UID} from slot {slotID}.");
                        _characterInventory.UnequipItem(slot.EquippedItem, performAnimation);
                    };
                }

                if (performAnimation)
                    QueueAfterCastingAction(unequipAction);
                else
                    unequipAction.Invoke();

                return true;
            }


            var stashEquipEnabled = equipFromStash && !_characterInventory.OwnsItem(equipSlot.UID) && slot.HasItemEquipped;
            var stashUnequipEnabled = !disableStashUnequip && unequipToStash && slot.HasItemEquipped;

            //If item being equipped is coming from the stash or equipment is being unequipped to the stash, do a stash equip
            if (stashEquipEnabled || stashUnequipEnabled)
            {
                var charEquipSlot = _characterEquipment.GetMatchingSlot(slotID);

                var equipFoundInStash = (_equipSetsProfile.StashEquipEnabled && GetIsAreaStashEnabled() && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any())
                        || (_equipSetsProfile.StashEquipEnabled && _equipSetsProfile.StashEquipAnywhereEnabled && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any());
                var equipFoundInInventory = _characterInventory.OwnsItem(equipSlot.UID);

                if (!equipFoundInStash && !equipFoundInInventory)
                    return false;


                var moveItem = slot.EquippedItem;

                if (performAnimation)
                    QueueAfterCastingAction(() => _characterInventory.EquipItem(equipSlot.UID, performAnimation));
                else
                    _characterInventory.EquipItem(equipSlot.UID, performAnimation);

                if (stashUnequipEnabled)
                    _queuedMovesToStash.Enqueue(moveItem);
            }
            //No stash involved. Perform standard equip.
            else
            {
                Action equipAction = () =>
                {
                    Logger.LogDebug($"Equipping item UID {equipSlot.UID} to slot {slotID}.");
                    _characterInventory.EquipItem(equipSlot.UID, performAnimation);
                };

                if (performAnimation)
                    QueueAfterCastingAction(equipAction);
                else
                    equipAction.Invoke();

            }

            return true;
        }

        Queue<Action> _queuedCastActions = new Queue<Action>();
        private void QueueAfterCastingAction(Action action)
        {
            Logger.LogDebug($"Queued cast action. {_queuedCastActions.Count} actions queued.");
            _queuedCastActions.Enqueue(action);
        }

        bool _castQueueProcessing = false;
        private IEnumerator InvokeQueuedCastActions()
        {
            _castQueueProcessing = true;
            Logger.LogDebug($"Started processing Cast Actions queue. Queue contains {_queuedCastActions.Count} actions.");
            while (_queuedCastActions.Count > 0)
            {
                if (!_character.IsCasting)
                {
                    _queuedCastActions.Dequeue().Invoke();
                }
                yield return null;
            }
            var timeoutAt = DateTime.Now.AddSeconds(5);
            Logger.LogDebug($"All Cast Actions in queue invoked. Awaiting last character casting to complete.");
            while (_character.IsCasting && timeoutAt > DateTime.Now)
                yield return null;

            Logger.LogDebug($"Finished processing Cast Actions queue.");
            _castQueueProcessing = false;
        }

        private void ProcessEquipQueues()
        {
            if (!_castQueueProcessing)
                _coroutines.StartRoutine(InvokeQueuedCastActions());
            if (!_stashQueueProcessing)
                _coroutines.StartRoutine(MoveQueuedItemsToStash());
        }

        bool _stashQueueProcessing = false;
        Queue<Equipment> _queuedMovesToStash = new Queue<Equipment>();
        private IEnumerator MoveQueuedItemsToStash()
        {
            _stashQueueProcessing = true;
            Logger.LogDebug($"Started processing Stash Items queue. Queue contains {_queuedMovesToStash.Count} actions.");
            yield return null;
            while (_castQueueProcessing)
                yield return null;

            while (_queuedMovesToStash.Count > 0)
            {
                var equipment = _queuedMovesToStash.Dequeue();
                //wait for parent container to be set / unequip to be finished
                var timeoutAt = DateTime.Now.AddSeconds(5);
                while (equipment.ParentContainer == null && timeoutAt > DateTime.Now)
                    yield return null;

                Logger.LogDebug($"Moving equipment {equipment.name} to stash. ParentContainer {equipment.ParentContainer}.");
                _character.Stash.TryMoveItemToContainer(equipment);
                yield return null;
            }

            yield return null;
            Logger.LogDebug($"Finished processing Stash Items queue.");
            _stashQueueProcessing = false;
        }

        public WeaponSet GetEquippedAsWeaponSet(string name)
        {
            var weaponSet = new WeaponSet() { Name = name };

            var slots = _character.Inventory.Equipment.EquipmentSlots.Where(s => s != null && s.HasItemEquipped).ToList();

            foreach (var slot in slots)
            {
                var equipSlot = ToEquipmentSlot(slot);
                if (equipSlot.Slot == EquipSlots.RightHand)
                    weaponSet.RightHand = equipSlot;
                else if (equipSlot.Slot == EquipSlots.LeftHand)
                    weaponSet.LeftHand = equipSlot;
            }
            return weaponSet;
        }

        private EquipSlot ToEquipmentSlot(EquipmentSlot slot)
        {
            if (!slot.HasItemEquipped || slot.EquippedItem == null)
                return null;

            if (!OutwardSlotIDsXRef.TryGetValue(slot.SlotType, out var slotType))
                slotType = EquipSlots.None;

            return new EquipSlot()
            {
                ItemID = slot.EquippedItemID,
                UID = slot.EquippedItemUID,
                Slot = slotType
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CharacterInventoryPatches.AfterInventoryIngredients -= AddStashIngredients;
                    EquipmentMenuPatches.AfterShow -= ShowEquipmentSetMenu;
                    EquipmentMenuPatches.AfterOnHide -= HideEquipmentSetMenu;
                    ItemDisplayPatches.AfterSetReferencedItem -= AddEquipmentSetIcon;
                    NetworkLevelLoaderPatches.MidLoadLevelAfter -= ShowHideSkillsMenu;
                    ClearSkillPreviewCache();
                }
                _character = null;
                _equipmentSetsMenus = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~InventoryService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
