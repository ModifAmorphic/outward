using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionUI.Services;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModifAmorphic.Outward.ActionUI.Monobehaviours
{
    internal class ActionSlotDropper : MonoBehaviour, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {
        ItemDisplay _draggedItemDisplay;
        ActionSlot _actionSlot;

        SlotDataService _slotDataService;

        Func<IModifLogger> _getLogger;
        private IModifLogger Logger => _getLogger?.Invoke() ?? NullLogger.Instance;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _actionSlot = GetComponentInParent<ActionSlot>();
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(Awake)}: ActionSlot SlotId: {_actionSlot.SlotId}");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(Start)}: ActionSlot SlotId: {_actionSlot.SlotId}");
        }

        public void SetLogger(Func<IModifLogger> getLogger) => _getLogger = getLogger;
        public void OnDrop(PointerEventData eventData)
        {
            //_actionSlot.Controller.AssignSlotAction()
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnDrop)}: ActionSlot SlotId: {_actionSlot.SlotId}, ItemDisplay: {_draggedItemDisplay?.name}, RefItem: {_draggedItemDisplay?.RefItem?.name}");
            if (_draggedItemDisplay?.RefItem != null && _draggedItemDisplay.RefItem.IsQuickSlotable)
            {
                var slotAction = GetSlotDataService().GetSlotAction(_draggedItemDisplay.RefItem);
                Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnDrop)}: ActionSlot SlotId: {_actionSlot.SlotId}. Got SlotAction for RefItem {_draggedItemDisplay?.RefItem?.name}");
                _actionSlot.Controller.AssignSlotAction(slotAction);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _draggedItemDisplay = GetDraggedElement(eventData);
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnPointerEnter)}: ActionSlot SlotId: {_actionSlot.SlotId}, ItemDisplay: {_draggedItemDisplay?.name}");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(OnPointerExit)}: ActionSlot SlotId: {_actionSlot.SlotId}");
            _draggedItemDisplay = null;
        }
        private ItemDisplay GetDraggedElement(PointerEventData eventData)
        {
            var pointerDrag = eventData.pointerDrag;
            Logger.LogDebug($"{nameof(ActionSlotDropper)}:{nameof(GetDraggedElement)}: ActionSlot SlotId: {_actionSlot.SlotId}. pointerDrag: {eventData.pointerDrag?.name}");
            if (pointerDrag == null)
                return null;

            return pointerDrag.GetComponent<ItemDisplay>();
        }

        private SlotDataService GetSlotDataService()
        {
            if (_slotDataService != null)
                return _slotDataService;

            var playerId = _actionSlot.HotbarsContainer.PlayerActionMenus.PlayerID;
            var psp = Psp.Instance.GetServicesProvider(playerId);
            _slotDataService = psp.GetService<SlotDataService>();

            return _slotDataService;
        }
    }
}
