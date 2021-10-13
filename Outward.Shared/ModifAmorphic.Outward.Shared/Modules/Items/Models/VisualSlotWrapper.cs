using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Items.Models
{
    internal class VisualSlotWrapper
    {
        private readonly VisualSlot _visualSlot;
        private VisualSlotWrapper(VisualSlot visualSlot) => _visualSlot = visualSlot;

        public static VisualSlotWrapper Wrap(VisualSlot visualSlot) => new VisualSlotWrapper(visualSlot);

        #region Private Fields Reflected as Properties
        public Item m_currentItem
        {
            get => _visualSlot.GetPrivateField<VisualSlot, Item>("m_currentItem");
            set => _visualSlot.SetPrivateField<VisualSlot, Item>("m_currentItem", value);
        }
        
        public ItemVisual m_editorCurrentVisuals
        {
            get => _visualSlot.GetPrivateField<VisualSlot, ItemVisual>("m_editorCurrentVisuals");
            set => _visualSlot.SetPrivateField<VisualSlot, ItemVisual>("m_editorCurrentVisuals", value);
        }

        public ItemVisual m_currentVisual
        {
            get => _visualSlot.GetPrivateField<VisualSlot, ItemVisual>("m_currentVisual");
            set => _visualSlot.SetPrivateField<VisualSlot, ItemVisual>("m_currentVisual", value);
        }
        //
        #endregion
        public Action PositionVisuals => () => _visualSlot.InvokePrivateMethod("PositionVisuals");
    }
}
