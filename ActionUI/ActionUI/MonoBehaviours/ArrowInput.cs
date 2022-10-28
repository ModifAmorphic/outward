using ModifAmorphic.Outward.Unity.ActionUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ArrowInput : MonoBehaviour
    {
        public Button LeftArrow;
        public Button RightArrow;
        public InputField InputText;
        public int Minimum = 1;
        public int Maximum = 100;


        public UnityEvent<int> OnValueChanged;

        public int Amount => int.TryParse(InputText.text, out var amount) ? amount : Minimum;

        // Start is called before the first frame update
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Start()
        {
            InputText.onValueChanged.AddListener((call) => ValidateInput(Amount));
        }

        public void SetAmount(int amount)
        {
            if (amount >= Minimum && amount <= Maximum)
                InputText.text = amount.ToString();
        }

        public void IncreaseAmount()
        {
            if (Amount < Maximum)
                InputText.text = (Amount + 1).ToString();
        }
        public void DecreaseAmount()
        {
            if (Amount > Minimum)
                InputText.text = (Amount - 1).ToString();
        }

        private void ValidateInput(int amount)
        {
            var constrainedAmount = amount;
            if (amount < Minimum)
                constrainedAmount = Minimum;
            else if (amount > Maximum)
                constrainedAmount = Maximum;

            InputText.SetTextWithoutNotify(constrainedAmount.ToString());
            OnValueChanged.Invoke(constrainedAmount);
        }
    }
}