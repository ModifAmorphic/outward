namespace ModifAmorphic.Outward.Shared.Config.Models
{
    public class SettingValueChangedArgs<T>
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }
}
