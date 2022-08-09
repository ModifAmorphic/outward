using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Testing
{
    internal class TestHotbarProfileData : IHotbarProfileDataService
    {
        public Dictionary<string, IHotbarProfileData> HotbarProfiles { get; set; } = new Dictionary<string, IHotbarProfileData>();

        public event Action<IHotbarProfileData> OnActiveProfileChanged;

        public IHotbarProfileData AddHotbar(IHotbarProfileData profile)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData AddRow(IHotbarProfileData profile)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData AddSlot(IHotbarProfileData profile)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData GetActiveProfile()
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData GetProfile(string name)
        {
            if (HotbarProfiles.TryGetValue(name, out var profile))
                return profile;
            return null;
        }

        public IEnumerable<string> GetProfileNames()
        {
            return HotbarProfiles.Keys;
        }

        public IHotbarProfileData RemoveHotbar(IHotbarProfileData profile)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData RemoveRow(IHotbarProfileData profile)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData RemoveSlot(IHotbarProfileData profile)
        {
            throw new NotImplementedException();
        }

        public void SaveProfile(IHotbarProfileData profile)
        {
            if (!HotbarProfiles.ContainsKey(profile.Name))
                HotbarProfiles.Add(profile.Name, profile);
            else
                HotbarProfiles[profile.Name] = profile;
        }

        public void SetActiveProfile(string name)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData SetCooldownTimer(IHotbarProfileData profile, bool showTimer, bool preciseTime)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfileData SetEmptySlotView(IHotbarProfileData profile, EmptySlotOptions option)
        {
            throw new NotImplementedException();
        }

        public bool TryGetActiveProfileName(out string name)
        {
            throw new NotImplementedException();
        }
    }
}
