using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    internal interface ISettingsView
    {
        bool IsShowing { get; }
        void Show();
        void Hide();
    }
}
