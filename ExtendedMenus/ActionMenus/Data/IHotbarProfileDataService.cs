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
        void UpdateProfile(HotbarsContainer hotbar, IHotbarProfileData profile);
        IHotbarProfileData AddHotbar(IHotbarProfileData profile);
        IHotbarProfileData RemoveHotbar(IHotbarProfileData profile);
        IHotbarProfileData AddRow(IHotbarProfileData profile);
        IHotbarProfileData RemoveRow(IHotbarProfileData profile);
        IHotbarProfileData AddSlot(IHotbarProfileData profile);
        IHotbarProfileData RemoveSlot(IHotbarProfileData profile);
        IHotbarProfileData SetCooldownTimer(IHotbarProfileData profile, bool showTimer, bool preciseTime);
        IHotbarProfileData SetEmptySlotView(IHotbarProfileData profile, EmptySlotOptions option);

        event Action<IHotbarProfileData> OnActiveProfileChanged;
    }
}
