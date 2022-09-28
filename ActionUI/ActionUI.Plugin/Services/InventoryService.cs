using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
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
        private readonly ModifCoroutine _coroutines;

        public bool GetIsStashEquipEnabled() => _equipSetsProfile.StashEquipEnabled && (GetIsAreaStashEnabled() || _equipSetsProfile.StashEquipAnywhereEnabled);
        public bool GetIsStashUnequipEnabled() => _equipSetsProfile.StashUnequipEnabled && (GetIsAreaStashEnabled() || _equipSetsProfile.StashUnequipAnywhereEnabled);

        private int _lastSetItemID = -1310000000;

        private int GetNextItemID() => --_lastSetItemID;

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

        public InventoryService(Character character, ProfileManager profileManager, EquipmentSetMenu equipmentSetsMenus, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _equipmentSetsMenus = equipmentSetsMenus;
            _coroutines = coroutines;
            _getLogger = getLogger;
        }

        public void Start()
        {
            CharacterInventoryPatches.AfterInventoryIngredients += AddStashIngredients;
            EquipmentMenuPatches.AfterShow += ShowEquipmentSetMenu;
            EquipmentMenuPatches.AfterOnHide += HideEquipmentSetMenu;
            ItemDisplayPatches.AfterSetReferencedItem += AddEquipmentSetIcon;

            var armorService = (ArmorSetsJsonService)_profileManager.ArmorSetService;
            var weaponService = (WeaponSetsJsonService)_profileManager.WeaponSetService;

            int weaponSetID = weaponService.GetLastSetID();
            int armorSetID = armorService.GetLastSetID();
            _lastSetItemID = armorSetID < weaponSetID ? armorSetID : weaponSetID;
            if (_lastSetItemID == 0)
                _lastSetItemID = -1310000000;


            if (LearnEquipmentSets<ArmorSetSkill>(_profileManager.ArmorSetService.GetEquipmentSetsProfile().EquipmentSets))
                armorService.Save();

            if (LearnEquipmentSets<WeaponSetSkill>(_profileManager.WeaponSetService.GetEquipmentSetsProfile().EquipmentSets))
                weaponService.Save();

            //Creates ItemDisplays for equipment sets.
            _coroutines.DoNextFrame(() =>
            {
                _character.CharacterUI.ShowMenu(CharacterUI.MenuScreens.Skills);
                _character.CharacterUI.HideMenu(CharacterUI.MenuScreens.Skills);
            });
        }

        public EquipSkillPreview GetEquipSkillPreview(IEquipmentSet set)
        {
            return new EquipSkillPreview(_characterEquipment.GetMatchingSlot(ActionUiEquipSlotsXRef[set.SlotIcon]));
        }

        private void HideEquipmentSetMenu(EquipmentMenu obj)
        {
            if (!_profileManager.ProfileService.GetActiveProfile().EquipmentSetsEnabled && !_equipmentSetsMenus.gameObject.activeSelf)
                return;

            _equipmentSetsMenus.Hide();
        }

        private void ShowEquipmentSetMenu(EquipmentMenu obj)
        {
            if (!_profileManager.ProfileService.GetActiveProfile().EquipmentSetsEnabled)
                return;

            _equipmentSetsMenus.Show();
            //_coroutines.DoNextFrame(() =>
            //{
                var equipXform = _equipmentSetsMenus.transform as RectTransform;
                var charMenus = _character.CharacterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel") as RectTransform;
                var menuBg = charMenus.Find("Background").GetComponent<Image>();
                var xpos = charMenus.anchoredPosition.x - equipXform.rect.width + 30;
                var ypos = charMenus.anchoredPosition.y + 10;
                equipXform.anchoredPosition = new Vector2(xpos, ypos);
                equipXform.GetComponent<Image>().material = menuBg.material;
            //});
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

        //private HashSet<string> _learnedSkills = new HashSet<string>();
        private bool LearnEquipmentSets<T>(IEnumerable<IEquipmentSet> sets) where T : EquipmentSetSkill
        {
            bool needsSave = false;
            foreach (IEquipmentSet set in sets)
            {
                int setId = set.SetID;
                var skill = LearnEquipmentSetSkill<T>(set);
                if (setId != set.SetID)
                    needsSave = true;
                if (skill.ItemDisplay != null)
                    AddEquipmentSetIcon(skill.ItemDisplay, skill);
            }
            return needsSave;
        }

        public T LearnEquipmentSetSkill<T>(IEquipmentSet equipmentSet) where T : EquipmentSetSkill
        {
            //if (_learnedSkills.Contains(equipmentSet.Name))
            //    return;

            Logger.LogDebug($"LearnEquipmentSetSkill: SetID=={equipmentSet.SetID}, IsItemLearned({equipmentSet.SetID})=={_characterInventory.SkillKnowledge.IsItemLearned(equipmentSet.SetID)}");
            if (equipmentSet.SetID == 0 || !_characterInventory.SkillKnowledge.IsItemLearned(equipmentSet.SetID))
            {
                if (equipmentSet.SetID == 0)
                    equipmentSet.SetID = GetNextItemID();
                var skillGo = new GameObject(equipmentSet.Name.Replace(" ", string.Empty));

                skillGo.SetActive(false);
                T skill = skillGo.AddComponent<T>();
                skill.SetEquipmentSet(equipmentSet);
                skill.Character = _character;
                skill.ItemID = equipmentSet.SetID;
                skillGo.SetActive(true);
                _coroutines.DoNextFrame(() => { 
                    _characterInventory.SkillKnowledge.AddItem(skill);
                    Logger.LogDebug($"LearnEquipmentSetSkill: Added new Skill {skill.name}.");
                });

                return skill;
            }
            else
            {
                var skill = (T)_characterInventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(s => s.ItemID == equipmentSet.SetID);
                skill.SetEquipmentSet(equipmentSet);
                Logger.LogDebug($"LearnEquipmentSetSkill: Updated existing EquipmentSetSkill {skill.name} with equipmentSet {equipmentSet.SetID} - {equipmentSet.Name}.");
                return skill;
            }

        }

        private void AddEquipmentSetIcon(ItemDisplay itemDisplay, EquipmentSetSkill setSkill)
        {
            if (itemDisplay.GetComponentsInChildren<Image>().Any(i => i.name == "imgEquipmentSet"))
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
                foreach(var slot in equippedSlots)
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
                    Logger.LogDebug($"Unequipping item UID {equipSlot.UID} from slot {slotID} to stash.");
                    unequipAction = () => _characterInventory.UnequipItem(slot.EquippedItem, performAnimation, _character.Stash);
                }
                else
                {
                    Logger.LogDebug($"Unequipping item UID {equipSlot.UID} from slot {slotID}.");
                    unequipAction = () => _characterInventory.UnequipItem(slot.EquippedItem, performAnimation);
                }

                if (performAnimation)
                    PerformActionAfterCasting(unequipAction);
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
                    PerformActionAfterCasting(() => _characterInventory.EquipItem(equipSlot.UID, performAnimation));
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
                    PerformActionAfterCasting(equipAction);
                else
                    equipAction.Invoke();

            }

            return true;
        }

        //private bool TryEquipSlot1(EquipSlot equipSlot, EquipSlots slotType, bool equipFromStash, bool unequipToStash)
        //{
        //    var slotID = ActionUiEquipSlotsXRef[slotType];
        //    var slot = _characterEquipment.GetMatchingSlot(slotID);
        //    var disableStashUnequip = !string.IsNullOrEmpty(slot.EquippedItemUID)
        //        && !_profileManager.ArmorSetService.IsContainedInSet(slot.EquippedItemUID)
        //        && !_profileManager.WeaponSetService.IsContainedInSet(slot.EquippedItemUID);

        //    var performAnimation = !_equipSetsProfile.SkipWeaponAnimationsEnabled && (slotType == EquipSlots.LeftHand || slotType == EquipSlots.RightHand);

        //    //Item is already equipped
        //    if (equipSlot != null && _characterEquipment.GetMatchingSlot(slotID).EquippedItemUID == equipSlot.UID)
        //        return true;

        //    //If equipSlot is empty, unequip any equipped item from the slot
        //    if (equipSlot == null)
        //    {
        //        //Nothing equipped, nothing to unequip
        //        if (_characterEquipment.IsEquipmentSlotEmpty(slotID))
        //            return true;

        //        Action unequipAction;
        //        if (unequipToStash && !disableStashUnequip)
        //            unequipAction = () => _characterInventory.UnequipItem(slot.EquippedItem, performAnimation, _character.Stash);
        //        else
        //            unequipAction = () => _characterInventory.UnequipItem(slot.EquippedItem, performAnimation);

        //        if (performAnimation)
        //            PerformActionAfterCasting(unequipAction);
        //        else
        //            unequipAction.Invoke();

        //        return true;
        //    }


        //    var stashEquipEnabled = equipFromStash && !_characterInventory.OwnsItem(equipSlot.UID) && !_characterEquipment.IsEquipmentSlotEmpty(slotID);
        //    var stashUnequipEnabled = !disableStashUnequip && unequipToStash && !_characterEquipment.IsEquipmentSlotEmpty(slotID);

        //    //If item being equipped is coming from the stash or equipment is being unequipped to the stash, do a stash equip
        //    if (stashEquipEnabled || stashUnequipEnabled)
        //    {
        //        var charEquipSlot = _characterEquipment.GetMatchingSlot(slotID);

        //        var equipFoundInStash = (_equipSetsProfile.StashEquipEnabled && GetIsAreaStashEnabled() && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any())
        //                || (_equipSetsProfile.StashEquipEnabled && _equipSetsProfile.StashEquipAnywhereEnabled && _character.Stash.GetContainedItemUIDs(equipSlot.ItemID).Any());
        //        var equipFoundInInventory = _characterInventory.OwnsItem(equipSlot.UID);

        //        if (!equipFoundInStash && !equipFoundInInventory)
        //            return false;

        //        StashEquip(slot, equipSlot, performAnimation, stashUnequipEnabled);
        //    }
        //    //No stash involved. Perform standard equip.
        //    else
        //    {
        //        Action equipAction = () =>
        //        {
        //            Logger.LogDebug($"Equipping item UID {equipSlot.UID} to slot {slotID}.");
        //            _characterInventory.EquipItem(equipSlot.UID, performAnimation);
        //        };

        //        if (performAnimation)
        //            PerformActionAfterCasting(equipAction);
        //        else
        //            equipAction.Invoke();

        //    }

        //    return true;
        //}

        //private void StashEquip(EquipmentSlot outwardSlot, EquipSlot equipSlot, bool performAnimation, bool unequipToStash)
        //{
        //    if (performAnimation)
        //    {
        //        PerformActionAfterCasting(() =>
        //        {
        //            Logger.LogDebug($"Unequipping item {outwardSlot.EquippedItem} from slot {outwardSlot.SlotType} to character's {(unequipToStash ? "stash" : "inventory")}.");
        //            if (unequipToStash)
        //                _characterInventory.UnequipItem(outwardSlot.EquippedItem, false, _character.Stash);
        //            else
        //                _characterInventory.UnequipItem(outwardSlot.EquippedItem, false);
        //        });
        //        PerformActionAfterCasting(() =>
        //        {
        //            Logger.LogDebug($"Equipping item UID {equipSlot.UID} to slot {outwardSlot.SlotType}.");
        //            _characterInventory.EquipItem(equipSlot.UID, performAnimation);
        //        });
        //    }
        //    else
        //    {
        //        _queuedUnequips.Enqueue(() =>
        //        {
        //            Logger.LogDebug($"Unequipping item {outwardSlot.EquippedItem} from slot {outwardSlot.SlotType} to character's {(unequipToStash ? "stash" : "inventory")}.");
        //            if (unequipToStash)
        //                _characterInventory.UnequipItem(outwardSlot.EquippedItem, false, _character.Stash);
        //            else
        //                _characterInventory.UnequipItem(outwardSlot.EquippedItem, false);
        //        });
        //        _queuedEquips.Enqueue(() =>
        //        {
        //            Logger.LogDebug($"Equipping item UID {equipSlot.UID} to slot {outwardSlot.SlotType}.");
        //            _characterInventory.EquipItem(equipSlot.UID, performAnimation);
        //        });
        //    }
        //}

        private void PerformActionAfterCasting(Action action)
        {
            Logger.LogDebug($"Queued cast action. {_queuedCastActions.Count} actions queued.");
            _queuedCastActions.Enqueue(action);

            //if (!_character.IsCasting)
            //    action.Invoke();
            //else
            //{
            //    _queuedCastActions.Enqueue(action);
            //    //if (!_castQueueProcessing)
            //    //    _coroutines.StartRoutine(InvokeQueuedCastActions());
            //}
        }

        Queue<Action> _queuedCastActions = new Queue<Action>();
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

        //private void ProcessEquipQueues()
        //{
        //    if (!_frameQueueProcessing)
        //        _coroutines.StartRoutine(InvokeQueuedFrameActions());
        //}

        //Queue<Action> _queuedUnequips = new Queue<Action>();
        //Queue<Action> _queuedEquips = new Queue<Action>();
        //bool _frameQueueProcessing = false;
        //private IEnumerator InvokeQueuedFrameActions()
        //{
        //    _frameQueueProcessing = true;
        //    bool unequipHappened = false;
        //    while (_queuedUnequips.Count > 0)
        //    {
        //        _queuedUnequips.Dequeue().Invoke();
        //        unequipHappened = true;
        //    }
        //    if (unequipHappened)
        //        yield return null;

        //    while (_queuedEquips.Count > 0)
        //    {
        //        _queuedEquips.Dequeue().Invoke();
        //    }
        //    yield return null;
        //    _frameQueueProcessing = false;
        //}

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
                }
                _character = null;
                _equipmentSetsMenus = null;
                // TODO: set large fields to null
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
