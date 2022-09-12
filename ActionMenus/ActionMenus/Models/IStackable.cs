using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IStackable
    {
        bool IsStackable { get; }
        int GetAmount();
    }
}
