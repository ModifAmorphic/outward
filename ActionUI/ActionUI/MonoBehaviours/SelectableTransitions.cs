using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class SelectableTransitions : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public Image SelectImage;

        
        private Selectable _selectable;
        public Selectable Selectable => _selectable;

        public bool Selected => _selected;
        private bool _selected;

        public event Action<SelectableTransitions> OnSelected;
        public event Action<SelectableTransitions> OnDeselected;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        private void Start()
        {
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
    }
}