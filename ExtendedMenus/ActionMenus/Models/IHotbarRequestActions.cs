using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IHotbarRequestActions
    {
        bool IsNextRequested();
        bool IsPreviousRequested();
        bool IsSelectRequested();
        int HotbarIndexRequested { get; }
    }
}
