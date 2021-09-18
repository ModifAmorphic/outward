using ModifAmorphic.Outward.ExtraSlots.Config;
using System;

namespace ModifAmorphic.Outward.ExtraSlots.Events
{
    internal static class ExtraSlotsConfigEvents
    {
        public static event EventHandler<ExtraSlotsSettings> ExtraSlotsSettingsChanged;
        public static void RaiseExtraSlotsSettingsChanged(object sender, ExtraSlotsSettings extraSlotsSettings)
        {
#if DEBUG
            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Triggering {nameof(ExtraSlotsSettingsChanged)} Event. {nameof(ExtraSlotsSettingsChanged)} null? {ExtraSlotsSettingsChanged == null}");
#endif
            ExtraSlotsSettingsChanged?.Invoke(sender, extraSlotsSettings);
        }

        public static event EventHandler<ExtraSlotsSettings> UiSettingsChanged;
        public static void RaiseUiSettingsChanged(object sender, ExtraSlotsSettings extraSlotsSettings)
        {
#if DEBUG
            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Triggering {nameof(UiSettingsChanged)} Event. {nameof(UiSettingsChanged)} null? {UiSettingsChanged == null}");
#endif
            UiSettingsChanged?.Invoke(sender, extraSlotsSettings);
        }

        public static event EventHandler<ExtraSlotsSettings> AdvancedSettingsChanged;
        public static void RaiseAdvancedSettingsChanged(object sender, ExtraSlotsSettings extraSlotsSettings)
        {
#if DEBUG
            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Triggering {nameof(AdvancedSettingsChanged)} Event. {nameof(AdvancedSettingsChanged)} null? {AdvancedSettingsChanged == null}");
#endif
            AdvancedSettingsChanged?.Invoke(sender, extraSlotsSettings);
        }

        public static event EventHandler<ExtraSlotsSettings> MainSettingsChanged;
        public static void RaiseMainSettingsChanged(object sender, ExtraSlotsSettings extraSlotsSettings)
        {
#if DEBUG
            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Triggering {nameof(MainSettingsChanged)} Event. {nameof(MainSettingsChanged)} null? {MainSettingsChanged == null}");
#endif
            MainSettingsChanged?.Invoke(sender, extraSlotsSettings);
        }

//        public static event EventHandler<ExtraSlotsSettings> LoggerSettingsChanged;
//        public static void RaiseLoggerSettingsChanged(object sender, ExtraSlotsSettings extraSlotsSettings)
//        {
//#if DEBUG
//            UnityEngine.Debug.Log($"[{ModInfo.ModName}] - Triggering {nameof(LoggerSettingsChanged)} Event. {nameof(LoggerSettingsChanged)} null? {LoggerSettingsChanged == null}");
//#endif
//            LoggerSettingsChanged?.Invoke(sender, extraSlotsSettings);
//        }
    }
}
