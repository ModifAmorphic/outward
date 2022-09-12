using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{ 
    public interface ICooldown
    {
        bool HasCooldown { get; }
        bool GetIsInCooldown();
        float GetProgress();
        float GetSecondsRemaining();
    }
}
