namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal class CharacterPrivates
    {
#pragma warning disable IDE1006 // Naming Styles
        public CharacterInventory m_inventory { get; set; }

        public float m_lastHandleBagTime { get; set; }
        public bool m_interactCoroutinePending { get; set; }
        public bool m_IsHoldingInteract { get; set; }
        public bool m_IsHoldingDragCorpse { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
