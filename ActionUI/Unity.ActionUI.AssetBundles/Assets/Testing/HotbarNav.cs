using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Testing
{
    internal class HotbarNav : IHotbarNavActions
    {
        public bool IsHotbarRequested(out int hotbarIndex)
        {
            hotbarIndex = -1;
            return false;
        }

        public bool IsNextRequested()
        {
            return Input.GetKeyDown(KeyCode.Period);
        }

        public bool IsPreviousRequested()
        {
            return Input.GetKeyDown(KeyCode.Comma);
        }
    }
}
