using System;
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

        public static Dropdown.OptionData GetSelectedOption(this Dropdown dropdown) => dropdown.options[dropdown.value];

        public static int SelectOption(this Dropdown dropdown, string optionText, bool caseSensitive = false)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text.Equals(optionText, StringComparison.InvariantCultureIgnoreCase))
                {
                    dropdown.value = i;
                    return i;
                }
            }

            return -1;
        }
        public static int SelectOptionSilent(this Dropdown dropdown, string optionText, bool caseSensitive = false)
        {
            for (int i = 0; i <  dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text.Equals(optionText, StringComparison.InvariantCultureIgnoreCase))
                {
                    dropdown.SetValueWithoutNotify(i);
                    return i;
                }
            }

            return -1;
        }
    }
}
