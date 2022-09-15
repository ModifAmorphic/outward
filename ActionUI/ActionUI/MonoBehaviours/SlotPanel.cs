using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class SlotPanel : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
    {
        //Border around the ActionSlot
        private Image _borderImage;
        public Image BorderImage => _borderImage;

        private Image _borderHighlightImage;
        public Image ActionBorderHighlight => _borderHighlightImage;

        private bool _isAwake;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            _borderImage = GetComponentsInChildren<Image>(true).First(i => i.name == "ActionBorder");
            _borderHighlightImage = GetComponentsInChildren<Image>(true).First(i => i.name == "ActionBorderHighlight");
            _borderHighlightImage.enabled = false;
            _borderImage.enabled = true;
            _isAwake = true;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isAwake)
            {
                _borderHighlightImage.enabled = true;
                _borderImage.enabled = false;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isAwake)
            {
                _borderHighlightImage.enabled = false;
                _borderImage.enabled = true;
            }
        }
    }
}
