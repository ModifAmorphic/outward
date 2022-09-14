using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Extensions
{
    public static class ButtonExtensions
    {
        private static readonly PropertyInfo _selectionStatePropInfo = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public enum SelectionState
        {
            /// <summary>
            /// The UI object can be selected.
            /// </summary>
            Normal,

            /// <summary>
            /// The UI object is highlighted.
            /// </summary>
            Highlighted,

            /// <summary>
            /// The UI object is pressed.
            /// </summary>
            Pressed,

            /// <summary>
            /// The UI object is selected
            /// </summary>
            Selected,

            /// <summary>
            /// The UI object cannot be selected.
            /// </summary>
            Disabled,
        }
        
        public static SelectionState GetSelectionState(this Button button) => (SelectionState)_selectionStatePropInfo.GetValue(button);
    }
}
