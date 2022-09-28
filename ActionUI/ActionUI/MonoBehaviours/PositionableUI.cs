using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class PositionableUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public Image BackgroundImage;
        public Button ResetButton;

        public string TransformPath => transform.GetPath();

        public RectTransform RectTransform => GetComponent<RectTransform>();

        private UIPosition _originPosition;
        public UIPosition OriginPosition => _originPosition;

        public bool HasMoved => _startPositionSet && (!Mathf.Approximately(StartPosition.x, RectTransform.anchoredPosition.x) || !Mathf.Approximately(StartPosition.y, RectTransform.anchoredPosition.y));

        public UnityEvent<bool> OnIsPositionableChanged { get; private set; } = new UnityEvent<bool>();

        private bool _positioningEnabled = false;
        public bool IsPositionable => _positioningEnabled;

        private ProfileManager _profileManager;
        private bool profileChangeEventNeeded = false;
        private bool _buttonInit;
        private bool _raycasterAdded;
        private bool _canvasAdded;

        private Vector2 _startPosition;
        private bool _startPositionSet;
        public Vector2 StartPosition
        {
            get => _startPosition;
            private set
            {
                _startPosition = value;
                _startPositionSet = true;
            }
        }
        private Vector2 _offset;

        public UnityEvent<PositionableUI> UIElementMoved { get; } = new UnityEvent<PositionableUI>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            if (ResetButton != null && !_buttonInit)
            {
                ResetButton.onClick.AddListener(ResetToOrigin);
                _buttonInit = true;
            }
            if (_originPosition == default)
            {
                _originPosition = RectTransform.ToRectTransformPosition();
            }

            if (_profileManager != null && _profileManager.PositionsProfileService != null)
                SetPositionFromProfile(_profileManager.PositionsProfileService.GetProfile());

            StartPosition = new Vector2(RectTransform.anchoredPosition.x, RectTransform.anchoredPosition.y);

            if (BackgroundImage != null)
                BackgroundImage.gameObject.SetActive(_positioningEnabled);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            if (profileChangeEventNeeded && _profileManager != null && _profileManager.PositionsProfileService != null)
            {
                profileChangeEventNeeded = false;
                DebugLogger.Log($"[Debug  :ActionMenus] PositionableUI{{{name}}}::Update: Adding OnProfileChanged listener.");
                _profileManager.PositionsProfileService.OnProfileChanged += OnProfileChanged;
            }
        }

        public void SetProfileManager(ProfileManager profileManager)
        {
            _profileManager = profileManager;
            profileChangeEventNeeded = true;
            if (_profileManager.PositionsProfileService != null)
            {
                DebugLogger.Log($"[Debug  :ActionMenus] PositionableUI{{{name}}}::SetProfileManager: Adding OnProfileChanged listener.");
                _profileManager.PositionsProfileService.OnProfileChanged += OnProfileChanged;
                profileChangeEventNeeded = false;
                SetPositionFromProfile(_profileManager.PositionsProfileService.GetProfile());
            }
        }

        public void EnableMovement()
        {
            _positioningEnabled = true;
            if (BackgroundImage != null)
                BackgroundImage.gameObject.SetActive(_positioningEnabled);

            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                gameObject.AddComponent<Canvas>();
                _canvasAdded = true;
            }
            var raycaster = GetComponent<GraphicRaycaster>();
            if (canvas == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
                _raycasterAdded = true;
            }
            OnIsPositionableChanged.Invoke(_positioningEnabled);
        }

        public void DisableMovement()
        {
            _positioningEnabled = false;
            if (BackgroundImage != null)
                BackgroundImage.gameObject.SetActive(_positioningEnabled);

            if (_raycasterAdded)
            {
                var raycaster = GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                    UnityEngine.Object.Destroy(raycaster);

                _raycasterAdded = false;
            }
            if (_canvasAdded)
            {
                var canvas = gameObject.GetComponent<Canvas>();
                if (canvas != null)
                    UnityEngine.Object.Destroy(canvas);

                _canvasAdded = false;
            }
            OnIsPositionableChanged.Invoke(_positioningEnabled);
        }

        public void SetPosition(float x, float y) => RectTransform.anchoredPosition = new Vector2(x, y);

        public void SetPosition(UIPosition position) => SetPosition(position.AnchoredPosition.X, position.AnchoredPosition.Y);

        public void SetPositionFromProfile(PositionsProfile profile)
        {
            var position = profile.Positions?.FirstOrDefault(p => p.TransformPath == TransformPath);
            if (position != default)
            {
                DebugLogger.Log($"[Debug  :ActionMenus] PositionableUI{{{name}}}: Setting position of PositionableUI {name} to modified position of ({position.ModifiedPosition.AnchoredPosition.X}, {position.ModifiedPosition.AnchoredPosition.Y}).");
                SetPosition(position.ModifiedPosition);
                _originPosition = position.OriginPosition;
            }
        }

        private void OnProfileChanged(PositionsProfile profile)
        {
            DebugLogger.Log($"[Debug  :ActionMenus] PositionableUI{{{name}}}: OnProfileChanged for PositionableUI {name}.");
            ResetToOrigin();
            SetPositionFromProfile(profile);
        }

        public void ResetToOrigin()
        {
            if (_originPosition != null)
            {
                DebugLogger.Log($"[Debug  :ActionMenus] PositionableUI{{{name}}}: Setting position of PositionableUI {name} to origin position of ({_originPosition.AnchoredPosition.X}, {_originPosition.AnchoredPosition.Y}).");
                SetPosition(_originPosition);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_positioningEnabled)
                transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _offset;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_positioningEnabled)
            {
                DebugLogger.Log("Dragging started.");
                if (_originPosition == default)
                {
                    _originPosition = RectTransform.ToRectTransformPosition();
                }
                StartPosition = new Vector2(RectTransform.anchoredPosition.x, RectTransform.anchoredPosition.y);
                _offset = eventData.position - new Vector2(RectTransform.position.x, RectTransform.position.y);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_positioningEnabled)
            {
                if (HasMoved)
                    UIElementMoved?.TryInvoke(this);
                DebugLogger.Log("Dragging Done.");
            }
        }

        /// <summary>
        /// Gets a new instance of a <see cref="UIPosition"/> using this <see cref="PositionableUI"/>'s <see cref="RectTransform"/>.
        /// </summary>
        /// <returns>New instance of a <see cref="UIPosition"/></returns>
        public UIPositions GetUIPositions() =>
          new UIPositions()
          {
              ModifiedPosition = RectTransform.ToRectTransformPosition(),
              OriginPosition = _originPosition,
              TransformPath = transform.GetPath()
          };
    }
}
