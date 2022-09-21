namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface IStackable
    {
        bool IsStackable { get; }
        int GetAmount();
    }
}
