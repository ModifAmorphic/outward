using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Services
{

    public class PositionsProfileJsonService : JsonProfileService<PositionsProfile>, IPositionsProfileService
    {
        protected override string FileName => "UIPositions.json";

        public event Action<PositionsProfile> OnProfileChanged;

        public PositionsProfileJsonService(ProfileService profileService, Func<IModifLogger> getLogger) : base(profileService, getLogger)
        {
        }

        public void AddOrUpdate(UIPositions position)
        {
            GetProfile().AddOrReplacePosition(position);
            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
        }

        public void Remove(UIPositions position)
        {
            GetProfile().RemovePosition(position);
            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
        }

        private void RemoveOriginPositions(PositionsProfile positonsProfile)
        {
            var removals = positonsProfile.Positions.Where(p => p.ModifiedPosition == p.OriginPosition).ToList();
            removals.ForEach(p => positonsProfile.RemovePosition(p));
        }

        protected override void SaveProfile(PositionsProfile positonsProfile)
        {
            RemoveOriginPositions(positonsProfile);
            base.SaveProfile(positonsProfile);
        }

        protected override void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            base.RefreshCachedProfile(actionMenusProfile, suppressChangeEvent);
            OnProfileChanged?.TryInvoke(GetProfile());
        }
    }
}
