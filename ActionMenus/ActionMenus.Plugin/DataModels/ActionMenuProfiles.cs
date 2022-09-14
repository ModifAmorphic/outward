using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.DataModels
{
    public class ActionMenuProfiles
    {
        public string ActiveProfile { get; set; }
        public List<ActionMenusProfile> Profiles { get; set; } = new List<ActionMenusProfile>();
    }
}
