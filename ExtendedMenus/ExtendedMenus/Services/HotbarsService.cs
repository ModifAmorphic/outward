using BepInEx;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus
{
    internal class HotbarsService
    {
        private readonly HotbarSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly Hotbars _hotbars;

        private readonly GameObject _hotbarsAsset;
        private readonly ModifCoroutine _coroutine;
        private readonly GameObject _modInactivableGo;

        private GameObject baseHud;

        public HotbarsService(BaseUnityPlugin baseUnityPlugin,
                                GameObject hotbarsAsset,
                                ModifCoroutine coroutine,
                                ModifGoService modifGoService,
                                HotbarSettings settings, Func<IModifLogger> getLogger)
        {
            _baseUnityPlugin = baseUnityPlugin;
            _hotbarsAsset = hotbarsAsset;
            _coroutine = coroutine;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            //_hotbars = hotbars;
            _settings = settings;
            _getLogger = getLogger;

            SplitScreenManagerPatches.AwakeAfter += InjectMenus;
            //MenuManagerPatches.AwakeBefore += MenuManagerPatches_AwakeBefore;
            settings.HotbarsChanged += (bars) => ConfigureSlots();
            settings.ActionSlotsChanged += (slots) => ConfigureSlots();
            //WaitAttachAsset();
            //ConfigureSlots();
        }

        private void InjectMenus(SplitScreenManager splitScreenManager, ref CharacterUI characterUI)
        {
            var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
            var hotbars = GameObject.Instantiate(_hotbarsAsset);
            hotbars.transform.SetParent(hud);
            hotbars.SetActive(true);

            //if (hud.Find())
            //_modInactivableGo
        }

        private void WaitAttachAsset()
        {
            bool startWhen() => FindBaseHud() != null;

            _coroutine.StartRoutine(
                _coroutine.InvokeAfter(startWhen, AttachAsset, 86400)
                );
        }
        private GameObject FindBaseHud()
        {
            if (baseHud != null)
                return baseHud;

            var hudChilds = Resources.FindObjectsOfTypeAll<LowStaminaListener>();
            for (int i = 0; i < hudChilds.Length; i++)
            {
                if (hudChilds[i].transform.parent.gameObject.GetPath() == "/PlayerUI/Canvas/GameplayPanels/HUD")
                {
                    baseHud = hudChilds[i].transform.parent.parent.parent.parent.gameObject;
                    return baseHud;
                }
            }
            return null;
        }
        private void AttachAsset()
        {
            var hud = FindBaseHud();
            Logger.LogDebug("Attaching Hotbar Asset to HUD " + hud.name);
            var hotbar = UnityEngine.Object.Instantiate(_hotbarsAsset);
            hotbar.transform.parent = hud.transform;
        }
        private void MenuManagerPatches_AwakeBefore(MenuManager menuManager)
        {
            //var parent = menuManager.transform.Find("/CharacterUIs/PlayerUI(Clone)/Canvas/GameplayPanels/HUD");
            //GameObject.Instantiate(_hotbarsAsset, parent);
            //throw new NotImplementedException();
        }

        public void ConfigureSlots()
        {
            _hotbars?.ConfigureHotbars(Hotbars.HotbarType.Grid, _settings.Hotbars, _settings.ActionSlots);
        }
    }
}
