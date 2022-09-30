using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IActionMenu
    {
        bool IsShowing { get; }
        UnityEvent OnShow { get; }
        UnityEvent OnHide { get; }

        void Hide();
    }
}
