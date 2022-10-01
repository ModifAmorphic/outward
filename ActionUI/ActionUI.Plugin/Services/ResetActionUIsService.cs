using BepInEx;
using ModifAmorphic.Outward.ActionUI.Monobehaviours;
using ModifAmorphic.Outward.ActionUI.Patches;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class ResetActionUIsService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly LevelCoroutines _coroutine;


        public ResetActionUIsService(
                                LevelCoroutines coroutine,
                                Func<IModifLogger> getLogger)
        {

            _coroutine = coroutine;
            _getLogger = getLogger;

            CharacterUIPatches.BeforeReleaseUI += ResetUIs;
        }

        private void ResetUIs(CharacterUI characterUI, int rewiredId)
        {
            Logger.LogDebug($"Destroying Action UIs for player {rewiredId}");
            if (Psp.Instance.TryGetServicesProvider(rewiredId, out var usp) && usp.TryGetService<PositionsService>(out var posService))
            {
                posService.DestroyPositionableUIs(characterUI);
            }

            Psp.Instance.TryDisposeServicesProvider(rewiredId);
            CharacterUIPatches.GetIsMenuFocused.TryRemove(rewiredId, out _);
            //var actionUI = characterUI.GetComponentInChildren<PlayerActionMenus>(true).gameObject;
            //actionUI.GetComponentsInChildren<SplitScreenScaler>()
            characterUI.GetComponentInChildren<EquipmentSetMenu>(true).gameObject.Destroy();
            characterUI.GetComponentInChildren<PlayerActionMenus>(true).gameObject.Destroy();
        }
    }
}
