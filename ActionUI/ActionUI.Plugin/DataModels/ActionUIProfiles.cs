using System.Collections.Generic;

namespace ModifAmorphic.Outward.UI.DataModels
{
    public class ActionUIProfiles
    {
        public string ActiveProfile { get; set; }
        public List<ActionUIProfile> Profiles { get; set; } = new List<ActionUIProfile>();
    }
}
