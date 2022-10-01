using System.Collections.Generic;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IActionUIProfileService
    {
        IEnumerable<string> GetProfileNames();
        IActionUIProfile GetActiveProfile();
        UnityEvent<IActionUIProfile> OnNewProfile { get; }
        UnityEvent<IActionUIProfile> OnActiveProfileChanged { get; }
        UnityEvent<IActionUIProfile> OnActiveProfileSwitched { get; }
        void SetActiveProfile(string name);
        void Save();
        void SaveNew(IActionUIProfile profile);
        void Rename(string newName);
    }
}
