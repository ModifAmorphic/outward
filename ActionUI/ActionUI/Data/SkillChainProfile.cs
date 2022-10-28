using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public class SkillChainProfile
    {
        public List<SkillChain> SkillChains { get; set; } = new List<SkillChain>();
    }
    public class SkillChain
    {
        public string Name { get; set; }
        public int ItemID { get; set; } = -1;
        public string StatusEffectIcon { get; set; }
        public int IconItemID { get; set; } = -1;
        public SortedList<int, ChainAction> ActionChain { get; set; } = new SortedList<int, ChainAction>();
    }
    [Serializable]
    public class ChainAction
    {
        public int ItemID = -1;
        public string ItemUID;
    }
}
