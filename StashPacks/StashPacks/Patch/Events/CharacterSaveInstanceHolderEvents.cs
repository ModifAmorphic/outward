using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class CharacterSaveInstanceHolderEvents
    {

        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<CharacterSaveInstanceHolder, Character> PlayerSaveLoadedAfter;

        public static void RaisePlayerSaveLoadedAfter(CharacterSaveInstanceHolder characterSaveInstanceHolder, Character character)
        {
            try
            {
                Logger?.LogTrace($"{nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaisePlayerSaveLoadedAfter)} raised. {nameof(CharacterSaveInstanceHolder)}" +
                    $".{nameof(CharacterSaveInstanceHolder.CharacterUID)} is '{characterSaveInstanceHolder?.CharacterUID}'.");
                PlayerSaveLoadedAfter?.Invoke(characterSaveInstanceHolder, character);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaisePlayerSaveLoadedAfter)}.", ex);
                if (Logger == null)
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaisePlayerSaveLoadedAfter)}:\n{ex}");
            }
        }

    }
}
