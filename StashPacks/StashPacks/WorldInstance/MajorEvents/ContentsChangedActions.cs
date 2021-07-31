using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorEvents
{
    internal class ContentsChangedActions : MajorBagActions
    {
        public ContentsChangedActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public void SubscribeToEvents()
        {
            ItemContainerEvents.RefreshWeightAfter += ContentsChanged;
        }

        private void ContentsChanged(Bag bag)
        {
            if (!IsWorldLoaded())
            {
                Logger.LogTrace($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: World is still loading. Ignoring content changes for Bag '{bag.Name}' ({bag.UID})");
                return;
            }

            //check if in someone's inventory
            if (bag.OwnerCharacter != null)
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' is currently owned by Character " +
                    $"UID {bag.OwnerCharacter.UID}. Bag is likely equipped or in character's inventory. Ignoring content changes.");
                return;
            }

            //check if owned by anyone
            if (string.IsNullOrWhiteSpace(bag.PreviousOwnerUID))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' ({bag.UID}) has no PreviousOwnerUID. Ignoring content changes.");
                return;
            }

            var characterUID = bag.PreviousOwnerUID;
            var character = CharacterManager.Instance.GetCharacter(characterUID);
            var bagStates = _instances.GetBagStateService(characterUID);

            //Check if disabled
            if (bagStates.IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' ({bag.UID}) has StashBag functionalty disabled. Ignoring content changes.");
                return;
            }

            if (!character.IsLocalPlayer)
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Player Character '{characterUID}' is not a local player. Ignoring content changes for bag '{bag.Name}' ({bag.UID}) " +
                    $"and disabling.");
                bagStates.DisableBag(bag.UID);
                return;
            }

            //Check if ever received stash update for state saves
            if (!bagStates.IsTracked(bag.ItemID))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' ({bag.UID}) has never received a sync from its home stash. Ignoring content changes.");
                return;
            }

            //Check if tracked for state saves
            if (!bagStates.IsTracked(bag.ItemID))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Ignoring content changes. Bag '{bag.Name}' ({bag.UID}) currently has state tracking disabled.");
                return;
            }

            //check if character is host, and bag is in home area (disable if so)
            if (IsHost(CharacterManager.Instance.GetCharacter(characterUID)) && GetBagAreaEnum(bag) == GetCurrentAreaEnum())
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Character '{characterUID}' is hosting the game and is in '{bag.Name}' ({bag.UID}) home area {GetBagAreaEnum(bag).GetName()}. StashBag functionalty disabled for bag.");
                bagStates.DisableBag(bag.UID);
                return;
            }

            //made it. save the state.
            if (!bagStates.TrySaveState(bag))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Unexpected problem saving bag '{bag.Name}' ({bag.UID}) for Character '{characterUID}'. Disabling StashBag functionalty for bag.");
                bagStates.DisableBag(bag.UID);
            }

            Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Saved current state of bag '{bag.Name}' ({bag.UID}) for Character '{characterUID}'.");
        }
    }
}
