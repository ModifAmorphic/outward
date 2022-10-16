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
    internal class SkillChainsService : IDisposable, IStartable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private Character _character;
        private CharacterInventory _characterInventory => _character.Inventory;
        private CharacterEquipment _characterEquipment => _character.Inventory.Equipment;
        private readonly ProfileManager _profileManager;
        private readonly InventoryService _inventoryService;
        private readonly SkillChainPrefabricator _skillChainPrefabricator;
        private IActionUIProfile _profile => _profileManager.ProfileService.GetActiveProfile();
        //private EquipmentSetsSettingsProfile _equipSetsProfile => _profileManager.ProfileService.GetActiveProfile().EquipmentSetsSettingsProfile;
        private EquipmentSetMenu _equipmentSetsMenus;
        private readonly LevelCoroutines _coroutines;


        //private bool _firstSetsAddedComplete;
        private bool _skillChainsEnabled;

        private bool disposedValue;

        //private Dictionary<EquipSlots, EquipSkillPreview> _cachedSkillPreviews = new Dictionary<EquipSlots, EquipSkillPreview>();

        public SkillChainsService(Character character, ProfileManager profileManager, InventoryService inventoryService, SkillChainPrefabricator skillChainPrefabricator, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            //_equipmentSetsMenus = equipmentSetsMenus;
            _inventoryService = inventoryService;
            _skillChainPrefabricator = skillChainPrefabricator;
            _coroutines = coroutines;
            _getLogger = getLogger;
        }

        public void Start()
        {
            try
            {
                _skillChainsEnabled = _profile.SkillChainsEnabled;
                //EquipmentMenuPatches.AfterShow += ShowSkillChainsMenu;
                //EquipmentMenuPatches.AfterOnHide += HideSkillChainsMenu;
                //ItemDisplayPatches.AfterSetReferencedItem += _skillChainPrefabricator.AddEquipmentSetIcon;
                _profileManager.ProfileService.OnActiveProfileChanged += TryProfileChanged;
                _profileManager.ProfileService.OnActiveProfileSwitched += TryProfileSwitched;
                //_profileManager.SkillChainService.SaveSkillChain(new SkillChain()
                //{
                //    ItemID = -1320000000,
                //    Name = "Runic Protection",
                //    ActionChain = new SortedList<int, ChainAction>
                //    {
                //        { 0, new ChainAction() { ItemID = 8100210 } },
                //        { 1, new ChainAction() { ItemID = 8100230 } },
                //    }
                //});
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(EquipService)}.", ex);
            }
        }

        private void HideSkillChainsMenu(EquipmentMenu obj)
        {
            if (obj.LocalCharacter.UID != _character.UID || (!_profileManager.ProfileService.GetActiveProfile().EquipmentSetsEnabled && !_equipmentSetsMenus.gameObject.activeSelf))
                return;

            _equipmentSetsMenus.Hide();
        }

        private void ShowSkillChainsMenu(EquipmentMenu equipMenu)
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
            if (_skillChainsEnabled != profile.EquipmentSetsEnabled)
            {
                _skillChainsEnabled = profile.EquipmentSetsEnabled;

                if (_skillChainsEnabled)
                {
                    AddSkillChains(_profileManager.SkillChainService.GetSkillChainProfile().SkillChains, true);
                }
                else
                {
                    //ClearSkillPreviewCache();
                    RemoveSkillChains(_profileManager.SkillChainService.GetSkillChainProfile().SkillChains);
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
            _skillChainsEnabled = profile.SkillChainsEnabled;

            //ClearSkillPreviewCache(); 
            RemoveUnknownSkillChains(_profileManager.SkillChainService.GetSkillChainProfile().SkillChains);

            if (_skillChainsEnabled)
            {
                AddSkillChains(_profileManager.SkillChainService.GetSkillChainProfile().SkillChains, true);
            }
            else
            {
                RemoveSkillChains(_profileManager.SkillChainService.GetSkillChainProfile().SkillChains);
            }

        }

        private void AddSkillChains(IEnumerable<SkillChain> skillChains, bool learnSets = false)
        {
            Logger.LogDebug($"Adding {skillChains?.Count()} {nameof(SkillChain)} prefabs for character {_character.name}.");
            foreach (var chain in skillChains)
            {
                var skill = _skillChainPrefabricator.AddOrGetSkillPrefab(chain);
                if (learnSets)
                    AddOrUpdateChainedSkill(skill);
                //if (skill.ItemDisplay != null)
                //    AddEquipmentSetIcon(skill.ItemDisplay, skill);
            }
        }

        public void AddOrUpdateChainedSkill(SkillChain skillChain)
        {
            var skill = _skillChainPrefabricator.AddOrGetSkillPrefab(skillChain);
            AddOrUpdateChainedSkill(skill);
        }

        public void AddOrUpdateChainedSkill(ChainedSkill skillPrefab)
        {
            var existingSkill = (ChainedSkill)_characterInventory.SkillKnowledge.transform.GetComponentsInChildren<Skill>().FirstOrDefault(s => s.ItemID == skillPrefab.ItemID);

            Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: {_characterInventory.SkillKnowledge.GetLearnedItems().Count} skills are known to character {_character.name}. existingSkill is {existingSkill?.name}");
            if (!_characterInventory.SkillKnowledge.IsItemLearned(skillPrefab) && existingSkill == null)
            {
                _characterInventory.TryUnlockSkill(skillPrefab);
                Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: Added skill {skillPrefab.name} to character {_character.name}.");
            }
            else
            {
                var learnedSkill = (ChainedSkill)_characterInventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(s => s.ItemID == skillPrefab.ItemID) ?? existingSkill;
                learnedSkill.SetChain(skillPrefab.DisplayName, skillPrefab.ItemID, skillPrefab.ChainSteps);
                Logger.LogDebug($"AddOrUpdateEquipmentSetSkill: Character {_character.name} had existing {nameof(ChainedSkill)} {learnedSkill.name} updated with latest skill chains.");
            }
        }

        public void RemoveSkillChains(IEnumerable<SkillChain> skillChains)
        {
            var removeChains = skillChains.ToList();
            for (int i = 0; i < removeChains.Count; i++)
                RemoveSkillChains(removeChains[i].ItemID);
        }

        public void RemoveUnknownSkillChains(IEnumerable<SkillChain> skillChains)
        {
            Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveUnknownSkillChains)}: Removing any unknown {nameof(SkillChain)}s.");
            var chainedSkills = _characterInventory.SkillKnowledge.GetLearnedItems().Where(s => s is ChainedSkill).Cast<ChainedSkill>().ToArray();

            for (int i = 0; i < chainedSkills.Length; i++)
            {
                if (!skillChains.Any(s => s.ItemID == chainedSkills[i].ItemID))
                    RemoveSkillChains(chainedSkills[i].ItemID);
            }
        }

        public void RemoveSkillChains(int itemID)
        {
            if (!_characterInventory.SkillKnowledge.IsItemLearned(itemID))
                return;

            var setSkill = _characterInventory.SkillKnowledge.GetLearnedItems().FirstOrDefault(l => l.ItemID == itemID);

            if (setSkill != null)
            {
                setSkill.transform.parent = null;
                setSkill.gameObject.Destroy();
                Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveSkillChains)}: Destroyed skill with ItemID == {itemID}.");
            }

            _characterInventory.SkillKnowledge.RemoveItem(itemID);

            _skillChainPrefabricator.RemoveSkillChainPrefab(itemID);
        }

        //public EquipSkillPreview GetEquipSkillPreview(IEquipmentSet set)
        //{
        //    if (_cachedSkillPreviews.TryGetValue(set.IconSlot, out var skillPreview))
        //        return skillPreview;

        //    _cachedSkillPreviews.Add(set.IconSlot, new EquipSkillPreview(_characterEquipment.GetMatchingSlot(ActionUiEquipSlotsXRef[set.IconSlot])));
        //    return _cachedSkillPreviews[set.IconSlot];
        //}

        //private void ClearSkillPreviewCache()
        //{
        //    var skillPreviews = _cachedSkillPreviews.Values.ToArray();
        //    for (int i = 0; i < skillPreviews.Length; i++)
        //        skillPreviews[i].Dispose();

        //    _cachedSkillPreviews.Clear();
        //}

        //private void AddEquipmentSetIcon(ItemDisplay itemDisplay, Item item)
        //{

        //    var existingIcons = itemDisplay.GetComponentsInChildren<Image>().Where(i => i.name == "imgEquipmentSet").ToArray();

        //    if (!(item is EquipmentSetSkill setSkill))
        //    {
        //        for (int i = 0; i < existingIcons.Length; i++)
        //            UnityEngine.Object.Destroy(existingIcons[i].gameObject);
        //        return;
        //    }
        //    else if (existingIcons.Any())
        //        return;

        //    var enchantedGo = itemDisplay.transform.Find("imgEnchanted").gameObject;
        //    var equipmentSetIconGo = UnityEngine.Object.Instantiate(enchantedGo, itemDisplay.transform);
        //    var existingImage = equipmentSetIconGo.GetComponent<Image>();
        //    UnityEngine.Object.DestroyImmediate(existingImage);

        //    equipmentSetIconGo.name = "imgEquipmentSet";
        //    var newImage = equipmentSetIconGo.AddComponent<Image>();
        //    newImage.sprite = ActionMenuResources.Instance.SpriteResources["EquipmentSetIcon"];
        //    equipmentSetIconGo.SetActive(true);

        //    Logger.LogDebug($"Added EquipmentSetIcon {equipmentSetIconGo.name} to ItemDisplay {itemDisplay.gameObject.name}.");
        //}

        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_profileManager?.ProfileService != null)
                    {
                        _profileManager.ProfileService.OnActiveProfileChanged -= TryProfileChanged;
                        _profileManager.ProfileService.OnActiveProfileSwitched -= TryProfileSwitched;
                    }
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
