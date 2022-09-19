using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Assets.Testing
{
    internal class TestHotbarProfileDataService : IHotbarProfileService
    {
        private string activeProfileName = TestDefaultProfile.DefaultProfile.Name;
        public Dictionary<string, IHotbarProfile> HotbarProfiles { get; set; } = new Dictionary<string, IHotbarProfile>()
        {
            { TestDefaultProfile.DefaultProfile.Name, TestDefaultProfile.DefaultProfile }
        };

        UnityEvent<IHotbarProfile, HotbarProfileChangeTypes> IHotbarProfileService.OnProfileChanged => throw new NotImplementedException();

        public event Action<IHotbarProfile> OnProfileChanged;

        public IHotbarProfile AddHotbar()
        {
            throw new NotImplementedException();
        }

        public IHotbarProfile AddRow()
        {
            throw new NotImplementedException();
        }

        public IHotbarProfile AddSlot()
        {
            throw new NotImplementedException();
        }

        public IHotbarProfile GetProfile()
        {
            return HotbarProfiles.ContainsKey(activeProfileName) ? HotbarProfiles[activeProfileName] : HotbarProfiles.Values.FirstOrDefault();
        }


        public IEnumerable<string> GetProfileNames()
        {
            return HotbarProfiles.Keys;
        }

        public IHotbarProfile RemoveHotbar()
        {
            throw new NotImplementedException();
        }

        public IHotbarProfile RemoveRow()
        {
            throw new NotImplementedException();
        }

        public IHotbarProfile RemoveSlot()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            
        }

        public void SaveNew(IHotbarProfile profile)
        {
            //if (!HotbarProfiles.ContainsKey(profile.Name))
            //    HotbarProfiles.Add(profile.Name, profile);
            //else
            //    HotbarProfiles[profile.Name] = profile;
        }

        public void SetActiveProfile(string name)
        {
            activeProfileName = name;
        }

        public IHotbarProfile SetCombatMode(bool combatMode)
        {
            GetProfile().CombatMode = combatMode;
            return GetProfile();
        }

        public IHotbarProfile SetCooldownTimer(bool showTimer, bool preciseTime)
        {
            throw new NotImplementedException();
        }

        public IHotbarProfile SetEmptySlotView(EmptySlotOptions option)
        {
            throw new NotImplementedException();
        }

        public bool TryGetActiveProfileName(out string name)
        {
            throw new NotImplementedException();
        }

        public void Update(HotbarsContainer hotbar)
        {
            throw new NotImplementedException();
        }

    }
}
