namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IActionSlotConfig
    {
        string HotkeyText { get; set; }
        bool ShowCooldownTime { get; set; }
        bool PreciseCooldownTime { get; set; }
        bool ShowZeroStackAmount { get; set; }
        EmptySlotOptions EmptySlotOption { get; set; }
    }
    public enum EmptySlotOptions
    {
        Transparent,
        Image,
        Hidden
    }
}
