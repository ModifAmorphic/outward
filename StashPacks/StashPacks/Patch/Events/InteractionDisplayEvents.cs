using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class InteractionDisplayEvents
    {
        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<InteractionDisplay, InputDisplay, InteractionTriggerBase> SetInteractableBefore;

        public static void RaiseSetInteractableBefore(InteractionDisplay interactionDisplay, ref InputDisplay interactionBag, InteractionTriggerBase interactionTrigger)
        {
            try
            {
                SetInteractableBefore?.Invoke(interactionDisplay, interactionBag, interactionTrigger);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(InteractionDisplayEvents)}::{nameof(RaiseSetInteractableBefore)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(InteractionDisplayEvents)}::{nameof(RaiseSetInteractableBefore)}:\n{ex}");
                }
            }
        }
    }
}
