using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal class CharacterPrivates
    {
        public CharacterInventory m_inventory { get; set; }
        public float m_lastHandleBagTime { get; set; }
        public bool m_interactCoroutinePending { get; set; }
        public bool m_IsHoldingInteract { get; set; }
        public bool m_IsHoldingDragCorpse { get; set; }
    }
}
