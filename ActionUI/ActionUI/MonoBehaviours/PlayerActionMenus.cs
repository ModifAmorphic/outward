using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class PlayerActionMenus : MonoBehaviour
    {
        private int _playerID = -1;
        public int PlayerID { get => _playerID; }

        public MainSettingsMenu MainSettingsMenu;
        public DurabilityDisplay DurabilityDisplay;
        public EquipmentSetMenu EquipmentSetMenus;
        //public SettingsView SettingsView;
        //public HotkeyCaptureMenu HotkeyCaptureMenu;
        //public HotbarSettingsView HotbarSettingsViewer;
        public ActionsViewer ActionsViewer;

        private UnityServicesProvider _servicesProvider;
        public UnityServicesProvider ServicesProvider => _servicesProvider;

        private ProfileManager _profileManager;
        public ProfileManager ProfileManager => _profileManager;

        private IActionMenu[] _actionMenus;

        //private Func<bool> _exitRequested;

        private MenuNavigationActions _navActions;
        public MenuNavigationActions NavActions => _navActions;

        private bool _isAwake = false;
        private bool _isPlayerAssigned = false;
        public bool IsPlayerAssigned => _isPlayerAssigned;

        private static readonly UnityEvent<PlayerActionMenus> _onPlayerIdAssigned = new UnityEvent<PlayerActionMenus>();
        public static UnityEvent<PlayerActionMenus> OnPlayerIdAssigned => _onPlayerIdAssigned;

        public void SetIDs(int playerID)
        {
            (_playerID) = (playerID);
            _servicesProvider = Psp.Instance.GetServicesProvider(playerID);
            _profileManager = new ProfileManager(playerID);
            var positionables = GetPositionableUIs();
            foreach (var ui in positionables)
                ui.SetProfileManager(_profileManager);

            _isPlayerAssigned = true;
            _onPlayerIdAssigned.Invoke(this);
        }

        public PositionableUI[] GetPositionableUIs() =>
            transform.parent.GetComponentsInChildren<PositionableUI>(true);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            //posText = GetComponentsInChildren<Text>().First(t => t.name == "PlayerPosText");
            //rectTransform = GetComponent<RectTransform>();

            //canvasPosText = GetComponentsInChildren<Text>().First(t => t.name == "ActionCanvasPosText");
            //canvasRectTransform = GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
            _actionMenus = GetComponentsInChildren<IActionMenu>(true);

            for (int i = 0; i < _actionMenus.Length; i++)
            {
                DebugLogger.Log($"_actionMenus[{i}] == null == {_actionMenus[i] == null}. _actionMenus[{i}].OnShow == null == {_actionMenus[i]?.OnShow == null}");
                var menuIndex = i;
                _actionMenus[i].OnShow.AddListener(() => OnShowMenu(menuIndex));
                _actionMenus[i].OnHide.AddListener(OnHideMenu);
            }
            _isAwake = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            //posText.text = $"Player Pos: {rectTransform.position.x}, {rectTransform.position.y}. Size {rectTransform.sizeDelta.x}, {rectTransform.sizeDelta.y}";
            //canvasPosText.text = $"ActionCanvas Pos: {canvasRectTransform.position.x}, {canvasRectTransform.position.y}. Size {canvasRectTransform.sizeDelta.x}, {canvasRectTransform.sizeDelta.y}";

            if (_navActions != null && (_navActions.Cancel?.Invoke() ?? false))
            {
                for (int i = 0; i < _actionMenus.Length; i++)
                {
                    if (_actionMenus[i].IsShowing)
                        _actionMenus[i].Hide();
                }
            }
        }

        public void ConfigureNavigation(MenuNavigationActions navActions) => _navActions = navActions;

        public bool AnyActionMenuShowing() => _isAwake && _actionMenus.Any(m => m.IsShowing);

        private void OnShowMenu(int menuIndex)
        {
            for (int i = 0; i < _actionMenus.Length; i++)
            {
                if (i != menuIndex && _actionMenus[i].IsShowing)
                    _actionMenus[i].Hide();
            }
            //BackPanel.SetActive(true);
        }
        private void OnHideMenu()
        {
            //if (!_actionMenus.Any(m => m.IsShowing))
            //BackPanel.SetActive(false);
        }


    }
}
