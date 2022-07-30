using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Settings
{
    internal class HotbarSettings
    {
        public event Action<int> HotbarsChanged;
        private int _hotbars;
        public int Hotbars
        {
            get => _hotbars;
            set
            {
                var oldValue = _hotbars;
                _hotbars = value;
                if (oldValue != _hotbars)
                    HotbarsChanged?.Invoke(_hotbars);
            }
        }

        public event Action<int> ActionSlotsChanged;
        private int _actionSlots;
        public int ActionSlots
        {
            get => _actionSlots;
            set
            {
                var oldValue = _actionSlots;
                _actionSlots = value;
                if (oldValue != _actionSlots)
                    ActionSlotsChanged?.Invoke(_actionSlots);
            }
        }
    }
}
