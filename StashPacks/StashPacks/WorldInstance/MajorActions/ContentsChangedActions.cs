using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    internal class ContentsChangedActions : MajorBagActions
    {
        public ContentsChangedActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
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

            if (!IsCurrentSceneStashPackEnabled())
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Current Scene is not StashPack Enabled. Disabling stashpack functionality for bag {bag.Name} ({bag.UID}).");
                BagStateService.DisableBag(bag.UID);
                return;
            }

            //check if in someone's inventory
            if (!bag.IsUsable())
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' is not in an updatable statues. May be unowned, equipped or in another container. Ignoring content changes.");
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
            if (BagStateService.IsBagDisabled(bag.UID))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Bag '{bag.Name}' ({bag.UID}) has StashBag functionalty disabled. Ignoring content changes.");
                return;
            }

            if (character == null || !character.IsLocalPlayer)
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Player Character '{characterUID}' is not a local player. Ignoring content changes for bag '{bag.Name}' ({bag.UID}) " +
                    $"and disabling.");
                BagStateService.DisableBag(bag.UID);
                return;
            }

            //Check if tracked for state saves
            if (!bagStates.IsContentChangesTracked(bag.ItemID))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Ignoring content changes. Bag '{bag.Name}' ({bag.UID}) currently has state tracking disabled.");
                return;
            }

            //check if character is host, and bag is in home area (disable if so)
            if (IsHomeStashInWorld(character, bag))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Character '{characterUID}' is hosting the game and is in '{bag.Name}' ({bag.UID}) home area {GetBagAreaEnum(bag).GetName()}. StashBag functionalty disabled for bag.");
                BagStateService.DisableBag(bag.UID);
                return;
            }

            //made it. save the state.
            if (!bagStates.TrySaveState(bag))
            {
                Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Unexpected problem saving bag '{bag.Name}' ({bag.UID}) for Character '{characterUID}'. Disabling StashBag functionalty for bag.");
                BagStateService.DisableBag(bag.UID);
            }

            Logger.LogDebug($"{nameof(ContentsChangedActions)}::{nameof(ContentsChanged)}: Saved current state of bag '{bag.Name}' ({bag.UID}) for Character '{characterUID}'.");
        }
    }
}
