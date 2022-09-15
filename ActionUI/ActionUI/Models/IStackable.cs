namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface IStackable
    {
        bool IsStackable { get; }
        int GetAmount();
    }
}
