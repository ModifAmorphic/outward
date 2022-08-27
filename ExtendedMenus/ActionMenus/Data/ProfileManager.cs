using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public class ProfileManager
    {
        private readonly int _playerId = -1;

        public event Action<IActionMenusProfile> OnActiveProfileChanged;

        public IActionMenusProfileService ProfileService => Psp.Instance.GetServicesProvider(_playerId).GetService<IActionMenusProfileService>();
        
        public IHotbarProfileService HotbarProfileService => Psp.Instance.GetServicesProvider(_playerId).GetService<IHotbarProfileService>();

        public ProfileManager(int playerId) => _playerId = playerId;


        public IActionMenusProfile GetActiveProfile()
        {
            var activeProfile = ProfileService.GetActiveProfile();
            if (activeProfile == null)
            {
                var names = ProfileService.GetProfileNames();
                if (names != null && names.Any())
                {
                    ProfileService.SetActiveProfile(names.First());
                    activeProfile = ProfileService.GetActiveProfile();
                }
            }
            return activeProfile;
        }

        public void SetActiveProfile(string name)
        {
            ProfileService.SetActiveProfile(name);
        }

        public IEnumerable<string> GetProfileNames() => ProfileService.GetProfileNames();
    }
}
