using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    internal class Profile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string KeyboardMapFile => Name + "_KeyboardMap.xml";
        public string HotbarsConfigFile => Name + "_Hotbars.json";

        public Profile HotbarsProfile { get; set; }

        public Profile GetProfile()
        {
            return null;
        }

        public void SaveProfile()
        {
        }
        //public static Profile Create(string name, HotbarsContainer hotbarsContainer)
        //{
        //    var barProfile = new Profile()
        //    {
        //        Rows = hotbarsContainer.Controller.GetRowCount(),
        //        SlotsPerBar = hotbarsContainer.Controller.GetActionSlotsPerBar(),
        //    };
        //    Profile profile = new Profile()
        //    {
        //        Name = name,
        //        HotbarsProfile = barProfile
        //    };
        //    return profile;
        //}
    }
}
