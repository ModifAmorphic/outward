using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModifAmorphic.Outward.ActionUI.Monobehaviours
{
    internal class ActionItemViewDropper : MonoBehaviour, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private ItemDisplay _draggedItemDisplay;
        private ActionItemView _actionSlot;
        [SerializeField]
        private int _playerID;

        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
        private Func<IModifLogger> _getLogger;
        //private IModifLogger Logger => _getLogger?.Invoke() ?? NullLogger.Instance;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _actionSlot = GetComponentInParent<ActionItemView>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start() { }

        public void SetPlayerID(int playerID) => _playerID = playerID;
        public void SetLogger(Func<IModifLogger> getLogger) => _getLogger = getLogger;
        public void OnDrop(PointerEventData eventData)
        {
            //_actionSlot.Controller.AssignSlotAction()
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnDrop)}: ActionItemView {_actionSlot.name}, ItemDisplay: {_draggedItemDisplay?.name}, RefItem: {_draggedItemDisplay?.RefItem?.name}");
            if (_draggedItemDisplay?.RefItem != null && _draggedItemDisplay.RefItem.IsQuickSlotable)
            {
                var slotAction = GetSlotDataService().GetSlotAction(_draggedItemDisplay.RefItem);
                Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnDrop)}: ActionItemView {_actionSlot.name}. Got SlotAction for RefItem {_draggedItemDisplay?.RefItem?.name}");
                _actionSlot.SetViewItem(slotAction);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _draggedItemDisplay = GetDraggedElement(eventData);
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnPointerEnter)}: ActionItemView {_actionSlot.name}, ItemDisplay: {_draggedItemDisplay?.name}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnPointerExit)}: ActionItemView {_actionSlot.name}");
            _draggedItemDisplay = null;
        }
        private ItemDisplay GetDraggedElement(PointerEventData eventData)
        {
            var pointerDrag = eventData.pointerDrag;
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(GetDraggedElement)}: ActionItemView {_actionSlot.name}. pointerDrag: {eventData.pointerDrag?.name}");
            if (pointerDrag == null)
                return null;

            return pointerDrag.GetComponent<ItemDisplay>();
        }

        private SlotDataService GetSlotDataService() => Psp.Instance.GetServicesProvider(_playerID).GetService<SlotDataService>();
    }
}
