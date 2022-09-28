using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public enum EquipmentSetTypes
    {
        Weapon,
        Armor
    }
    [UnityScriptComponent]
    public class EquipmentSetView : MonoBehaviour
    {
        public PlayerActionMenus PlayerMenu;

        public EquipmentSetTypes EquipmentSetType;

        public Dropdown EquipmentSetDropdown;
        public Button NewSetButton;
        public Button RenameSetButton;
        public Button SaveSetButton;
        //public Button LoadSetButton;


        public Dropdown EquipmentIconDropdown;

        public ActionItemView EquipmentIcon;

        public EquipmentSetNameInput SetNamePanel;

        private Dictionary<EquipSlots, string> _equipSlotsNames = new Dictionary<EquipSlots, string>()
        {
            { EquipSlots.Head, "Helm" },
            { EquipSlots.Chest, "Chest" },
            { EquipSlots.Feet, "Boots" },
            { EquipSlots.RightHand, "Main Hand" },
            { EquipSlots.LeftHand, "Left Hand" }
        };

        private Dictionary<string, EquipSlots> _equipSlots = new Dictionary<string, EquipSlots>()
        {
            { "Helm", EquipSlots.Head },
            { "Chest", EquipSlots.Chest },
            { "Boots", EquipSlots.Feet },
            { "Main Hand", EquipSlots.RightHand },
            { "Left Hand", EquipSlots.LeftHand }
        };

        private IEquipmentSetService<T> GetEquipmentService<T>() where T : IEquipmentSet
        {
            if (typeof(T).Equals(typeof(ArmorSet)))
                return (IEquipmentSetService<T>)PlayerMenu.ProfileManager.ArmorSetService;
            else if (typeof(T).Equals(typeof(WeaponSet)))
                return (IEquipmentSetService<T>)PlayerMenu.ProfileManager.WeaponSetService;

            throw new ArgumentOutOfRangeException(nameof(T));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            PlayerMenu.EquipmentSetMenus.OnHide.AddListener(() =>
            {
                if (SetNamePanel.IsShowing)
                    SetNamePanel.Hide();
            });

            EquipmentSetDropdown.ClearOptions();
            EquipmentIconDropdown.ClearOptions();

            NewSetButton.onClick.AddListener(PromptNewSet);
            RenameSetButton.onClick.AddListener(PromptRenameSet);

            if (EquipmentSetType == EquipmentSetTypes.Armor)
            {
                EquipmentSetDropdown.onValueChanged.AddListener(EquipmentSetChanged<ArmorSet>);
                EquipmentIconDropdown.onValueChanged.AddListener((index) => UpdateSetIcon<ArmorSet>());
                SaveSetButton.onClick.AddListener(SaveEquipmentSet<ArmorSet>);
                GetEquipmentService<ArmorSet>().OnNewSet += (set) => SaveNewSet<ArmorSet>(set.Name);
                GetEquipmentService<ArmorSet>().OnRenamedSet += SetRenamed;
            }
            else
            {
                EquipmentSetDropdown.onValueChanged.AddListener(EquipmentSetChanged<WeaponSet>);
                EquipmentIconDropdown.onValueChanged.AddListener((index) => UpdateSetIcon<WeaponSet>());
                SaveSetButton.onClick.AddListener(SaveEquipmentSet<WeaponSet>);
                GetEquipmentService<WeaponSet>().OnNewSet += (set) => SaveNewSet<WeaponSet>(set.Name);
                GetEquipmentService<WeaponSet>().OnRenamedSet += SetRenamed;
            }
        }

        private void SetRenamed<T>(T equipmentSet, string oldName, string newName) where T : IEquipmentSet
        {
            SetEquipmentSetOptions<T>();
            EquipmentSetDropdown.SelectOption(newName, false);
        }


        // Start is called before the first frame update
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Start()
        {
            //Show();
        }

        public void Show()
        {
            gameObject.SetActive(true);

            SetEquipmentIconOptions();
            if (EquipmentSetType == EquipmentSetTypes.Armor)
            {
                SetEquipmentSetOptions<ArmorSet>();
                SelectEquippedSet<ArmorSet>();
                UpdateSetIcon<ArmorSet>();
            }
            else if (EquipmentSetType == EquipmentSetTypes.Weapon)
            {
                SetEquipmentSetOptions<WeaponSet>();
                SelectEquippedSet<WeaponSet>();
                UpdateSetIcon<WeaponSet>();
            }
        }

        public void Hide()
        {
            if (SetNamePanel.IsShowing)
            {
                SetNamePanel.Hide();
                return;
            }
            EquipmentSetDropdown.ClearOptions();
            EquipmentIconDropdown.ClearOptions();
            gameObject.SetActive(false);
        }

        private void SetEquipmentIconOptions()
        {
            EquipmentIconDropdown.ClearOptions();

            if (EquipmentSetType == EquipmentSetTypes.Armor)
            {
                EquipmentIconDropdown.AddOptions(new List<OptionData>()
                {
                    new OptionData(_equipSlotsNames[EquipSlots.Head]),
                    new OptionData(_equipSlotsNames[EquipSlots.Chest]),
                    new OptionData(_equipSlotsNames[EquipSlots.Feet]),
                });
                EquipmentIconDropdown.SetValueWithoutNotify(1);
            }
            else if (EquipmentSetType == EquipmentSetTypes.Weapon)
            {
                EquipmentIconDropdown.AddOptions(new List<OptionData>()
                {
                    new OptionData(_equipSlotsNames[EquipSlots.RightHand]),
                    new OptionData(_equipSlotsNames[EquipSlots.LeftHand]),
                });
                EquipmentIconDropdown.SetValueWithoutNotify(0);
            }
        }

        private void UpdateSetIcon<T>() where T : IEquipmentSet
        {
            DebugLogger.Log($"Getting equipment set {EquipmentSetDropdown.GetSelectedOption().text}");
            T set;
            if (EquipmentSetDropdown.value > 0)
                set = GetEquipmentService<T>().GetEquipmentSet(EquipmentSetDropdown.GetSelectedOption().text);
            else
                set = GetEquipmentService<T>().GetEquippedAsSet("Default");

            DebugLogger.Log($"Got equipment set {set?.Name}");

            //stash the slot icon set it to the dropdown value
            var slotIcon = set.SlotIcon;
            DebugLogger.Log($"Setting SlotIcon to {EquipmentIconDropdown.GetSelectedOption().text}. _equipSlots.ContainsKey(\"{ EquipmentIconDropdown.GetSelectedOption().text}\") == {_equipSlots.ContainsKey(EquipmentIconDropdown.GetSelectedOption().text)}");
            set.SlotIcon = _equipSlots[EquipmentIconDropdown.GetSelectedOption().text];
            EquipmentIcon.SetViewItem(GetEquipmentService<T>().GetSlotActionPreview(set));

            //Set icon back to original
            set.SlotIcon = slotIcon;

            DebugLogger.Log($"Done updating preview icon.");
        }


        private void SetEquipmentSetOptions<T>() where T : IEquipmentSet
        {
            var equipmentSets = GetEquipmentService<T>().GetEquipmentSetsProfile();

            EquipmentSetDropdown.ClearOptions();
            var options = new List<OptionData>()
            {
                new OptionData(string.Empty)
            };
            options.AddRange(equipmentSets.EquipmentSets.OrderBy(e => e.Name).Select(e => new OptionData(e.Name)));
            EquipmentSetDropdown.AddOptions(options);

        }

        private void SelectEquippedSet<T>() where T : IEquipmentSet
        {
            int index;
            for (index = 0; index < EquipmentSetDropdown.options.Count; index++)
            {
                if (GetEquipmentService<T>().IsSetEquipped(EquipmentSetDropdown.options[index].text))
                    break;
            }

            if (index < EquipmentSetDropdown.options.Count)
            {
                EquipmentSetDropdown.SetValueWithoutNotify(index);
                SelectSlotIcon<T>();
            }
            else
            {
                EquipmentSetDropdown.SetValueWithoutNotify(0);
                EquipmentIconDropdown.value = EquipmentSetType == EquipmentSetTypes.Weapon ? 0 : 1;
            }
        }

        private void SelectSlotIcon<T>() where T : IEquipmentSet
        {
            var set = GetEquipmentService<T>().GetEquipmentSet(EquipmentSetDropdown.GetSelectedOption().text);
            EquipmentIconDropdown.SelectOption(_equipSlotsNames[set.SlotIcon]);
        }

        private void SaveNewSet<T>(string name) where T : IEquipmentSet
        {
            SetEquipmentSetOptions<T>();
            int index;
            for (index = 0; index < EquipmentSetDropdown.options.Count; index++)
            {
                if (EquipmentSetDropdown.options[index].text.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    break;
            }
            if (index < EquipmentSetDropdown.options.Count)
            {
                EquipmentSetDropdown.SetValueWithoutNotify(index);
                EquipmentIconDropdown.value = EquipmentSetType == EquipmentSetTypes.Weapon ? 0 : 1;
            }
            SaveEquipmentSet<T>();
        }

        private void EquipmentSetChanged<T>(int index) where T : IEquipmentSet
        {
            Debug.Log($"Setting Equipment Set to '{EquipmentSetDropdown.GetSelectedOption().text}'. Index={index}.");

            var set = GetEquipmentService<T>().GetEquipmentSet(EquipmentSetDropdown.GetSelectedOption().text);
            if (index == 0)
            {
                SaveSetButton.interactable = false;
                return;
            }

            if (!GetEquipmentService<T>().TryEquipSet(set))
            {
                //TODO: Give better indicator that equippin failed
            }
            SaveSetButton.interactable = true;
            SelectSlotIcon<T>();
            //else
            //{
            //    EquipmentSetDropdown.SetValueWithoutNotify(0);
            //}
        }

        private void PromptRenameSet() => SetNamePanel.Show(EquipmentSetType, EquipmentSetDropdown.GetSelectedOption().text);

        private void PromptNewSet() => SetNamePanel.Show(EquipmentSetType);

        private void SaveEquipmentSet<T>() where T : IEquipmentSet
        {
            if (EquipmentSetDropdown.value == 0)
                return;

            //LoadEquipmentSet<T>();

            var set = GetEquipmentService<T>().GetEquippedAsSet(EquipmentSetDropdown.GetSelectedOption().text);
            set.SlotIcon = _equipSlots[EquipmentIconDropdown.GetSelectedOption().text];
            GetEquipmentService<T>().LearnEquipmentSetSkill(set);
            GetEquipmentService<T>().SaveEquipmentSet(set);
        }


        //private void SelectEquipmentSet<T>(string name) where T : IEquipmentSet
        //{
        //    SetEquipmentSetOptions<T>();
        //    int index;
        //    for (index = 0; index < EquipmentSetDropdown.options.Count; index++)
        //    {
        //        if (EquipmentSetDropdown.options[index].text.Equals(name, StringComparison.InvariantCultureIgnoreCase))
        //            break;
        //    }
        //    if (index < EquipmentSetDropdown.options.Count)
        //        EquipmentSetDropdown.SetValueWithoutNotify(index);
        //}
    }
}