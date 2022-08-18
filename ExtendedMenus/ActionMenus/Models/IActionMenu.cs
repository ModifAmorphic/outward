using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Models
{
    internal interface IActionMenu
    {
        bool IsShowing { get; }
        UnityEvent OnShow { get; }
        UnityEvent OnHide { get; }

        void Hide();
    }
}
