namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IHotbarNavActions
    {
        bool IsNextRequested();
        bool IsPreviousRequested();
        bool IsHotbarRequested(out int hotbarIndex);
    }
}
