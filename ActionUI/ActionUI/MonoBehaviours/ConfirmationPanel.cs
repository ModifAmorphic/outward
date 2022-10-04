using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ConfirmationPanel : MonoBehaviour
    {

        public Button ConfirmButton;
        public Button CancelButton;
        public Text PromptText;
        public Text DisplayText;

        public bool IsShowing => gameObject.activeSelf;

        private bool _isShowInvoked;

        Action _confirmationAction;

        public UnityEvent OnShow { get; } = new UnityEvent();

        public UnityEvent OnHide { get; } = new UnityEvent();


        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            if (!_isShowInvoked)
                Hide(false);
        }

        public void Show(Action confirmationAction, string promptText)
        {
            PromptText.text = promptText;
            _confirmationAction = confirmationAction;
            _isShowInvoked = true;

            gameObject.SetActive(true);
            OnShow.TryInvoke();
        }

        public void Hide() => Hide(true);

        private void Hide(bool raiseEvent)
        {
            gameObject.SetActive(false);
            _isShowInvoked = false;
            if (raiseEvent)
            {
                OnHide.TryInvoke();
            }
        }

        public void OnConfirm()
        {
            _confirmationAction.Invoke();
            Hide();
        }
    }
}