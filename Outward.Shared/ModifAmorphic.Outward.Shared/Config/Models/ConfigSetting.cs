using BepInEx.Configuration;
using System;

namespace ModifAmorphic.Outward.Config.Models
{
    public class ConfigSetting<T>
    {
        public string Name { get; set; }
        public string Section { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public int Order { get; set; }
        public bool IsAdvanced { get; set; }

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                //TODO: not really the safest but since ConfigEntries only allow for
                //certain types of T that are all structs except string, it works out.
                //Remove in future. Really only useful for debugging.
                var oldValue = _value;
                _value = value;
                RaiseValueChangedEvent(oldValue, value);
            }
        }
        public T DefaultValue { get; set; }
        public bool IsVisible { get; internal set; }

        public event EventHandler<SettingValueChangedArgs<T>> ValueChanged;

        //internal properties
        internal ConfigEntry<T> BoundConfigEntry { get; set; }
        internal bool SuppressValueChangedEvents { get; set; } = false;

        private void RaiseValueChangedEvent(T oldValue, T newValue)
        {
            if (!SuppressValueChangedEvents)
            {
#if DEBUG
                UnityEngine.Debug.Log($"[{DebugLoggerInfo.ModName}] - ConfigSetting: {this.Name}. Triggering {nameof(ValueChanged)} Event. " +
                    $"oldValue: {oldValue}. newvalue: {newValue} " +
                    $"{nameof(ValueChanged)} null? {ValueChanged == null}. " +
                    $"{nameof(SuppressValueChangedEvents)}? {SuppressValueChangedEvents}");
#endif
                ValueChanged?.Invoke(null
                    , new SettingValueChangedArgs<T>()
                    {
                        NewValue = newValue,
                        OldValue = oldValue
                    });
            }
        }
    }
}
