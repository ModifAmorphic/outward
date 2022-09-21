using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionUI.Extensions
{
    public static class DropdownExtensions
    {
        private static readonly Dropdown.DropdownEvent noDropdownEvent = new Dropdown.DropdownEvent();
        /// <summary>
        /// Sets the Toggle's isOn property without triggering the onValueChanged UnityEvent.
        /// </summary>
        public static void SetValue(this Dropdown dropdown, int value)
        {
            var onValueChanged = dropdown.onValueChanged;
            dropdown.onValueChanged = noDropdownEvent;
            dropdown.value = value;
            dropdown.onValueChanged = onValueChanged;
        }

        public static void ClearOptionsSilent(this Dropdown dropdown)
        {
            var onValueChanged = dropdown.onValueChanged;
            dropdown.onValueChanged = noDropdownEvent;
            dropdown.ClearOptions();
            dropdown.onValueChanged = onValueChanged;
        }

        public static void AddOptionSilent(this Dropdown dropdown, List<Dropdown.OptionData> options)
        {
            var onValueChanged = dropdown.onValueChanged;
            dropdown.onValueChanged = noDropdownEvent;
            dropdown.AddOptions(options.ToList());
            dropdown.onValueChanged = onValueChanged;
        }
    }
}
