using ModifAmorphic.Outward.Logging;
using UnityEngine;

namespace ModifAmorphic.Outward.ExtraSlots.Display
{
    internal class QuickSlotPanelDisplayMover
    {
        private readonly IModifLogger _logger;
        private readonly QuickSlotPanel _quickSlotPanel;
        public QuickSlotPanelDisplayMover(QuickSlotPanel quickSlotPanel, IModifLogger logger)
        {
            this._quickSlotPanel = quickSlotPanel;
            this._logger = logger;
        }
        public (Vector3 originalPos, Vector3 centeredPos) CenterHorizontally(float xOffset = 0, float yOffset = 0)
        {
            var quickSlotDisplays = _quickSlotPanel.transform.GetComponentsInChildren<QuickSlotDisplay>();
            var slotWidth = quickSlotDisplays[1].transform.position.x - quickSlotDisplays[0].transform.position.x;
            var panelHalfWidth = slotWidth * quickSlotDisplays.Length / 2f;
            var screenCenterX = Screen.width / 2f;
            var relXOffset = quickSlotDisplays[0].transform.position.x - _quickSlotPanel.transform.position.x;
            var panelX = (screenCenterX - panelHalfWidth) - relXOffset + slotWidth / 2f + xOffset;

            _logger.LogDebug($"{nameof(TransformMover)}.{nameof(CenterHorizontally)}(): {_quickSlotPanel.name}: slotWidth={slotWidth}; panelHalfWidth={panelHalfWidth}; screenCenterX={screenCenterX}; panelX={panelX}; xOffset={xOffset};\n\tquickSlotDisplays[0].transform.position.x={quickSlotDisplays[0].transform.position.x};\n\tquickSlotPanel.transform.position.x={_quickSlotPanel.transform.position.x}");
            var originalPos = new Vector3(_quickSlotPanel.transform.position.x, _quickSlotPanel.transform.position.y, _quickSlotPanel.transform.position.z);
            _quickSlotPanel.transform.position = new Vector3(panelX, _quickSlotPanel.transform.position.y, _quickSlotPanel.transform.position.z);

            return (originalPos, _quickSlotPanel.transform.position);
        }
    }
}
