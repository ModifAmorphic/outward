using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Monobehaviours;
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
        private SkillChainMenu _skillChainMenu;
        private readonly LevelCoroutines _coroutines;


        //private bool _firstSetsAddedComplete;
        private bool _skillChainsEnabled;
        private bool _menuFirstShown;

        private bool disposedValue;

        //private Dictionary<EquipSlots, EquipSkillPreview> _cachedSkillPreviews = new Dictionary<EquipSlots, EquipSkillPreview>();

        public SkillChainsService(Character character, ProfileManager profileManager, SkillChainMenu skillChainMenu, InventoryService inventoryService, SkillChainPrefabricator skillChainPrefabricator, LevelCoroutines coroutines, Func<IModifLogger> getLogger)
        {
            _character = character;
            _profileManager = profileManager;
            _skillChainMenu = skillChainMenu;

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
                MenuPanelPatches.AfterShowInventoryMenu += ShowSkillChainsMenu;
                MenuPanelPatches.AfterOnHideMenuPanel += HideSkillChainsMenu;
                //MenuPanelPatches.AfterOnHideInventoryMenu += HideSkillChainsMenu;
                MenuPanelPatches.AfterShowSkillMenu += ShowSkillChainsMenu;
                //MenuPanelPatches.AfterOnHideSkillMenu += HideSkillChainsMenu;
                _profileManager.ProfileService.OnActiveProfileChanged += TryProfileChanged;
                _profileManager.ProfileService.OnActiveProfileSwitched += TryProfileSwitched;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {nameof(EquipService)}.", ex);
            }
        }

        private void HideSkillChainsMenu(MenuPanel menuPanel)
        {
            Logger.LogDebug($"menuPanel=={(menuPanel == null ? "null" : "not null")}, menuPanel.LocalCharacter=={(menuPanel?.LocalCharacter == null ? "null" : "not null")}, _character=={(_character == null ? "null" : "not null")}" +
                $", _profileManager=={(_profileManager == null ? "null" : "not null")}" +
                $", _profileManager.ProfileService=={(_profileManager?.ProfileService == null ? "null" : "not null")}" +
                $", _profileManager.ProfileService.GetActiveProfile()=={(_profileManager?.ProfileService?.GetActiveProfile() == null ? "null" : "not null")}");
            if (menuPanel?.LocalCharacter?.UID != _character.UID || (!_profileManager.ProfileService.GetActiveProfile().SkillChainsEnabled))
                return;
            if (_skillChainMenu.IsShowing)
                _skillChainMenu.Hide(true);
            _skillChainMenu.ToggleChainsButton.gameObject.SetActive(false);
        }

        private void ShowSkillChainsMenu(MenuPanel menuPanel)
        {
            if (menuPanel.LocalCharacter.UID != _character.UID || !_profileManager.ProfileService.GetActiveProfile().SkillChainsEnabled)
                return;

            if (_skillChainMenu.ParentTransform.parent != menuPanel.transform.parent)
                _skillChainMenu.ParentTransform.SetParent(menuPanel.transform.parent);

            _skillChainMenu.ToggleChainsButton.gameObject.SetActive(true);

            var actionView = _skillChainMenu.ActionPrefab.gameObject.GetOrAddComponent<ActionItemViewDropper>();
            actionView.SetPlayerID(_character.OwnerPlayerSys.PlayerID);

            _skillChainMenu.Show();
            if (!_menuFirstShown)
                _coroutines.DoNextFrame(() => PositionMenu());
            else
                PositionMenu();

            _menuFirstShown = true;
        }

        private void PositionMenu()
        {
            var mainMenuTransform = _character.CharacterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel") as RectTransform;
            var menuBg = mainMenuTransform.Find("Background").GetComponent<Image>();
            var refCorners = new Vector3[4];
            ((RectTransform)menuBg.transform).GetWorldCorners(refCorners);
            var topLeft = refCorners[1];

            Logger.LogDebug($"(x={topLeft.x}, y={topLeft.x})");

            _skillChainMenu.ParentTransform.anchorMax = new Vector2(1, 1);
            _skillChainMenu.ParentTransform.anchorMin = new Vector2(1, 1);
            var xPos = topLeft.x - 39 * menuBg.transform.lossyScale.x;
            //var xPos = topLeft.x; // + 20;
            var yPos = topLeft.y; // + 13;
            _skillChainMenu.ParentTransform.position = new Vector3(xPos, yPos);
            _skillChainMenu.ParentTransform.anchoredPosition = new Vector3(_skillChainMenu.ParentTransform.anchoredPosition.x + 25, _skillChainMenu.ParentTransform.anchoredPosition.y);
            _skillChainMenu.GetComponent<Image>().material = menuBg.material;
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
            if (_skillChainsEnabled != profile.SkillChainsEnabled)
            {
                _skillChainsEnabled = profile.SkillChainsEnabled;

                if (_skillChainsEnabled)
                {
                    AddSkillChains(_profileManager.SkillChainService.GetSkillChainProfile().SkillChains, true);
                }
                else
                {
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
                if (string.IsNullOrWhiteSpace(skillPrefab.StatusEffectIcon))
                    learnedSkill.SetChain(skillPrefab.DisplayName, skillPrefab.ItemID, skillPrefab.IconItemID, skillPrefab.ChainSteps);
                else
                    learnedSkill.SetChain(skillPrefab.DisplayName, skillPrefab.ItemID, skillPrefab.StatusEffectIcon, skillPrefab.ChainSteps);

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

        private void ShowSkillChainMenu(MenuPanel menuPanel)
        {
            if (menuPanel.LocalCharacter.UID != _character.UID || !_profileManager.ProfileService.GetActiveProfile().EquipmentSetsEnabled)
                return;

            if (_skillChainMenu.transform.parent != menuPanel.transform)
                _skillChainMenu.transform.SetParent(menuPanel.transform);
            _skillChainMenu.Show();

            _coroutines.DoNextFrame(() =>
            {
                var mainMenuTransform = _character.CharacterUI.transform.Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel") as RectTransform;
                var menuBg = mainMenuTransform.Find("Background").GetComponent<Image>();
                var refCorners = new Vector3[4];
                ((RectTransform)menuBg.transform).GetWorldCorners(refCorners);
                var topLeft = refCorners[1];

                Logger.LogDebug($"(x={topLeft.x}, y={topLeft.x})");

                var equipRectTransform = _skillChainMenu.transform as RectTransform;
                equipRectTransform.anchorMax = new Vector2(1, 1);
                equipRectTransform.anchorMin = new Vector2(1, 1);
                var xPos = topLeft.x; // + 20;
                var yPos = topLeft.y; // + 13;
                equipRectTransform.position = new Vector3(xPos, yPos);
                equipRectTransform.anchoredPosition = new Vector3(equipRectTransform.anchoredPosition.x + 25, equipRectTransform.anchoredPosition.y);
                equipRectTransform.GetComponent<Image>().material = menuBg.material;
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_profileManager?.ProfileService != null)
                    {
                        MenuPanelPatches.AfterShowInventoryMenu -= ShowSkillChainsMenu;
                        MenuPanelPatches.AfterOnHideMenuPanel -= HideSkillChainsMenu;
                        MenuPanelPatches.AfterShowSkillMenu -= ShowSkillChainsMenu;
                        if (_profileManager?.ProfileService != null)
                        {
                            _profileManager.ProfileService.OnActiveProfileChanged -= TryProfileChanged;
                            _profileManager.ProfileService.OnActiveProfileSwitched -= TryProfileSwitched;
                        }
                    }
                }
                _character = null;
                _skillChainMenu = null;
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
