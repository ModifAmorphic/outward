using System.Collections;
using System.Collections.Generic;
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

        private CanvasGroup _canvasGroup;
        private Image _border;
        private Image _icon;
        private GameObject _assignedAction;

        private void Awake()
        {
            if (SlotNo != 0)
            {
                name = $"ActionSlot_{HotbarId}_{SlotNo - 1}";
                buttonText = GetComponentInChildren<Text>();
                buttonText.text = $"Bar {HotbarId}\nSlot: {SlotNo}"; //itemTypes[typeIndex];
            }
            SetComponents();
        }
        private void Start()
        {
            if (_assignedAction == null)
            {
                Debug.Log("ActionSlot:Start()");
                AssignEmpty();
            }
        }
        private void SetComponents()
        {
            var images = GetComponentsInChildren<Image>();

            foreach (var image in images)
            {
                switch (image.name)
                {
                    case "ActionBorder":
                        _border = image;
                        break;
                    case "ActionIcon":
                        _icon = image;
                        break;
                }
            }
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        public void AssignEmpty()
        {
            _canvasGroup.alpha = 0.6f;
            _icon.overrideSprite = null;
            _icon.sprite = null;
            _icon.color = Color.grey;
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