using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph
{
    [HarmonyPatch(typeof(CharacterUI))]
    internal static class CharacterUIPatches
    {
        public static event Action<CharacterUIAwakeFields> AwakeBefore;
        [HarmonyPatch("Awake", MethodType.Normal)]
        [HarmonyPrefix]
        private static void AwakePrefix(CharacterUI __instance, ref Type[] ___MenuTypes, ref MenuPanel[] ___m_menus, ref MenuTab[] ___m_menuTabs)
        {
            AwakeBefore?.Invoke(new CharacterUIAwakeFields()
            {
                CharacterUI = __instance,
                MenuPanels = ___m_menus,
                MenuTabs = ___m_menuTabs,
                MenuTypes = ___MenuTypes
            });

        }

        public static event Action<CharacterUI> StartAfter;
        [HarmonyPatch("Start", MethodType.Normal)]
        [HarmonyPostfix]
        private static void StartPostfix(CharacterUI __instance)
        {
            StartAfter?.Invoke(__instance);
        }

        public static event Action<CharacterUI> ShowGameplayPanelBefore;
        [HarmonyPatch(nameof(CharacterUI.ShowGameplayPanel), MethodType.Normal)]
        [HarmonyPrefix]
        private static void ShowGameplayPanelPostfix(CharacterUI __instance)
        {
            ShowGameplayPanelBefore?.Invoke(__instance);
        }
    }
    internal class CharacterUIAwakeFields
    {
        public CharacterUI CharacterUI;
        public Type[] MenuTypes;
        public MenuPanel[] MenuPanels;
        public MenuTab[] MenuTabs;
    }
}
