using ModifAmorphic.Outward.Unity.ActionUI;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ActionItemView : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {

        public ActionImages ActionImages;
        public MouseClickListener MouseClickListener;
        public UnityEvent<int> OnSlotActionSet;
        public UnityEvent<int> OnSlotActionReset;

        private ISlotAction _slotAction;
        public ISlotAction SlotAction => _slotAction;

        private int _viewID = -1;
        public int ViewID => _viewID;

        private Button _button;
        public Button Button => _button;

        private Text _text;
        private Text _stackText;

        //Border around the ActionSlot
        private Image _borderImage;
        public Image BorderImage => _borderImage;

        private Image _borderHighlightImage;
        public Image ActionBorderHighlight => _borderHighlightImage;

        private bool _isAwake;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _button = GetComponentInChildren<Button>(true);
            _text = GetComponentsInChildren<Text>(true).First(t => t.name.Equals("ActionText"));
            _stackText = _button.GetComponentsInChildren<Text>(true).First(t => t.name.Equals("StackText"));
            _borderImage = GetComponentsInChildren<Image>(true).First(i => i.name == "ActionBorder");
            _borderHighlightImage = GetComponentsInChildren<Image>(true).First(i => i.name == "ActionBorderHighlight");
            _borderHighlightImage.enabled = false;
            _borderImage.enabled = true;
            MouseClickListener.OnRightClick.AddListener(ResetViewItem);
            
            //if (OnSlotActionSet == null)
            //    OnSlotActionSet = new UnityEvent<int>();
            //if (OnSlotActionReset == null)
            //    OnSlotActionReset = new UnityEvent<int>();

            _isAwake = true;
        }

        public void SetViewID(int viewID) => _viewID = viewID;

        public void SetViewItem(ISlotAction action)
        {
            if (_slotAction != null)
            {
                DebugLogger.Log($"SetViewItem: ClearImages()");
                _slotAction.OnIconsChanged -= UpdateActionIcons;
                ActionImages.ClearImages();
            }

            _slotAction = action;
            //_button.image.sprite = action.ActionIcons[0];
            for (int i = 0; i < _slotAction.ActionIcons.Length; i++)
            {
                DebugLogger.Log($"SetViewItem: AddOrUpdateImage({i})");
                ActionImages.AddOrUpdateImage(_slotAction.ActionIcons[i]);
            }
            DebugLogger.Log($"SetViewItem: Setting action text.");
            _text.text = action.DisplayName;
            _stackText.text = action.Stack != null && action.Stack.IsStackable && action.Stack.GetAmount() > 0 ? action.Stack.GetAmount().ToString() : string.Empty;
            if (action.HasDynamicIcon)
                action.OnIconsChanged += UpdateActionIcons;

            DebugLogger.Log($"SetViewItem: Raising OnSlotActionSet for ViewID {ViewID}.");
            OnSlotActionSet.Invoke(ViewID);
        }


        public void ResetViewItem()
        {
            if (_slotAction != null)
            {
                _slotAction.OnIconsChanged -= UpdateActionIcons;
            }

            ActionImages?.ClearImages();
            _slotAction = null;
            _text.text = string.Empty;
            _stackText.text = string.Empty;

            DebugLogger.Log($"SetViewItem: Raising OnSlotActionReset for ViewID {ViewID}.");
            OnSlotActionReset.Invoke(ViewID);
        }

        private void UpdateActionIcons(ActionSlotIcon[] icons)
        {
            ActionImages.ClearImages();

            for (int i = 0; i < _slotAction.ActionIcons.Length; i++)
            {
                ActionImages.AddOrUpdateImage(_slotAction.ActionIcons[i]);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isAwake)
            {
                _borderHighlightImage.enabled = true;
                _borderImage.enabled = false;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isAwake)
            {
                _borderHighlightImage.enabled = false;
                _borderImage.enabled = true;
            }
        }

    }
}