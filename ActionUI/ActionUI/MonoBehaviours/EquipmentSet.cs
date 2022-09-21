using ModifAmorphic.Outward.Unity.ActionUI;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public enum EquipmentSetTypes
    {
        Weapon,
        Armor
    }
    [UnityScriptComponent]
    public class EquipmentSet : MonoBehaviour
    {
        public EquipmentSetTypes EquipmentSetType;

        public Dropdown EquipmentSetDropdown;
        public Button NewSetButton;
        public Button RenameSetButton;

        public Button LoadSetButton;
        public Button SaveSetButton;

        public Dropdown EquipmentIconDropdown;

        public ActionItemView EquipmentIcon;

        public GameObject SetNamePanel;
        public InputField EquipmentSetNameInput;
        public Button NameEquipmentSetButton;



        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void OnAwake()
        {

        }

        // Start is called before the first frame update
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Start()
        {

        }
    }
}