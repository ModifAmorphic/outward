using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ModifAmorphic.Outward.Unity.ActionUI.Extensions.SelectableExtensions;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class SelectableTransitions : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Image SelectImage;


        private Selectable _selectable;
        public Selectable Selectable => _selectable;

        public SelectionState SelectionState => _selectable?.GetSelectionState() ?? SelectionState.Normal;

        public bool Selected => _selected;
        private bool _selected;

        public event Action<SelectableTransitions> OnSelected;
        public event Action<SelectableTransitions> OnDeselected;

        //public event Action<SelectableTransitions> OnPressed;
        //public event Action<SelectableTransitions> OnUnpressed;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        private void Start()
        {
            if (SelectImage != null)
                SelectImage.enabled = _selected;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (SelectImage != null)
                SelectImage.enabled = true;

            _selected = true;
            OnSelected?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (SelectImage != null)
                SelectImage.enabled = false;

            _selected = false;
            OnDeselected?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}