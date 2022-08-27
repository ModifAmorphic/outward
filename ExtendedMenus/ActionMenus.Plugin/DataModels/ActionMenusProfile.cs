using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.DataModels
{
    public class ActionMenusProfile : IActionMenusProfile
    {
        public string ActiveProfile { get; set; }
        [JsonIgnore]
        public string Path { get; set; }
    }
}
