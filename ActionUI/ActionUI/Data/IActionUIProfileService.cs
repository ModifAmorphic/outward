using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IActionUIProfileService
    {
        IEnumerable<string> GetProfileNames();
        IActionUIProfile GetActiveProfile();
        UnityEvent<IActionUIProfile> OnNewProfile { get; }
        UnityEvent<IActionUIProfile> OnActiveProfileChanged { get; }
        void SetActiveProfile(string name);
        void Save();
        void SaveNew(IActionUIProfile profile);
        void Rename(string newName);
    }
}
