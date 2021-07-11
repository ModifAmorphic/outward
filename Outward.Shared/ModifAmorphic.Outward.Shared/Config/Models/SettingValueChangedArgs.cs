namespace ModifAmorphic.Outward.Config.Models
{
    public class SettingValueChangedArgs<T>
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }
}
