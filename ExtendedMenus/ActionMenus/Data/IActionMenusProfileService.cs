using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IActionMenusProfileService
    {
        IEnumerable<string> GetProfileNames();
        IActionMenusProfile GetActiveProfile();
        event Action<IActionMenusProfile> OnActiveProfileChanged;
        void SetActiveProfile(string name);
    }
}
