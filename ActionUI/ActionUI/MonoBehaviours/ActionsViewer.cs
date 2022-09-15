using ModifAmorphic.Outward.ActionMenus.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ActionsViewer : MonoBehaviour, IActionMenu
    {

        public PlayerActionMenus PlayerMenu;
        public ViewerLeftNav LeftNav;

        private GridLayoutGroup _gridLayout;
        public GameObject BaseGridAction;
        private int _slotId;

        public bool IsShowing => gameObject.activeSelf;

        public delegate void SlotActionSelected(int slotId, ISlotAction slotAction);
        public event SlotActionSelected OnSlotActionSelected;


        public bool HasData => GetViewData() != null;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _gridLayout = GetComponentInChildren<GridLayoutGroup>();
            BaseGridAction.SetActive(false);
            Hide();
        }
        // Start is called before the first frame update
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Start()
        {
            Clear();
        }

        public void Clear()
        {
            var children = GetComponentsInChildren<ActionItemView>();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name != "BaseSlotPanel")
                    children[i].gameObject.Destroy();
            }
        }
        public void LoadData(IEnumerable<ISlotAction> actionViews)
        {
            Clear();
            if (!HasData)
                return;
            foreach (var item in actionViews)
            {
                AddActionView(item);
            }
        }
        public void AddActionView(ISlotAction slotAction)
        {
            AddGridItem(slotAction);
        }
        public void Show(int slotId)
        {
            _slotId = slotId;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            InitLeftNav();
            DoNextFrame(LeftNav.ClickSelectedTab);
            OnShow?.Invoke();
        }
        public void Hide()
        {
            _slotId = 0;
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        private void InitLeftNav()
        {
            LeftNav.ClearViewTabs();
            var viewData = GetViewData();
            if (viewData.GetActionsTabData() != null)
            {
                foreach (var tabView in viewData.GetActionsTabData().OrderBy(d => d.TabOrder))
                {
                    var button = LeftNav.AddViewTab(tabView.DisplayName);
                    button.onClick.AddListener(() => LoadData(tabView.GetSlotActions()));
                }
            }
            LeftNav.AddViewTab("All")
                .onClick.AddListener(() => LoadData(viewData.GetAllActions()));
        }

        private IActionViewData GetViewData() => Psp.Instance.GetServicesProvider(PlayerMenu.PlayerID).GetService<IActionViewData>();

        private void AddGridItem(ISlotAction slotAction)
        {
            var actionItemGo = Instantiate(BaseGridAction, _gridLayout.transform);
            var itemCount = _gridLayout.GetComponentsInChildren<ActionItemView>().Count();
            actionItemGo.SetActive(true);
            var actionItem = actionItemGo.GetComponent<ActionItemView>();
            actionItem.SetViewItem(slotAction);
            actionItem.name = "SlotActionView_" + itemCount;
            actionItem.Button.onClick.AddListener(() => RaiseSelectionAndHide(_slotId, actionItem.SlotAction));
        }

        private void RaiseSelectionAndHide(int slotId, ISlotAction slotAction)
        {
            OnSlotActionSelected?.Invoke(slotId, slotAction);
            Clear();
            DoNextFrame(Hide);
        }

        private void DoNextFrame(Action action) => StartCoroutine(NextFrameCoroutine(action));

        private IEnumerator NextFrameCoroutine(Action action)
        {
            yield return null;
            action.Invoke();
        }
    }
}