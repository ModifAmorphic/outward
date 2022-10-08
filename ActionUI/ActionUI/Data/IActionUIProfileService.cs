using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IActionUIProfileService
    {
        IEnumerable<string> GetProfileNames();
        IActionUIProfile GetActiveProfile();
        event Action<IActionUIProfile> OnActiveProfileChanged;
        event Action<IActionUIProfile> OnActiveProfileSwitched;
        event Action<IActionUIProfile> OnActiveProfileSwitching;
        void SetActiveProfile(string name);
        void Save();
        void SaveNew(IActionUIProfile profile);
        void Rename(string newName);
    }
}
