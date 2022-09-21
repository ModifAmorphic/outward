namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface ISlotData
    {
        int SlotIndex { get; set; }
        IActionSlotConfig Config { get; set; }
    }
}
