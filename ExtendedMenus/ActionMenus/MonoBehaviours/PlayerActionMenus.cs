using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class PlayerActionMenus : MonoBehaviour
    {
        private int _playerID = -1;
        public int PlayerID { get => _playerID; }

        public MainSettingsMenu MainSettingsMenu;
        //public SettingsView SettingsView;
        //public HotkeyCaptureMenu HotkeyCaptureMenu;
        //public HotbarSettingsView HotbarSettingsViewer;
        public ActionsViewer ActionsViewer;

        private UnityServicesProvider _servicesProvider;
        public UnityServicesProvider ServicesProvider => _servicesProvider;

        private ProfileManager _profileManager;
        public ProfileManager ProfileManager => _profileManager;

        private IActionMenu[] _actionMenus;

        private Func<bool> _exitRequested;

        private bool _isAwake = false;

        public void SetIDs(int playerID)
        {
            (_playerID) = (playerID);
            _servicesProvider = Psp.Instance.GetServicesProvider(playerID);
            _profileManager = new ProfileManager(playerID);
            var positionables = GetPositionableUIs();
            foreach (var ui in positionables)
                ui.SetProfileManager(_profileManager);
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
                Debug.Log($"_actionMenus[{i}] == null == {_actionMenus[i] == null}. _actionMenus[{i}].OnShow == null == {_actionMenus[i]?.OnShow == null}");
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

            if (_exitRequested != null && _exitRequested.Invoke())
            {
                for (int i = 0; i < _actionMenus.Length; i++)
                {
                    if (_actionMenus[i].IsShowing)
                        _actionMenus[i].Hide();
                }
            }
        }

        public void ConfigureExit(Func<bool> exitRequested) => _exitRequested = exitRequested;
        
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
