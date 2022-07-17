using BepInEx;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
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

        private readonly GameObject _menusAsset;
        private readonly Func<Hotbars, CharacterUI, HotbarService> _hotbarServiceFac;
        private readonly ModifCoroutine _coroutine;
        private readonly GameObject _modInactivableGo;
        private GameObject _overhaulMenusGo;
        public readonly Dictionary<int, PlayerActionMenus> _playerMenus = new Dictionary<int, PlayerActionMenus>();

        private GameObject baseHud;

        public PlayerMenuService(BaseUnityPlugin baseUnityPlugin,
                                GameObject menusAsset,
                                Func<Hotbars, CharacterUI, HotbarService> hotbarServiceFac,
                                ModifCoroutine coroutine,
                                ModifGoService modifGoService,
                                HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            _baseUnityPlugin = baseUnityPlugin;
            _menusAsset = menusAsset;
            _hotbarServiceFac = hotbarServiceFac;
            _coroutine = coroutine;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            //_hotbars = hotbars;
            _settings = settings;
            _getLogger = getLogger;

            GetOrAddMenuGameObject();

            SplitPlayerPatches.InitAfter += InjectMenus;
            SplitPlayerPatches.SetCharacterAfter += SetPlayerMenuCharacter;
            SplitScreenManagerPatches.RemoveLocalPlayerAfter += RemovePlayerMenu;
            //SplitScreenManagerPatches.AwakeAfter += InjectMenus;
            //MenuManagerPatches.AwakeBefore += MenuManagerPatches_AwakeBefore;
            //settings.HotbarsChanged += (bars) => ConfigureSlots();
            //settings.ActionSlotsChanged += (slots) => ConfigureSlots();
            //WaitAttachAsset();
            //ConfigureSlots();
        }

        private void SetPlayerMenuCharacter(SplitPlayer splitPlayer, Character character)
        {
            var actionMenus = _playerMenus[splitPlayer.RewiredID].PlayerMenu.gameObject;
            actionMenus.name = character.name + "_UIX";
            actionMenus.SetActive(true);
            _playerMenus[splitPlayer.RewiredID].HotbarService = _hotbarServiceFac(actionMenus.GetComponentInChildren<Hotbars>(), character.CharacterUI);
        }

        private void RemovePlayerMenu(SplitScreenManager splitScreenManager, SplitPlayer player, string playerId)
        {
            if (_playerMenus.TryGetValue(player.RewiredID, out var playerMenu))
            {
                UnityEngine.Object.Destroy(playerMenu.PlayerMenu.gameObject);
                _playerMenus.Remove(player.RewiredID);
            }
        }

        private GameObject GetOrAddMenuGameObject()
        {
            if (_overhaulMenusGo == null)
            {
                _overhaulMenusGo = new GameObject(MenuOverhaulSettings.GameObjectName);
                UnityEngine.Object.DontDestroyOnLoad(_overhaulMenusGo);
            }
            return _overhaulMenusGo;
        }

        private void InjectMenus(SplitPlayer splitPlayer)
        {
            if (!_playerMenus.ContainsKey(splitPlayer.RewiredID))
            {
                var playerMenuPrefab = _menusAsset.GetComponentInChildren<PlayerMenu>().gameObject;
                var playerMenuGo = GameObject.Instantiate(playerMenuPrefab);
                playerMenuGo.SetActive(false);
                _playerMenus.Add(splitPlayer.RewiredID, new PlayerActionMenus() { PlayerMenu = playerMenuGo.GetComponent<PlayerMenu>() });
                _playerMenus[splitPlayer.RewiredID].PlayerMenu.SetIDs(splitPlayer.RewiredID, splitPlayer.PlayerUID);
                playerMenuGo.transform.SetParent(_overhaulMenusGo.transform);
                UnityEngine.Object.DontDestroyOnLoad(playerMenuGo);
            }

            //if (hud.Find())
            //_modInactivableGo
        }
    }
}
