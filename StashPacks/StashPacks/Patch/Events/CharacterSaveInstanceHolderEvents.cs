using ModifAmorphic.Outward.Logging;
using System;

namespace ModifAmorphic.Outward.StashPacks.Patch.Events
{
    internal static class CharacterSaveInstanceHolderEvents
    {

        private static IModifLogger Logger => LoggerFactory?.Invoke();
        public static Func<IModifLogger> LoggerFactory { get; set; }

        public static event Action<CharacterSaveInstanceHolder, Character> PlayerSaveLoadedAfter;
        public static event Action<CharacterSaveInstanceHolder> SaveAfter;

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
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaisePlayerSaveLoadedAfter)}:\n{ex}");
                }
            }
        }

        public static void RaiseSaveAfter(CharacterSaveInstanceHolder characterSaveInstanceHolder)
        {
            try
            {
                Logger?.LogTrace($"{nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaiseSaveAfter)} raised. {nameof(CharacterSaveInstanceHolder)}" +
                    $".{nameof(CharacterSaveInstanceHolder.CharacterUID)} is '{characterSaveInstanceHolder?.CharacterUID}'.");
                SaveAfter?.Invoke(characterSaveInstanceHolder);
            }
            catch (Exception ex)
            {
                Logger?.LogException($"Exception in {nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaiseSaveAfter)}.", ex);
                if (Logger == null)
                {
                    UnityEngine.Debug.LogError($"Exception in {nameof(CharacterSaveInstanceHolderEvents)}::{nameof(RaiseSaveAfter)}:\n{ex}");
                }
            }
        }

    }
}
