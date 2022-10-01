using ModifAmorphic.Outward.ActionUI.DataModels;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class PositionsService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        //private readonly PlayerActionMenus _actionMenus;
        private readonly ModifCoroutine _coroutine;
        //private readonly CharacterUI _characterUI;
        private readonly GameObject _modInactivableGo;
        //private readonly ProfileManager _profileManager;

        private static readonly HashSet<string> _positionBlocklist = new HashSet<string>()
        {
            "CorruptionSmog", "PanicOverlay", "TargetingFlare", "CharacterBars", "LowHealth", "LowStamina", "Chat - Panel"
        };

        public PositionsService(
                                //PlayerActionMenus playerActionMenus,
                                ModifCoroutine coroutine,
                                //CharacterUI characterUI,
                                ModifGoService modifGoService,
                                //ProfileManager profileManager,
                                Func<IModifLogger> getLogger)
        {
            //_actionMenus = playerActionMenus;
            _coroutine = coroutine;
            //_characterUI = characterUI;
            _modInactivableGo = modifGoService.GetModResources(ModInfo.ModId, false);
            //_profileManager = profileManager;
            _getLogger = getLogger;
        }

        public void InjectPositionableUIs(CharacterUI characterUI)
        {
            var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
            //hud.GetOrAddComponent<GraphicRaycaster>();
            var positionablePrefab = _modInactivableGo.transform.Find("PositionableBg").gameObject;
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

        private void ToggleQuickslotPositonable(CharacterUI characterUI, PositionsProfile positionsProfile, bool enabled)
        {

            var keyboard = characterUI.transform.Find("Canvas/GameplayPanels/HUD/QuickSlot/Keyboard");
            var positionable = keyboard.GetComponent<PositionableUI>();
            if (enabled)
            {
                var slotsGroup = keyboard.GetComponent<HorizontalLayoutGroup>();
                var slots = slotsGroup.GetComponentsInChildren<EditorQuickSlotDisplayPlacer>();
                var slotRect = slots.First().GetComponent<RectTransform>().rect;
                var width = (slotRect.width + slotsGroup.spacing) * 8 + slotsGroup.padding.horizontal * 2 - slotsGroup.spacing;
                var height = slotRect.height + slotsGroup.padding.horizontal * 2;
                Logger.LogDebug($"{nameof(PlayerMenuService)}::{nameof(ToggleQuickslotPositonable)}(): {slots.Length} QuickSlots found. Setting PositionableBg rect dimensions to ({width}, {height}).  Slot size ({slotRect.width}, {slotRect.height})");
                var rectTranform = positionable.BackgroundImage.GetComponent<RectTransform>();
                rectTranform.anchorMin = new Vector2(1f, 0f);
                rectTranform.anchorMax = new Vector2(1f, 1f);
                rectTranform.pivot = new Vector2(1f, .5f);
                rectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * 1.2f);

                positionable.SetPositionFromProfile(positionsProfile);
            }

            positionable.enabled = enabled;
        }

        private bool _isKeepPositionablesListening = false;
        public void StartKeepPostionablesVisible(PlayerActionMenus actionMenus, CharacterUI characterUI)
        {
            if (_isKeepPositionablesListening)
                return;

            var uiPositionScreen = actionMenus.GetComponentInChildren<UIPositionScreen>();
            var hud = characterUI.transform.Find("Canvas/GameplayPanels/HUD");
            var positonables = hud.GetComponentsInChildren<PositionableUI>(true).Where(p => p != null);
            uiPositionScreen.OnShow.AddListener(() => _coroutine.StartRoutine(KeepPositionablesVisible(positonables.ToArray())));

            _isKeepPositionablesListening = true;
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
