using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IActionMenusProfileService
    {
        IEnumerable<string> GetProfileNames();
        IActionMenusProfile GetActiveProfile();
        UnityEvent<IActionMenusProfile> OnNewProfile { get; }
        UnityEvent<IActionMenusProfile> OnActiveProfileChanged { get; }
        void SetActiveProfile(string name);
        void Save();
        void SaveNew(IActionMenusProfile profile);
        void Rename(string newName);
    }
}
