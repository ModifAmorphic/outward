using BepInEx;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
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
        private readonly HotbarSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly GameObject _playerMenuPrefab;
        private readonly LevelCoroutines _coroutine;
        private readonly GameObject _modInactivableGo;

        private float _initialPauseMenuHeight;
        private const int _baseActivePauseButtons = 7;

        private Button _actionSlotsButton;
        public Button ActionSlotsButton => _actionSlotsButton;

        private readonly static Dictionary<int, ControllerMap> _controllerMaps = new Dictionary<int, ControllerMap>();

        public PlayerMenuService(BaseUnityPlugin baseUnityPlugin,
                                GameObject playerMenuPrefab,
                                LevelCoroutines coroutine,
                                ModifGoService modifGoService,
                                HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            _baseUnityPlugin = baseUnityPlugin;
            _playerMenuPrefab = playerMenuPrefab;
            _coroutine = coroutine;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            _settings = settings;
            _getLogger = getLogger;


            SplitPlayerPatches.InitAfter += InjectMenus;
            SplitPlayerPatches.SetCharacterAfter += SetPlayerMenuCharacter;
            SplitScreenManagerPatches.RemoveLocalPlayerAfter += RemovePlayerMenu;
            PauseMenuPatches.AfterRefreshDisplay += ResizePauseMenu;

            //coroutine.InvokeAfterPlayersLoaded(() => NetworkLevelLoader.Instance, LoadDefaultControllerMaps, 300);

        }

        private void SetPlayerMenuCharacter(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.Instance.GetServicesProvider(splitPlayer.RewiredID);
            var playerMenu = psp.GetService<PlayerActionMenus>();
            var playerMenuGo = playerMenu.gameObject;
            playerMenuGo.name = "PlayerActionMenus_" + character.UID;

            playerMenuGo.SetActive(true);

            psp.AddSingleton(playerMenuGo.GetComponentInChildren<HotbarsContainer>());

            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            var gamePanels = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels");
            playerMenuGo.transform.SetParent(gamePanels.transform);
            playerMenu.ConfigureExit(() => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)));

            //_coroutine.StartRoutine(SetSortOrderNextFrame(playerMenuGo));
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
            playerMenu.SetIDs(splitPlayer.RewiredID);
            //MenuManagerPatches.GetIsApplicationFocused = playerMenu.AnyActionMenuShowing;
            CharacterUIPatches.GetIsMenuFocused = playerMenu.AnyActionMenuShowing;
            UnityEngine.Object.DontDestroyOnLoad(playerMenuGo);

            //Get the main
            var hideOnPauseGo = splitPlayer.CharUI.transform.Find("Canvas/PauseMenu/Buttons/Content/HideOnPause/").gameObject;
            var settingsBtn = hideOnPauseGo.GetComponentsInChildren<Button>().First(b => b.name.Equals("btnOptions", StringComparison.InvariantCultureIgnoreCase));

            _actionSlotsButton = UnityEngine.Object.Instantiate(settingsBtn, hideOnPauseGo.transform);
            _actionSlotsButton.name = "btnActionMenuHotkeys";
            _actionSlotsButton.transform.SetSiblingIndex(settingsBtn.transform.GetSiblingIndex() + 1);
            
            var menuText = _actionSlotsButton.GetComponentInChildren<Text>();
            UnityEngine.Object.Destroy(menuText.GetComponent<UILocalize>());
            menuText.text = "Action Slots";

            //get the PauseMenu component so the PauseMenu UI can be hidden later
            var pauseMenu = splitPlayer.CharUI.transform.Find("Canvas/PauseMenu").GetComponent<PauseMenu>();
            //This removes any persistent (set in Unity Editor) onClick listeners.
            _actionSlotsButton.onClick = new Button.ButtonClickedEvent();
            //Add a new listener to hide the pause menu and show the Hotbar Setting Menu
            _actionSlotsButton.onClick.AddListener(() => {
                pauseMenu.Hide();
                playerMenu.HotbarSettingsViewer.Show();
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
