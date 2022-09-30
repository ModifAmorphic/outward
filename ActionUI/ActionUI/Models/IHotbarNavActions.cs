namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IHotbarNavActions
    {
        bool IsNextRequested();
        bool IsPreviousRequested();
        bool IsHotbarRequested(out int hotbarIndex);
    }
}
