using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Patches;
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
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public class EquipService : IDisposable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private CharacterInventory _characterInventory => _character.Inventory;
        private CharacterEquipment _characterEquipment => _character.Inventory.Equipment;
        private readonly ProfileManager _profileManager;
        private readonly InventoryService _inventoryService;
        private readonly EquipSetPrefabService _equipSetPrefabService;
        private IActionUIProfile _profile => _profileManager.ProfileService.GetActiveProfile();
        private EquipmentSetsSettingsProfile _equipSetsProfile => _profileManager.ProfileService.GetActiveProfile().EquipmentSetsSettingsProfile;
        private EquipmentSetMenu _equipmentSetsMenus;
        private readonly LevelCoroutines _coroutines;


        //private bool _firstSetsAddedComplete;
        private bool _equipmentSetsEnabled;

        private bool disposedValue;

        private Dictionary<EquipSlots, EquipSkillPreview> _cachedSkillPreviews = new Dictionary<EquipSlots, EquipSkillPreview>();

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

        public EquipService(Character character, ProfileManager profileManager, EquipmentSetMenu equipmentSetsMenus, InventoryService inventoryService, EquipSetPrefabService equipSetPrefabService, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _equipmentSetsMenus = equipmentSetsMenus;
            _inventoryService = inventoryService;
            _equipSetPrefabService = equipSetPrefabService;
            _coroutines = coroutines;
            _getLogger = getLogger;
        }

        public void Start()
        {
            try
            {
                _equipmentSetsEnabled = _profile.EquipmentSetsEnabled;
                EquipmentMenuPatches.AfterShow += ShowEquipmentSetMenu;
                EquipmentMenuPatches.AfterOnHide += HideEquipmentSetMenu;
                ItemDisplayPatches.AfterSetReferencedItem += _equipSetPrefabService.AddEquipmentSetIcon;
                NetworkLevelLoader.Instance.onOverallLoadingDone += ShowHideSkillsMenu;
                _profileManager.ProfileService.OnActiveProfileChanged += TryProfileChanged;
                _profileManager.ProfileService.OnActiveProfileSwitched += TryProfileSwitched;

                var armorService = (ArmorSetsJsonService)_profileManager.ArmorSetService;
                var weaponService = (WeaponSetsJsonService)_profileManager.WeaponSetService;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(EquipService)}.", ex);
            }
        }

        private void ShowHideSkillsMenu()
        {
            if (_character?.CharacterUI == null)
                return;

            //Creates ItemDisplays for equipment set skills.

            //_character.CharacterUI.ShowMenu(CharacterUI.MenuScreens.Skills);
            //_character.CharacterUI.HideMenu(CharacterUI.MenuScreens.Skills);

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
                var xPos = topLeft.x; // + 20;
                var yPos = topLeft.y; // + 13;
                equipRectTransform.position = new Vector3(xPos, yPos);
                equipRectTransform.anchoredPosition = new Vector3(equipRectTransform.anchoredPosition.x + 25, equipRectTransform.anchoredPosition.y);
                equipRectTransform.GetComponent<Image>().material = menuBg.material;
            });
        }

        private void TryProfileChanged(IActionUIProfile profile)
        {
            try
            {
                ProfileChanged(profile);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to adjust equipment sets after profile change.", ex);
            }
        }

        private void ProfileChanged(IActionUIProfile profile)
        {
            if (_equipmentSetsEnabled != profile.EquipmentSetsEnabled)
            {
                _equipmentSetsEnabled = profile.EquipmentSetsEnabled;

                if (_equipmentSetsEnabled)
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

        private void TryProfileSwitched(IActionUIProfile profile)
        {
            try
            {
                ProfileSwitched(profile);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to adjust equipment sets after profile switch.", ex);
            }
        }

        private void ProfileSwitched(IActionUIProfile profile)
        {
            _equipmentSetsEnabled = profile.EquipmentSetsEnabled;

            ClearSkillPreviewCache();
            RemoveUnknownEquipmentSets<ArmorSetSkill>(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets);
            RemoveUnknownEquipmentSets<WeaponSetSkill>(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets);

            if (_equipmentSetsEnabled)
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
            foreach (IEquipmentSet set in sets)
            {
                var skill = _equipSetPrefabService.AddOrGetEquipmentSetSkillPrefab<T>(set);
                if (learnSets)
                    AddOrUpdateEquipmentSetSkill(skill, set);
                if (skill.ItemDisplay != null)
                    AddEquipmentSetIcon(skill.ItemDisplay, skill);
            }
        }

        public void AddOrUpdateEquipmentSetSkill<T>(IEquipmentSet equipmentSet) where T : EquipmentSetSkill
        {
            var skill = _equipSetPrefabService.AddOrGetEquipmentSetSkillPrefab<T>(equipmentSet);
            AddOrUpdateEquipmentSetSkill(skill, equipmentSet);
        }

        public void AddOrUpdateEquipmentSetSkill(EquipmentSetSkill skillPrefab, IEquipmentSet equipmentSet)
        {
            var existingSkill = _characterInventory.SkillKnowledge.transform.GetComponentsInChildren<Skill>().FirstOrDefault(s => s.ItemID == skillPrefab.ItemID);

            Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: {_characterInventory.SkillKnowledge.GetLearnedItems().Count} skills are known to character {_character.name}. existingSkill is {existingSkill?.name}");
            if (!_characterInventory.SkillKnowledge.IsItemLearned(skillPrefab) && existingSkill == null)
            {
                //var skill = (EquipmentSetSkill)ResourcesPrefabManager.Instance.GenerateItem(skillPrefab.ItemIDString);
                //var skill = UnityEngine.Object.Instantiate(skillPrefab, _characterInventory.SkillKnowledge.transform);
                //skill.IsPrefab = false;
                //if (!skill.gameObject.activeSelf)
                //    skill.gameObject.SetActive(true);
                _characterInventory.TryUnlockSkill(skillPrefab);
                Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: Added skill {skillPrefab.name} to character {_character.name}.");
            }
            else
            {
                var learnedSkill = (EquipmentSetSkill)_characterInventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(s => s.ItemID == equipmentSet.SetID) ?? (EquipmentSetSkill)existingSkill;
                learnedSkill.SetEquipmentSet(equipmentSet);
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

            _equipSetPrefabService.RemoveEquipmentSetPrefab(setID);
        }

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

        private void AddEquipmentSetIcon(ItemDisplay itemDisplay, Item item)
        {

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
            newImage.sprite = ActionMenuResources.Instance.SpriteResources["EquipmentSetIcon"];
            equipmentSetIconGo.SetActive(true);

            Logger.LogDebug($"Added EquipmentSetIcon {equipmentSetIconGo.name} to ItemDisplay {itemDisplay.gameObject.name}.");
        }

        public bool GetIsStashEquipEnabled() => _equipSetsProfile.StashEquipEnabled && (_inventoryService.GetAreaContainsStash() || _equipSetsProfile.StashEquipAnywhereEnabled);
        public bool GetIsStashUnequipEnabled() => _equipSetsProfile.StashUnequipEnabled && (_inventoryService.GetAreaContainsStash() || _equipSetsProfile.StashUnequipAnywhereEnabled);

        public bool HasItems(IEnumerable<EquipSlot> slots) => slots.All(e => HasItem(e));

        public bool HasItem(EquipSlot equipSlot) => equipSlot == null
                        || _characterInventory.OwnsItem(equipSlot.UID)
                        || (GetIsStashEquipEnabled() && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any());

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
            bool isRightEquipped = false;
            bool isLeftEquipped = false;
            //Empty weapon set. Unequip both weapons.
            if (weaponSet == null || weaponSet.RightHand == null && weaponSet.LeftHand == null)
            {
                isRightEquipped = TryEquipSlot(null, EquipSlots.RightHand, equipFromStash, unequipToStash);
                isLeftEquipped = TryEquipSlot(null, EquipSlots.LeftHand, equipFromStash, unequipToStash);
            }
            //Set contains an item for each hand.
            else if (weaponSet.RightHand?.UID != weaponSet.LeftHand?.UID)
            {
                isRightEquipped = TryEquipSlot(weaponSet.RightHand, EquipSlots.RightHand, equipFromStash, unequipToStash);
                isLeftEquipped = TryEquipSlot(weaponSet.LeftHand, EquipSlots.LeftHand, equipFromStash, unequipToStash);
            }
            //Set is a 2h weapon.
            else
            {
                var equipment = (Equipment)ResourcesPrefabManager.Instance.GetItemPrefab(weaponSet.RightHand.ItemID);
                if (equipment.TwoHandedLeft)
                {
                    isLeftEquipped = TryEquipSlot(weaponSet.LeftHand, EquipSlots.RightHand, equipFromStash, unequipToStash);
                    isRightEquipped = true;
                }
                else
                {
                    isLeftEquipped = true;
                    isRightEquipped = TryEquipSlot(weaponSet.RightHand, EquipSlots.RightHand, equipFromStash, unequipToStash);
                }
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
                        Logger.LogDebug($"Unequipping item UID {equipSlot?.UID} from slot {slotID} to stash.");
                        _characterInventory.UnequipItem(slot.EquippedItem, performAnimation, _character.Stash);
                    };
                }
                else
                {
                    unequipAction = () =>
                    {
                        Logger.LogDebug($"Unequipping item UID {equipSlot?.UID} from slot {slotID}.");
                        _characterInventory.UnequipItem(slot.EquippedItem, performAnimation);
                    };
                }

                if (performAnimation)
                    QueueAfterCastingAction(unequipAction);
                else
                    unequipAction.Invoke();

                return true;
            }

            var isEquipInCharInventory = _characterInventory.OwnsItem(equipSlot.UID);
            var shouldEquipFromStash = equipFromStash && !isEquipInCharInventory && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any();
            var shouldUnequipToStash = !disableStashUnequip && unequipToStash;

            //If item being equipped is coming from the stash or equipment is being unequipped to the stash, do a stash equip
            if (shouldEquipFromStash || shouldUnequipToStash)
            {
                var charEquipSlot = _characterEquipment.GetMatchingSlot(slotID);

                if (!shouldEquipFromStash && !isEquipInCharInventory)
                    return false;

                var moveItem = slot.EquippedItem;

                if (performAnimation)
                    QueueAfterCastingAction(() => _characterInventory.EquipItem(equipSlot.UID, performAnimation));
                else
                    _characterInventory.EquipItem(equipSlot.UID, performAnimation);

                if (shouldUnequipToStash && moveItem != null)
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
                    EquipmentMenuPatches.AfterShow -= ShowEquipmentSetMenu;
                    EquipmentMenuPatches.AfterOnHide -= HideEquipmentSetMenu;
                    ItemDisplayPatches.AfterSetReferencedItem -= _equipSetPrefabService.AddEquipmentSetIcon;
                    NetworkLevelLoader.Instance.onOverallLoadingDone -= ShowHideSkillsMenu;
                    if (_profileManager?.ProfileService != null)
                    {
                        _profileManager.ProfileService.OnActiveProfileChanged -= TryProfileChanged;

                        _profileManager.ProfileService.OnActiveProfileSwitched -= TryProfileSwitched;
                    }
                    ClearSkillPreviewCache();
                }
                _character = null;
                _equipmentSetsMenus = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
