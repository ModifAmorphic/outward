using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ActionItemView : MonoBehaviour
    {
        private ISlotAction _slotAction;
        public ISlotAction SlotAction => _slotAction;

        private Button _button;
        public Button Button => _button;
        private Text _text;
        private Text _stackText;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _button = GetComponentInChildren<Button>(true);
            _text = _button.GetComponentsInChildren<Text>(true).First(t => t.name.Equals("ButtonText"));
            _stackText = _button.GetComponentsInChildren<Text>(true).First(t => t.name.Equals("StackText"));
        }
       
        public void SetViewItem(ISlotAction action)
        {
            _slotAction = action;
            _button.image.sprite = action.ActionIcon;
            _text.text = action.DisplayName;
            _stackText.text = action.Stack != null && action.Stack.IsStackable && action.Stack.GetAmount() > 0 ? action.Stack.GetAmount().ToString() : string.Empty;
        }
    }
}