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
        public Dictionary<string, HotbarProfileData> HotbarProfiles { get; set; } = new Dictionary<string, HotbarProfileData>();
        public HotbarProfileData Create(string name, HotbarsContainer hotbarsContainer)
        {
            var data = ToHotbarData(hotbarsContainer);
            var hotbarAssigns = data.Select(h => (IHotbarData<ISlotData>)h);

            var profile = new HotbarProfileData()
            {
                Name = name,
                Rows = hotbarsContainer.Controller.GetRowCount(),
                SlotsPerRow = hotbarsContainer.Controller.GetActionSlotsPerBar(),
                HotbarAssignments = hotbarAssigns.ToList()
            };

            if (!HotbarProfiles.ContainsKey(name))
                HotbarProfiles.Add(name, profile);
            else
                HotbarProfiles[name] = profile;

            return profile;
        }

        public HotbarProfileData GetProfile(string name)
        {
            if (HotbarProfiles.TryGetValue(name, out var profile))
                return profile;
            return null;
        }

        public IEnumerable<string> GetProfileNames()
        {
            return HotbarProfiles.Keys;
        }

        public void SaveProfile(HotbarProfileData profile)
        {
            if (!HotbarProfiles.ContainsKey(profile.Name))
                HotbarProfiles.Add(profile.Name, profile);
            else
                HotbarProfiles[profile.Name] = profile;
        }

        public static List<HotbarData> ToHotbarData(HotbarsContainer hbc)
        {
            var hotbars = new List<HotbarData>();

            for (int h = 0; h < hbc.Hotbars.Length; h++)
            {
                var hotbarData = new HotbarData()
                {
                    HotbarIndex = h
                };
                for (int s = 0; s < hbc.Hotbars[h].Length; s++)
                {
                    hotbarData.SlotsAssigned.Add(new TestSlotData()
                    {
                        SlotIndex = hbc.Hotbars[h][s].SlotIndex,
                        Config = hbc.Hotbars[h][s].Config,
                        });
                }
                hotbars.Add(hotbarData);
            }

            return hotbars;
        }
    }
}
