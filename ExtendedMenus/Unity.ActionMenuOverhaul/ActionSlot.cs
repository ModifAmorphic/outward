using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ActionSlot : MonoBehaviour
    {
        private Text buttonText;
        public int HotbarId;
        public int SlotNo;

        private Transform _slotPanel;

        private CanvasGroup _canvasGroup;
        private Image _border;

        private Text _keyText;
        public Text KeyText => _keyText;

        private Image _actionImage;
        public Image ActionImage => _actionImage;

        private GameObject _assignedAction;


        private Button _actionButton;
        public Button ActionButton => _actionButton;

        private void Awake()
        {
            SetComponents();
            if (name != "BaseActionSlot")
            {
                name = $"ActionSlot_{HotbarId}_{SlotNo}";
#if DEBUG
                buttonText = _actionButton.GetComponentInChildren<Text>(true);
                buttonText.text = $"Bar {HotbarId}\nSlot: {SlotNo}"; //itemTypes[typeIndex];
#endif
            }
        }
        private void Start()
        {
            if (_assignedAction == null)
            {
                Debug.Log("ActionSlot:Start()");
                //AssignEmpty();
            }
        }
        private void SetComponents()
        {
            _slotPanel = transform.Find("SlotPanel");
            _canvasGroup = _slotPanel.GetComponent<CanvasGroup>();
            
            _keyText = GetComponentsInChildren<Text>(true).First(t => t.name == "KeyText");
            _border = _slotPanel.GetComponentsInChildren<Image>(true).First(i => i.name == "ActionBorder");
            _actionButton = _slotPanel.GetComponentInChildren<Button>(true);

            _actionImage = _actionButton.GetComponent<Image>();
            
            //var images = _actionButton.GetComponentsInChildren<Image>();

            
        }
        public void AssignEmpty()
        {
            _canvasGroup.alpha = 0.6f;
            _actionImage.overrideSprite = null;
            _actionImage.sprite = null;
            _actionImage.color = Color.grey;
            //_icon.gameObject.SetActive(false);
                ////this.m_sldCooldown.fillRect.GetComponent<Image>().overrideSprite = (Sprite)null;
                ////this.m_sldCooldown.normalizedValue = 0.0f;
                ////this.m_lblStackCount.text = "";
                ////this.m_lblName.text = "";
                ////this.m_coolDownHolder.alpha = 0.0f;
                //this.m_canvasGroup.alpha = 0.6f;
                ////if (this.m_sldDurability.gameObject.activeSelf)
                ////    this.m_sldDurability.gameObject.SetActive(false);
                ////if (!this.m_brokeStatusIcon.gameObject.activeSelf)
                ////    return;
                //this.m_brokeStatusIcon.gameObject.SetActive(false);
        }
        public void AssignAction()
        {
            _canvasGroup.alpha = 1f;
        }
    }
}