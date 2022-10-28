using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public class PositionsService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly ModifCoroutine _coroutine;
        private readonly GameObject _modInactivableGo;

        private static readonly HashSet<string> _positionBlocklist = new HashSet<string>()
        {
            "CorruptionSmog", "PanicOverlay", "TargetingFlare", "CharacterBars", "LowHealth", "LowStamina", "Chat - Panel"
        };

        public PositionsService(
                                ModifCoroutine coroutine,
                                ModifGoService modifGoService,
                                Func<IModifLogger> getLogger)
        {
            _coroutine = coroutine;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            _getLogger = getLogger;
        }

        public void InjectPositionableUIs(CharacterUI characterUI)
        {
            var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
            //hud.GetOrAddComponent<GraphicRaycaster>();
            var positionablePrefab = _modInactivableGo.transform.Find("Prefabs/PositionableBg").gameObject;
            for (int i = 0; i < hud.childCount; i++)
            {
                if (hud.GetChild(i) is RectTransform uiRect)
                {
                    if (uiRect.name == "Tutorialization_DropBag" || uiRect.name == "Tutorialization_UseBandage")
                        uiRect = uiRect.Find("Panel") as RectTransform;

                    if (uiRect.name == "QuickSlot")
                        uiRect = uiRect.Find("Keyboard") as RectTransform;

                    if (!_positionBlocklist.Contains(uiRect.name))
                        AddPositionableUI(uiRect, positionablePrefab);
                }
            }
        }

        public void DestroyPositionableUIs(CharacterUI characterUI)
        {
            var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
            //hud.GetOrAddComponent<GraphicRaycaster>();
            var positionables = hud.GetComponentsInChildren<PositionableUI>();
            for (int i = 0; i < positionables.Length; i++)
            {
                UnityEngine.Object.Destroy(positionables[i]);
            }
        }

        private static void AddPositionableUI(RectTransform transform, GameObject positionablePrefab)
        {
            var bg = UnityEngine.Object.Instantiate(positionablePrefab, transform);
            bg.name = positionablePrefab.name;
            bg.transform.SetAsLastSibling();
            var positionableUI = transform.gameObject.AddComponent<PositionableUI>();
            positionableUI.BackgroundImage = bg.GetComponent<Image>();
            positionableUI.ResetButton = bg.GetComponentInChildren<Button>();
            bg.GetComponentInChildren<Text>().text = transform.name;
        }

        public void ToggleQuickslotsPositonable(ActionUIProfile profile, PlayerActionMenus actionMenus, CharacterUI characterUI)
        {
            if (!profile.ActionSlotsEnabled)
            {
                var hotbars = actionMenus.gameObject.GetComponentInChildren<HotbarsContainer>();
                hotbars.gameObject.Destroy();
            }
            else
            {
                var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
                var quickSlot = hud.Find("QuickSlot/Keyboard").gameObject;
                var quickSlotPositonable = quickSlot.GetComponent<PositionableUI>();
                if (quickSlotPositonable != null)
                {
                    UnityEngine.Object.Destroy(quickSlotPositonable);
                    quickSlot.transform.Find("PositionableBg").gameObject.Destroy();
                }

                var positonableGo = quickSlot.transform.Find("PositionableBg");
                if (positonableGo != null)
                {
                    positonableGo.gameObject.Destroy();
                }
            }
        }

        private bool _isKeepPositionablesListening = false;
        public void StartKeepPostionablesVisible(PlayerActionMenus actionMenus, CharacterUI characterUI)
        {
            if (_isKeepPositionablesListening)
                return;

            try
            {
                var uiPositionScreen = actionMenus.GetComponentInChildren<UIPositionScreen>(true);
                var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
                var positonables = hud.GetComponentsInChildren<PositionableUI>(true).Where(p => p != null);
                uiPositionScreen.OnShow.AddListener(() => _coroutine.StartRoutine(KeepPositionablesVisible(positonables.ToArray())));
                _isKeepPositionablesListening = true;
            }
            catch (Exception ex)
            {
                Logger.LogException("Failed to start Keep Postionables Visible coroutine", ex);
            }
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
                    //Skip activating quickslot bar. If it's inactive, then keep it that way.
                    if (positionables[i].name == "Keyboard")
                        continue;

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
    }
}
