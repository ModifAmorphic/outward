namespace ModifAmorphic.Outward.ActionMenus.Models
{
    internal interface IOutwardItem
    {
        string ActionUid { get; }
        Item ActionItem { get; }
    }
}