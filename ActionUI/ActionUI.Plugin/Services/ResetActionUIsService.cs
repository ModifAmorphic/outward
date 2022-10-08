using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using System;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class ResetActionUIsService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly ServicesProvider _services;
        private readonly LevelCoroutines _coroutine;


        public ResetActionUIsService(
                                ServicesProvider services,
                                LevelCoroutines coroutine,
                                Func<IModifLogger> getLogger)
        {
            _services = services;
            _coroutine = coroutine;
            _getLogger = getLogger;

            CharacterUIPatches.BeforeReleaseUI += ResetUIs;
            LobbySystemPatches.BeforeClearPlayerSystems += ResetAllPlayerUIs;
        }

        private void ResetAllPlayerUIs(LobbySystem lobbySystem)
        {
            var players = lobbySystem.PlayersInLobby.FindAll(p => p.IsLocalPlayer);
            foreach (var p in players)
            {
                ResetUIs(p.ControlledCharacter.CharacterUI, p.PlayerID);
            }
        }

        private void SaveProfiles(CharacterUI characterUI, int rewiredId)
        {
            var charUID = characterUI.TargetCharacter.UID;
            Logger.LogInfo($"Saving active profile data for character {charUID}");

            if (!Psp.Instance.TryGetServicesProvider(rewiredId, out var usp))
            {
                Logger.LogWarning($"Could not find profile data to save for character {charUID}. No profiles saved.");
                return;
            }

            if (usp.TryGetService<IPositionsProfileService>(out var posService))
                SaveProfile((PositionsProfileJsonService)posService, charUID);
            else
                Logger.LogWarning($"Could not find Positions Profile to save for character {charUID}. Profile will not be saved.");

            if (usp.TryGetService<IHotbarProfileService>(out var hotbarService))
                SaveProfile((HotbarProfileJsonService)hotbarService, charUID);
            else
                Logger.LogWarning($"Could not find Hotbars Profile to save for character {charUID}. Profile will not be saved.");

            if (usp.TryGetService<ControllerMapService>(out var controllerService))
                SaveProfile(controllerService, charUID);
            else
                Logger.LogWarning($"Could not find Rewired Map Profile to save for character {charUID}. Profile will not be saved.");

            if (usp.TryGetService<IEquipmentSetService<ArmorSet>>(out var armorService))
                SaveProfile((ArmorSetsJsonService)armorService, charUID);
            else
                Logger.LogWarning($"Could not find Armor Sets Profile to save for character {charUID}. Profile will not be saved.");

            if (usp.TryGetService<IEquipmentSetService<WeaponSet>>(out var weaponService))
                SaveProfile((WeaponSetsJsonService)weaponService, charUID);
            else
                Logger.LogWarning($"Could not find Weapon Sets Profile to save for character {charUID}. Profile will not be saved.");

            if (usp.TryGetService<IActionUIProfileService>(out var profileService))
                SaveProfile((ProfileService)profileService, charUID);
            else
                Logger.LogWarning($"Could not find Action UI Profile to save for character {charUID}. Profile will not be saved.");

            if (_services.TryGetService<GlobalProfileService>(out var globalService))
                SaveProfile(globalService, charUID);
            else
                Logger.LogWarning($"Could not find Global Profile to save for character {charUID}. Profile will not be saved.");
        }

        private void SaveProfile(ISavableProfile profileService, string characterUID)
        {
            try
            {
                profileService.Save();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to save profile {profileService.GetType()} for character {characterUID}.", ex);
            }
        }

        private void ResetUIs(CharacterUI characterUI, int rewiredId)
        {
            try
            {
                SaveProfiles(characterUI, rewiredId);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Unexpected error occured when saving profiles for character {characterUI.TargetCharacter.UID}.", ex);
            }

            Logger.LogDebug($"Destroying Action UIs for player {rewiredId}");
            if (Psp.Instance.TryGetServicesProvider(rewiredId, out var usp) && usp.TryGetService<PositionsService>(out var posService))
            {
                try
                {
                    posService.DestroyPositionableUIs(characterUI);
                }
                catch (Exception ex)
                {
                    Logger.LogException("Dispose of PositionableUIs failed.", ex);
                }
            }

            Psp.Instance.TryDisposeServicesProvider(rewiredId);
            CharacterUIPatches.GetIsMenuFocused.TryRemove(rewiredId, out _);
            
            try
            {
                characterUI.GetComponentInChildren<EquipmentSetMenu>(true).gameObject.Destroy();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Dispose of {nameof(EquipmentSetMenu)} gameobject failed.", ex);
            }
            
            try
            {
                characterUI.GetComponentInChildren<PlayerActionMenus>(true).gameObject.Destroy();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Dispose of {nameof(PlayerActionMenus)} gameobject failed.", ex);
            }
        }
    }
}
