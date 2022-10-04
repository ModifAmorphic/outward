using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class UIPositionScreen : MonoBehaviour, ISettingsView
    {
        public MainSettingsMenu MainSettingsMenu;
        public Image BackPanel;

        public Text ExitScreenText;

        private bool _isInit;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        public bool IsShowing => gameObject.activeSelf && _isInit;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            DebugLogger.Log("UIPositionScreen::Awake");
            Hide(false);
            _isInit = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            DebugLogger.Log("UIPositionScreen::Start");
        }


        public void Show()
        {
            gameObject.SetActive(true);
            BackPanel.gameObject.SetActive(true);
            ExitScreenText.gameObject.SetActive(true);
            EnableUIPositioning();
            OnShow?.TryInvoke();
        }

        public void Hide() => Hide(true);


        private void Hide(bool raiseEvent)
        {
            DebugLogger.Log("UIPositionScreen::Hide");
            //if (!IsShowing)
            //    return;

            gameObject.SetActive(false);
            BackPanel.gameObject.SetActive(false);
            ExitScreenText.gameObject.SetActive(false);

            if (_isInit)
                DisableAndSavePositions();

            if (raiseEvent)
            {
                OnHide?.TryInvoke();
            }
        }
        private void EnableUIPositioning()
        {
            var uis = MainSettingsMenu.PlayerMenu.GetPositionableUIs();

            foreach (var ui in uis)
                ui.EnableMovement();
        }
        private void DisableAndSavePositions()
        {
            var uis = MainSettingsMenu.PlayerMenu.GetPositionableUIs();
            var positonService = MainSettingsMenu.PlayerMenu.ProfileManager.PositionsProfileService;
            var positions = positonService.GetProfile();
            bool saveNeeded = false;
            DebugLogger.Log($"Checking {uis.Length} UI Element positons for changes.");
            foreach (var ui in uis)
            {
                ui.DisableMovement();
                var uiPositons = ui.GetUIPositions();
                if (ui.HasMoved)
                {
                    positions.AddOrReplacePosition(ui.GetUIPositions());
                    DebugLogger.Log($"Found position change for UI element '{ui.TransformPath}.");
                    saveNeeded = true;
                }
            }

            if (saveNeeded)
                positonService.Save();
        }
    }
}