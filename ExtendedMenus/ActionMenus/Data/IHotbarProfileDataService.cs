using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IHotbarProfileDataService
    {
        IEnumerable<string> GetProfileNames();
        IHotbarProfileData GetProfile(string name);
        IHotbarProfileData GetActiveProfile();
        bool TryGetActiveProfileName(out string name);
        void SetActiveProfile(string name);
        void SaveProfile(IHotbarProfileData profile);
        IHotbarProfileData AddHotbar(IHotbarProfileData profile);
        IHotbarProfileData RemoveHotbar(IHotbarProfileData profile);
        IHotbarProfileData AddRow(IHotbarProfileData profile);
        IHotbarProfileData RemoveRow(IHotbarProfileData profile);
        IHotbarProfileData AddSlot(IHotbarProfileData profile);
        IHotbarProfileData RemoveSlot(IHotbarProfileData profile);

        event Action<IHotbarProfileData> OnActiveProfileChanged;
    }
}
