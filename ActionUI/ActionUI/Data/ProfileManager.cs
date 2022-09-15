using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public class ProfileManager
    {
        private readonly int _playerId = -1;

        public IActionUIProfileService ProfileService
        {
            get
            {
                if (Psp.Instance.GetServicesProvider(_playerId).TryGetService<IActionUIProfileService>(out var service))
                    return service;

                return null;
            }
        }

        public IHotbarProfileService HotbarProfileService
        {
            get
            {
                if (Psp.Instance.GetServicesProvider(_playerId).TryGetService<IHotbarProfileService>(out var service))
                    return service;

                return null;
            }
        }

        public IPositionsProfileService PositionsProfileService
        {
            get
            {
                if (Psp.Instance.GetServicesProvider(_playerId).TryGetService<IPositionsProfileService>(out var service))
                    return service;

                return null;
            }
        }

        public ProfileManager(int playerId) => _playerId = playerId;


        //public IActionMenusProfile GetActiveProfile()
        //{
        //    var activeProfile = ProfileService.GetActiveProfile();
        //    if (activeProfile == null)
        //    {
        //        var names = ProfileService.GetProfileNames();
        //        if (names != null && names.Any())
        //        {
        //            ProfileService.SetActiveProfile(names.First());
        //            activeProfile = ProfileService.GetActiveProfile();
        //        }
        //    }
        //    return activeProfile;
        //}

        //public void SetActiveProfile(string name)
        //{
        //    ProfileService.SetActiveProfile(name);
        //}

        //public IEnumerable<string> GetProfileNames() => ProfileService.GetProfileNames();
    }
}
