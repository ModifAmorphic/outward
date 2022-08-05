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
using System.Text;
using UnityEngine;

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
        //private GameObject _overhaulMenusGo;
        //public readonly static Dictionary<int, PlayerMenu> PlayerMenu = new Dictionary<int, PlayerMenu>();

        private readonly static Dictionary<int, ControllerMap> _controllerMaps = new Dictionary<int, ControllerMap>();

        private GameObject baseHud;

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
            //_hotbars = hotbars;
            _settings = settings;
            _getLogger = getLogger;

            //GetOrAddMenuGameObject();

            SplitPlayerPatches.InitAfter += InjectMenus;
            SplitPlayerPatches.SetCharacterAfter += SetPlayerMenuCharacter;
            SplitScreenManagerPatches.RemoveLocalPlayerAfter += RemovePlayerMenu;
            coroutine.InvokeAfterPlayersLoaded(() => NetworkLevelLoader.Instance, LoadDefaultControllerMaps, 300);

            //SplitScreenManagerPatches.AwakeAfter += InjectMenus;
            //MenuManagerPatches.AwakeBefore += MenuManagerPatches_AwakeBefore;
            //settings.HotbarsChanged += (bars) => ConfigureSlots();
            //settings.ActionSlotsChanged += (slots) => ConfigureSlots();
            //WaitAttachAsset();
            //ConfigureSlots();
        }

        private void SetPlayerMenuCharacter(SplitPlayer splitPlayer, Character character)
        {
            var psp = Psp.GetServicesProvider(splitPlayer.RewiredID);
            var playerMenus = psp.GetService<PlayerMenu>().gameObject;
            //var actionMenus = playerMenu.gameObject;
            playerMenus.name = character.name + "_UIX";

            playerMenus.SetActive(true);

            psp.AddSingleton(playerMenus.GetComponentInChildren<HotbarsContainer>());

            var player = ReInput.players.GetPlayer(splitPlayer.RewiredID);

            playerMenus.GetComponentInChildren<HotbarSettingsViewer>(true).ConfigureExit(() => player.GetButtonDown(ControlsInput.GetMenuActionName(ControlsInput.MenuActions.Cancel)));

            //PlayerMenus[splitPlayer.RewiredID].HotbarService = 
            //    _hotbarServiceFac(playerMenus.GetComponentInChildren<HotbarsContainer>(), ReInput.players.GetPlayer(splitPlayer.RewiredID), splitPlayer.AssignedCharacter);

            var hud = splitPlayer.CharUI.transform.Find("Canvas/GameplayPanels/HUD");
            playerMenus.transform.SetParent(hud.transform);

            _coroutine.StartRoutine(SetSortOrderNextFrame(playerMenus));
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
            if (Psp.GetServicesProvider(player.RewiredID).TryGetService<PlayerMenu>(out var playerMenu))
            {
                UnityEngine.Object.Destroy(playerMenu.gameObject);
                Psp.GetServicesProvider(player.RewiredID).TryRemove<PlayerMenu>();
            }
        }

        //private GameObject GetOrAddMenuGameObject()
        //{
        //    if (_overhaulMenusGo == null)
        //    {
        //        _overhaulMenusGo = new GameObject(ActionMenuSettings.GameObjectName);
        //        UnityEngine.Object.DontDestroyOnLoad(_overhaulMenusGo);
        //    }
        //    return _overhaulMenusGo;
        //}

        private void InjectMenus(SplitPlayer splitPlayer)
        {
            var psp = Psp.GetServicesProvider(splitPlayer.RewiredID);

            if (psp.TryGetService<PlayerMenu>(out var _))
                return;

            var playerMenuPrefab = _playerMenuPrefab.gameObject;
            var playerMenuGo = GameObject.Instantiate(playerMenuPrefab);
            playerMenuGo.SetActive(false);
            //PlayerMenus.Add(splitPlayer.RewiredID, new PlayerActionMenus() { PlayerMenu = _playerMenuPrefab });
            var playerMenu = playerMenuGo.GetComponent<PlayerMenu>();
            psp.AddSingleton(playerMenu);
            playerMenu.SetIDs(splitPlayer.RewiredID);

            //PlayerMenus[splitPlayer.RewiredID].PlayerMenu.SetIDs(splitPlayer.RewiredID, splitPlayer.PlayerUID);
            //PlayerMenus[splitPlayer.RewiredID].PlayerMenu.SetIDs(splitPlayer.RewiredID);
            
            //playerMenuGo.transform.SetParent(_overhaulMenusGo.transform);
            UnityEngine.Object.DontDestroyOnLoad(playerMenuGo);

            
            //var hotbarPanel = playerMenuGo.GetComponentInChildren<HotbarsContainer>().transform;

            //_modInactivableGo
        }
        private void LoadDefaultControllerMaps()
        {
            foreach (var player in ReInput.players.AllPlayers)
            {
                if (player.name.Equals("System", StringComparison.OrdinalIgnoreCase))
                    continue;

                Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(LoadDefaultControllerMaps)}(): Loading default keyboard maps for player {player.id} maps.");
                
                //var json = File.ReadAllText(RewiredConstants.ActionSlotsDefaultKeyboardMapFile);
                //var keyboardMaps = new List<string>() { json };
                //player.controllers.maps.AddMapsFromJson(ControllerType.Keyboard, 0, keyboardMaps);
                
                var xml = File.ReadAllText(RewiredConstants.ActionSlots.DefaultKeyboardMapFile);
                var keyboardMap = ControllerMap.CreateFromXml(ControllerType.Keyboard, xml);
                player.controllers.maps.AddMap<KeyboardMap>(0, keyboardMap);
                _controllerMaps.Add(player.id, keyboardMap);
            }
        }
    }
}
