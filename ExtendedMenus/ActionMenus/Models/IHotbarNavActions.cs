using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IHotbarNavActions
    {
        bool IsNextRequested();
        bool IsPreviousRequested();
        bool IsHotbarRequested(out int hotbarIndex);
    }
}
