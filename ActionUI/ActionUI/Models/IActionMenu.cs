using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IActionMenu
    {
        bool IsShowing { get; }
        UnityEvent OnShow { get; }
        UnityEvent OnHide { get; }

        void Hide();
    }
}
