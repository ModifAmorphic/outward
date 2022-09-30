namespace ModifAmorphic.Outward.ActionUI.Models
{
    internal interface IOutwardItem
    {
        string ActionUid { get; }
        Item ActionItem { get; }
    }
}