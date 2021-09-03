using HarmonyLib;
using ModifAmorphic.Outward.StashPacks.Patch.Events;

namespace ModifAmorphic.Outward.StashPacks.Patch
{
    [HarmonyPatch(typeof(InteractionDisplay))]
    internal static class InteractionDisplayPatches
    {
        [HarmonyPatch(nameof(InteractionDisplay.SetInteractable), MethodType.Normal)]
        [HarmonyPrefix]
        private static void SetInteractable(InteractionDisplay __instance, ref InputDisplay ___m_interactionBag, InteractionTriggerBase _interactionTrigger)
        {
            InteractionDisplayEvents.RaiseSetInteractableBefore(__instance, ref ___m_interactionBag, _interactionTrigger);
        }
    }
}
