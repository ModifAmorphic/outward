using ModifAmorphic.Outward.Unity.ActionMenus.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.EventTriggers
{
    [UnityScriptComponent]
    internal class PositionableUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public Image BackgroundImage;

        public string TransformPath => transform.GetPath();

        public RectTransform RectTransform => GetComponent<RectTransform>();
        public Vector2 StartPosition { get; private set; }

        public bool HasMoved => StartPosition != default || (StartPosition.x == RectTransform.position.x || StartPosition.y == RectTransform.position.y);

        private Vector2 _offset;
        private bool _dragEnabled = true;

        public UnityEvent<PositionableUI> UIElementMoved { get; } = new UnityEvent<PositionableUI>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            if (StartPosition == default)
            {
                StartPosition = new Vector2(RectTransform.position.x, RectTransform.position.y);
            }
        }

        public void EnableMovement()
        {
            _dragEnabled = true;
            if (BackgroundImage != null)
                BackgroundImage.enabled = true;
        }

        public void DisableMovement()
        {
            _dragEnabled = true;
            if (BackgroundImage != null)
                BackgroundImage.enabled = true;
        }

        public void SetPosition(Vector2 position) => SetPosition(position.x, position.y);
        
        public void SetPosition(float x, float y)
        {
            RectTransform.position = new Vector2(x, y);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragEnabled)
                transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _offset;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_dragEnabled)
            {
                Debug.Log("Dragging started.");
                _offset = eventData.position - new Vector2(transform.position.x, transform.position.y);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_dragEnabled)
            {
                UIElementMoved?.Invoke(this);
                Debug.Log("Dragging Disabled.");
            }
        }
    }
}
