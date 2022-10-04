using BepInEx;
using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.ActionUI.Monobehaviours;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class PlayerMenuService : IDisposable
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly GameObject _playerMenuPrefab;
        private readonly PositionsService _positionsService;
        private readonly LevelCoroutines _coroutine;
        private readonly GameObject _modInactivableGo;
        private readonly SharedServicesInjector _sharedServicesInjector;

        private float _initialPauseMenuHeight;
        private const int _baseActivePauseButtons = 7;

        private Button _actionUiGo;
        private bool disposedValue;

        public Button ActionMenusButton => _actionUiGo;

        public delegate void PlayerActionMenusConfigured(PlayerActionMenus actionMenus, SplitPlayer splitPlayer);
        public event PlayerActionMenusConfigured OnPlayerActionMenusConfigured;

        private readonly static Dictionary<int, ControllerMap> _controllerMaps = new Dictionary<int, ControllerMap>();

        public PlayerMenuService(BaseUnityPlugin baseUnityPlugin,
                                GameObject playerMenuPrefab,
                                SharedServicesInjector sharedServicesInjector,
                                PositionsService positionsService,
                                LevelCoroutines coroutine,
                                ModifGoService modifGoService,
                                Func<IModifLogger> getLogger)
        {
            _baseUnityPlugin = baseUnityPlugin;
            _playerMenuPrefab = playerMenuPrefab;
            _sharedServicesInjector = sharedServicesInjector;
            _positionsService = positionsService;
            _coroutine = coroutine;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            _getLogger = getLogger;


            SplitPlayerPatches.InitAfter += InjectMenus;
            _sharedServicesInjector.OnSharedServicesInjected += SetPlayerMenuCharacter;
            SplitScreenManagerPatches.RemoveLocalPlayerAfter += RemovePlayerMenu;
            PauseMenuPatches.AfterRefreshDisplay += (pauseMenu) => _coroutine.StartRoutine(ResizePauseMenu(pauseMenu));
        }

        private void InjectMenus(SplitPlayer splitPlayer)
        {
            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(InjectMenus)}(): Injecting PlayerActionMenus for player rewiredId {splitPlayer.RewiredID}.");
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);

            if (psp.TryGetService<PlayerActionMenus>(out var _))
                return;

            var playerMenuPrefab = _playerMenuPrefab.gameObject;
            var gamePanels = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels");
            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(InjectMenus)}(): Creating PlayerActionMenus instance for player rewiredId {splitPlayer.RewiredID}.");
            var playerMenuGo = UnityEngine.Object.Instantiate(playerMenuPrefab, gamePanels);

            psp.AddSingleton(playerMenuGo.GetComponentInChildren<HotkeyCaptureMenu>(true));
            playerMenuGo.SetActive(false);
            var playerMenu = playerMenuGo.GetComponent<PlayerActionMenus>();
            psp.AddSingleton(playerMenu);

            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(InjectMenus)}(): Injecting Positionable UI component.");
            _positionsService.InjectPositionableUIs(splitPlayer.CharUI);

            playerMenu.SetIDs(splitPlayer.RewiredID);

            if (!CharacterUIPatches.GetIsMenuFocused.ContainsKey(splitPlayer.RewiredID))
                CharacterUIPatches.GetIsMenuFocused.Add(splitPlayer.RewiredID, (playerId) => GetAnyMenuShowing(playerMenu, playerId));

            //UnityEngine.Object.DontDestroyOnLoad(playerMenuGo);

            InjectPauseMenu(splitPlayer, playerMenu);

        }

        private bool GetAnyMenuShowing(PlayerActionMenus actionMenus, int playerId)
        {
            if (actionMenus.PlayerID != playerId)
                return false;
            var isShowing = actionMenus.AnyActionMenuShowing();

            return isShowing;
        }

        private bool ShouldQuickslotItemBeDestroyed(bool actionSlotsActive, int playerID, int requestingPlayerID) => !actionSlotsActive || playerID != requestingPlayerID;

        private void SetPlayerMenuCharacter(SplitPlayer splitPlayer)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var playerMenu = psp.GetService<PlayerActionMenus>();
            var playerMenuGo = playerMenu.gameObject;
            var character = splitPlayer.AssignedCharacter;

            playerMenuGo.name = "PlayerActionMenus_" + character.UID;
            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(SetPlayerMenuCharacter)}(): Activating {playerMenuGo.name} for rewired ID {splitPlayer.RewiredID}.");
            playerMenuGo.SetActive(true);

            var profile = GetActiveProfile(playerMenu.ProfileManager);
            _positionsService.ToggleQuickslotsPositonable(profile, playerMenu, character.CharacterUI);

            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            var hud = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels/HUD");


            playerMenu.ConfigureNavigation(GetNavigationActions(player));

            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(SetPlayerMenuCharacter)}(): Adding SplitScreenScaler component to Action UIs.");
            AddSplitScreenScaler(playerMenu, character.CharacterUI);

            _positionsService.StartKeepPostionablesVisible(playerMenu, character.CharacterUI);

            if (profile.ActionSlotsEnabled)
            {
                //Add canvas with 0 sort order to drop panel to allow drag and drop to the hotbar.
                var dropPanel = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels/Menus/DropPanel");
                var dropCanvas = dropPanel.GetOrAddComponent<Canvas>();
                dropPanel.GetOrAddComponent<GraphicRaycaster>();
               
                _coroutine.DoNextFrame(() => {
                    dropCanvas.overrideSorting = true;
                    dropCanvas.sortingOrder = 0;
                    Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(SetPlayerMenuCharacter)}(): Changed '{dropCanvas.gameObject.GetPath()}' sortingOrder to {dropCanvas.sortingOrder}.");
                });
                
            }

            var isActionSlotsEnabled = playerMenu.ProfileManager.ProfileService.GetActiveProfile().ActionSlotsEnabled;
            if (!CharacterQuickSlotManagerPatches.AllowItemDestroyed.ContainsKey(splitPlayer.RewiredID))
                CharacterQuickSlotManagerPatches.AllowItemDestroyed.Add(splitPlayer.RewiredID,
                    (requestingPlayerId) => ShouldQuickslotItemBeDestroyed(
                        isActionSlotsEnabled,
                        splitPlayer.RewiredID,
                        requestingPlayerId)
                    );

            try
            {
                OnPlayerActionMenusConfigured?.Invoke(playerMenu, splitPlayer);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private MenuNavigationActions GetNavigationActions(Player player)
        {
            return new MenuNavigationActions()
            {
                MoveUp = () => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.MoveUp)),
                MoveDown = () => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.MoveDown)),
                MoveLeft = () => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.MoveLeft)),
                MoveRight = () => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.MoveRight)),
                Cancel = () => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)),
            };
        }

        private ActionUIProfile GetActiveProfile(ProfileManager profileManager) => (ActionUIProfile)profileManager.ProfileService.GetActiveProfile();

        private void AddSplitScreenScaler(PlayerActionMenus actionMenus, CharacterUI characterUI)
        {
            var menus = actionMenus.GetComponentsInChildren<IActionMenu>(true);
            foreach (var menu in menus)
            {
                var screenScaler = ((MonoBehaviour)menu).gameObject.GetOrAddComponent<SplitScreenScaler>();
                screenScaler.CharacterUI = characterUI;
            }
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
                if (CharacterUIPatches.GetIsMenuFocused.ContainsKey(player.RewiredID))
                    CharacterUIPatches.GetIsMenuFocused.Remove(player.RewiredID);

                UnityEngine.Object.Destroy(playerMenu.gameObject);
                Psp.Instance.GetServicesProvider(player.RewiredID).TryRemove<PlayerActionMenus>();
            }
        }

        private void InjectPauseMenu(SplitPlayer splitPlayer, PlayerActionMenus actionMenus)
        {
            Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(InjectPauseMenu)}(): Adding 'Action UI' button to pause menu.");
            //Get the main
            var hideOnPauseGo = splitPlayer.CharUI.transform.Find("Canvas/PauseMenu/Buttons/Content/HideOnPause/").gameObject;
            var buttons = hideOnPauseGo.GetComponentsInChildren<Button>();
            var actionUiButton = buttons.FirstOrDefault(b => b.name == "btnActionUiSettings");

            if (actionUiButton == null)
            {
                var settingsBtn = buttons.First(b => b.name.Equals("btnOptions", StringComparison.InvariantCultureIgnoreCase));

                _actionUiGo = UnityEngine.Object.Instantiate(settingsBtn);
                _actionUiGo.name = "btnActionUiSettings";
                var localScale = _actionUiGo.transform.localScale;
                _actionUiGo.transform.SetParent(settingsBtn.transform.parent, false);
                _actionUiGo.transform.localScale = localScale;
                _actionUiGo.transform.SetSiblingIndex(settingsBtn.transform.GetSiblingIndex() + 1);

                actionUiButton = _actionUiGo.GetComponent<Button>();

                var menuText = _actionUiGo.GetComponentInChildren<Text>();
                UnityEngine.Object.Destroy(menuText.GetComponent<UILocalize>());
                menuText.text = "Action UI";
            }
            //get the PauseMenu component so the PauseMenu UI can be hidden later
            var pauseMenu = splitPlayer.CharUI.transform.Find("Canvas/PauseMenu").GetComponent<PauseMenu>();
            _actionUiGo.onClick.RemoveAllListeners();
            //This removes any persistent (set in Unity Editor) onClick listeners.
            _actionUiGo.onClick = new Button.ButtonClickedEvent();
            //Add a new listener to hide the pause menu and show the Action Setting Menu
            _actionUiGo.onClick.AddListener(() =>
            {
                pauseMenu.Hide();
                actionMenus.MainSettingsMenu.Show();
            });
        }

        private IEnumerator ResizePauseMenu(PauseMenu pauseMenu)
        {
            yield return null;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SplitPlayerPatches.InitAfter -= InjectMenus;
                    if (_sharedServicesInjector != null)
                        _sharedServicesInjector.OnSharedServicesInjected -= SetPlayerMenuCharacter;
                    SplitScreenManagerPatches.RemoveLocalPlayerAfter -= RemovePlayerMenu;
                    PauseMenuPatches.AfterRefreshDisplay -= (pauseMenu) => _coroutine.StartRoutine(ResizePauseMenu(pauseMenu));
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PlayerMenuService()
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
