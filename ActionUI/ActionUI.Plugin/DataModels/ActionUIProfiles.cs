using System.Collections.Generic;

namespace ModifAmorphic.Outward.ActionUI.DataModels
{
    public class ActionUIProfiles
    {
        public string ActiveProfile { get; set; }
        public List<ActionUIProfile> Profiles { get; set; } = new List<ActionUIProfile>();
    }
}
