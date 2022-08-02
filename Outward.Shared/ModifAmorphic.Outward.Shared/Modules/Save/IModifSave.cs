namespace ModifAmorphic.Outward.Modules.Save
{
    public interface IModifSave<T>
    {
        string SaveName { get; }

        string Serialize();
        T Deserialize();
    }
}
