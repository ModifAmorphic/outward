using BepInEx;
using ModifAmorphic.Outward.ActionMenus.DataModels;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class PlayerMenuService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly GameObject _playerMenuPrefab;
        private readonly LevelCoroutines _coroutine;
        private readonly GameObject _modInactivableGo;

        private float _initialPauseMenuHeight;
        private const int _baseActivePauseButtons = 7;

        private Button _actionMenusButton;
        public Button ActionMenusButton => _actionMenusButton;

        private readonly static Dictionary<int, ControllerMap> _controllerMaps = new Dictionary<int, ControllerMap>();

        public PlayerMenuService(BaseUnityPlugin baseUnityPlugin,
                                GameObject playerMenuPrefab,
                                LevelCoroutines coroutine,
                                ModifGoService modifGoService,
                                Func<IModifLogger> getLogger)
        {
            _baseUnityPlugin = baseUnityPlugin;
            _playerMenuPrefab = playerMenuPrefab;
            _coroutine = coroutine;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            _getLogger = getLogger;


            SplitPlayerPatches.InitAfter += InjectMenus;
            SplitPlayerPatches.SetCharacterAfter += SetPlayerMenuCharacter;
            SplitScreenManagerPatches.RemoveLocalPlayerAfter += RemovePlayerMenu;
            PauseMenuPatches.AfterRefreshDisplay += ResizePauseMenu;

            //coroutine.InvokeAfterPlayersLoaded(() => NetworkLevelLoader.Instance, LoadDefaultControllerMaps, 300);

        }
        private Coroutine AAKeepAliveCoroutine;

        private void InjectMenus(SplitPlayer splitPlayer)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);

            if (psp.TryGetService<PlayerActionMenus>(out var _))
                return;

            var playerMenuPrefab = _playerMenuPrefab.gameObject;
            var gamePanels = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels");
            var playerMenuGo = UnityEngine.Object.Instantiate(playerMenuPrefab, gamePanels);

            psp.AddSingleton(playerMenuGo.GetComponentInChildren<HotkeyCaptureMenu>(true));
            playerMenuGo.SetActive(false);
            var playerMenu = playerMenuGo.GetComponent<PlayerActionMenus>();
            psp.AddSingleton(playerMenu);

            InjectPositionableUIs(splitPlayer.CharUI);

            playerMenu.SetIDs(splitPlayer.RewiredID);
            //MenuManagerPatches.GetIsApplicationFocused = playerMenu.AnyActionMenuShowing;
            CharacterUIPatches.GetIsMenuFocused = playerMenu.AnyActionMenuShowing;
            UnityEngine.Object.DontDestroyOnLoad(playerMenuGo);

            InjectPauseMenu(splitPlayer, playerMenu);
        }

        private void SetPlayerMenuCharacter(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var playerMenu = psp.GetService<PlayerActionMenus>();
            var playerMenuGo = playerMenu.gameObject;
            playerMenuGo.name = "PlayerActionMenus_" + character.UID;
            playerMenuGo.SetActive(true);

            var profile = GetOrCreateActiveProfile(playerMenu.ProfileManager);
            ToggleQuickslotsPositonable(profile, playerMenu, character.CharacterUI);

            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            var hud = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels/HUD");
            //playerMenuGo.transform.SetParent(gamePanels.transform);
            playerMenu.ConfigureExit(() => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)));

            var uiPositionScreen = playerMenuGo.GetComponentInChildren<UIPositionScreen>();
            var positonables = hud.GetComponentsInChildren<PositionableUI>(true).Where(p => p != null);
            uiPositionScreen.OnShow.AddListener(() => AAKeepAliveCoroutine = _coroutine.StartRoutine(KeepPositionablesVisible(positonables.ToArray())));

            //_coroutine.StartRoutine(SetSortOrderNextFrame(playerMenuGo));
        }

        private ActionMenusProfile GetOrCreateActiveProfile(ProfileManager profileManager)
        {
            var activeProfile = profileManager.GetActiveProfile();

            if (activeProfile == null)
            {
                Logger.LogDebug($"No active profile set. Checking if any profiles exist");
                var names = profileManager.GetProfileNames();
                if (names == null || !names.Any())
                {
                    Logger.LogDebug($"No profiles found. Creating default profile '{ActionMenuSettings.DefaultProfile.Name}'");
                    profileManager.ProfileService.SaveNew(ActionMenuSettings.DefaultProfile);
                    names = profileManager.GetProfileNames();
                }
                else
                    profileManager.SetActiveProfile(names.First());
            }

            return (ActionMenusProfile)profileManager.GetActiveProfile();
        }

        private IEnumerator KeepPositionablesVisible(PositionableUI[] positionables)
        {
            var needsDisable = new List<PositionableUI>();
            var zeroAlphas = new List<CanvasGroup>();
            var noBlocks = new List<CanvasGroup>();

            var cdl = positionables.FirstOrDefault(p => p.name == "ConfirmDeployListener");
            var cdlNeedsInactive = false;
            if (cdl != null)
            {
                var interactionPress = cdl.transform.Find("InteractionPress").gameObject;
                if (!interactionPress.activeSelf)
                {
                    cdlNeedsInactive = true;
                    interactionPress.SetActive(true);
                }
            }

            //yield return new WaitForEndOfFrame();
            while (positionables[0].IsPositionable)
            {
                for (int i = 1; i < positionables.Length; i++)
                {
                    if (!positionables[i].gameObject.activeSelf)
                    {
                        positionables[i].gameObject.SetActive(true);
                        needsDisable.Add(positionables[i]);
                    }
                    var cg = positionables[i].GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        if (cg.alpha == 0f)
                        {
                            cg.alpha = 1f;
                            zeroAlphas.Add(cg);
                        }
                        if (!cg.blocksRaycasts)
                        {
                            cg.blocksRaycasts = true;
                            noBlocks.Add(cg);
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            
            if (cdlNeedsInactive)
                cdl.transform.Find("InteractionPress").gameObject.SetActive(false);

            foreach (var positionable in needsDisable)
            {
                if (positionable.gameObject.activeSelf)
                    positionable.gameObject.SetActive(false);
            }
            foreach (var cg in zeroAlphas)
            {
                if (cg.alpha > 0f)
                    cg.alpha = 0f;
            }
            foreach (var cg in noBlocks)
            {
                if (cg.blocksRaycasts)
                    cg.blocksRaycasts = false;
            }
        }

        private void ToggleQuickslotsPositonable(ActionMenusProfile profile, PlayerActionMenus actionMenus, CharacterUI characterUI)
        {
            if (!profile.ActionSlotsEnabled)
            {
                var hotbars = actionMenus.gameObject.GetComponentInChildren<HotbarsContainer>();
                hotbars.gameObject.Destroy();
            }
            else
            {
                var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
                var quickSlot = hud.Find("QuickSlot").gameObject;
                UnityEngine.Object.Destroy(quickSlot.GetComponent<PositionableUI>());
                UnityEngine.Object.Destroy(quickSlot.transform.Find("PositionableBg").gameObject);
            }
        }

        private HashSet<string> _positionBlocklist = new HashSet<string>()
        {
            "CorruptionSmog", "PanicOverlay", "TargetingFlare", "CharacterBars", "LowHealth", "LowStamina", "Chat - Panel"
        };
        private void InjectPositionableUIs(CharacterUI characterUI)
        {
            var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
            //hud.GetOrAddComponent<GraphicRaycaster>();

            for (int i = 0; i < hud.childCount; i++)
            {
                if (hud.GetChild(i) is RectTransform uiRect)
                {
                    if (uiRect.name == "Tutorialization_DropBag" || uiRect.name == "Tutorialization_UseBandage")
                        uiRect = uiRect.Find("Panel") as RectTransform;

                    if (!_positionBlocklist.Contains(uiRect.name))
                        AddPositionableUI(uiRect);
                }
            }
        }
        private void AddPositionableUI(RectTransform transform)
        {
            var bgPrefab = _modInactivableGo.transform.Find("PositionableBg") as RectTransform;
            var bg = UnityEngine.Object.Instantiate(bgPrefab, transform);
            bg.name = bgPrefab.name;
            bg.SetAsLastSibling();
            var positionableUI = transform.gameObject.AddComponent<PositionableUI>();
            positionableUI.BackgroundImage = bg.GetComponent<Image>();
            positionableUI.ResetButton = bg.GetComponentInChildren<Button>();
            bg.GetComponentInChildren<Text>().text = transform.name;
        }

        private IEnumerator SetSortOrderNextFrame(GameObject actionMenus)
        {
            yield return null;

            var mainCanvas = actionMenus.transform.Find("MenuCanvas").gameObject.GetComponent<Canvas>();
            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(SetPlayerMenuCharacter)}(): Setting overrideSorting to true for Canvas {mainCanvas?.name}.");
            mainCanvas.overrideSorting = true;
            mainCanvas.sortingOrder = 2;
        }
        private void RemovePlayerMenu(SplitScreenManager splitScreenManager, SplitPlayer player, string playerId)
        {
            if (Psp.Instance.GetServicesProvider(player.RewiredID).TryGetService<PlayerActionMenus>(out var playerMenu))
            {
                UnityEngine.Object.Destroy(playerMenu.gameObject);
                Psp.Instance.GetServicesProvider(player.RewiredID).TryRemove<PlayerActionMenus>();
            }
        }

        private void InjectPauseMenu(SplitPlayer splitPlayer, PlayerActionMenus actionMenus)
        {
            //Get the main
            var hideOnPauseGo = splitPlayer.CharUI.transform.Find("Canvas/PauseMenu/Buttons/Content/HideOnPause/").gameObject;
            var settingsBtn = hideOnPauseGo.GetComponentsInChildren<Button>().First(b => b.name.Equals("btnOptions", StringComparison.InvariantCultureIgnoreCase));

            _actionMenusButton = UnityEngine.Object.Instantiate(settingsBtn, hideOnPauseGo.transform);
            _actionMenusButton.name = "btnActionMenuSettings";
            _actionMenusButton.transform.SetSiblingIndex(settingsBtn.transform.GetSiblingIndex() + 1);

            var menuText = _actionMenusButton.GetComponentInChildren<Text>();
            UnityEngine.Object.Destroy(menuText.GetComponent<UILocalize>());
            menuText.text = "Action Menus";

            //get the PauseMenu component so the PauseMenu UI can be hidden later
            var pauseMenu = splitPlayer.CharUI.transform.Find("Canvas/PauseMenu").GetComponent<PauseMenu>();
            //This removes any persistent (set in Unity Editor) onClick listeners.
            _actionMenusButton.onClick = new Button.ButtonClickedEvent();
            //Add a new listener to hide the pause menu and show the Action Setting Menu
            _actionMenusButton.onClick.AddListener(() => {
                pauseMenu.Hide();
                actionMenus.MainSettingsMenu.Show();
            });
            //_actionSlotsButton.transform.SetParent(pauseMenu.transform, false);
        }

        private void ResizePauseMenu(PauseMenu pauseMenu)
        {
            //var pauseMenuPrefab = Resources.FindObjectsOfTypeAll<PauseMenu>().First(p => p.gameObject.GetPath().Equals("/PlayerUI/Canvas/PauseMenu"));
            //var prefabRect = pauseMenuPrefab.transform.Find("Buttons").GetComponent<RectTransform>();
            //var prefabButtons = pauseMenuPrefab.transform.Find("Buttons/Content/HideOnPause").GetComponentsInChildren<Button>();

            var parentRect = pauseMenu.transform.Find("Buttons").GetComponent<RectTransform>();
            if (_initialPauseMenuHeight == 0f)
                _initialPauseMenuHeight = parentRect.rect.height;

            var hideOnPause = pauseMenu.transform.Find("Buttons/Content/HideOnPause");

            var vertLayout = hideOnPause.GetComponent<VerticalLayoutGroup>();
            var buttons = hideOnPause.GetComponentsInChildren<Button>();

            float btnHeight = buttons.First().GetComponent<RectTransform>().rect.height;
                
            float increaseHeight = (vertLayout.spacing + btnHeight) * (buttons.Length - _baseActivePauseButtons);
            Logger.LogDebug($"Resizing Pause Menu height from {parentRect.rect.height} to {_initialPauseMenuHeight + increaseHeight}.");
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _initialPauseMenuHeight + increaseHeight);
        }
    }
}
