using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Linq;
using System.Reflection;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    class BagDisplayActions : MajorBagActions
    {
        public BagDisplayActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger) { }

        public override void SubscribeToEvents()
        {
            BagEvents.ShowContentBefore += ShowStashPanel;
            ItemEvents.DisplayNameAfter += GetPersonalizedBagDisplayName;
            InteractionDisplayEvents.SetInteractableBefore += SetBagInteractionActionText;
            CharacterInventoryEvents.DropBagItemBefore += (c, bag) => DoAfterBagLoaded(bag, (uid) => BagVisualizer.RemoveLanternSlot((Bag)bag));
            _instances.HostSettings.DisableBagScalingRotationChanged += (newValue) => ToggleBagScaleRotation(newValue);
            ToggleBagScaleRotation(_instances.HostSettings.DisableBagScalingRotation);
            //BagUnclaimed += OnBagUnclaimed;
        }
        private void ToggleBagScaleRotation(bool isDisabled)
        {
            if (!isDisabled)
            {
                EnableScaleRotation();
            }
            else
            {
                DisableScaleRotation();
            }
        }
        private void EnableScaleRotation()
        {
            NetworkLevelLoaderEvents.MidLoadLevelBefore += SizeBagsForNewLevel;
            CharacterInventoryEvents.DropBagItemBefore += DropBagItemBefore;
            //CharacterInventoryEvents.DropBagItemAfter += DropBagItemAfter;
            ItemContainerEvents.RefreshWeightAfter += ScaleBag;
            _instances.StashPackNet.DroppingStashPack += ScaleRotateFreezeFallingPack;
            //_instances.StashPackNet.StashPackLinkChanged += (args) => FreezeIfNotFrozen(args.bagUID);
            //ItemEvents.OnReceiveItemParentChangeRequestAfter += OnReceiveItemParentChangeRequestAfter;
        }


        private void DisableScaleRotation()
        {
            NetworkLevelLoaderEvents.MidLoadLevelBefore -= SizeBagsForNewLevel;
            CharacterInventoryEvents.DropBagItemBefore -= DropBagItemBefore;
            //CharacterInventoryEvents.DropBagItemAfter -= DropBagItemAfter;
            ItemContainerEvents.RefreshWeightAfter -= ScaleBag;
            //ItemEvents.OnReceiveItemParentChangeRequestAfter -= OnReceiveItemParentChangeRequestAfter;
            _instances.StashPackNet.DroppingStashPack -= ScaleRotateFreezeFallingPack;
            //_instances.StashPackNet.StashPackLinkChanged -= (args) => FreezeIfNotFrozen(args.bagUID);
        }

        private void SizeBagsForNewLevel(NetworkLevelLoader networkLevelLoader)
        {
            Logger.LogDebug($"{nameof(BagDisplayActions)}::{nameof(SizeBagsForNewLevel)}: Sizing bags after level load of scene {networkLevelLoader.TargetScene}.");
            
            DoAfterLevelLoaded(networkLevelLoader, () =>
            {
                if (!_instances.TryGetStashPackWorldData(out var spData))
                    return;
                var stashPacks = spData.GetAllStashPacks()?.Where(p => !p.StashBag.IsEquipped && !p.StashBag.IsInContainer);

                foreach (var p in stashPacks)
                {
                    BagVisualizer.ScaleBag(p.StashBag);
                    BagVisualizer.RemoveLanternSlot(p.StashBag);
                    if (!PhotonNetwork.isNonMasterClientInRoom)
                        BagVisualizer.FreezeBag(p.StashBag);
                }
            });
        }

        private void SetBagInteractionActionText(InteractionDisplay interactionDisplay, InputDisplay bagDisplay, InteractionTriggerBase interactionTrigger)
        {
            if (bagDisplay == null)
            {
                return;
            }

            var bag = interactionTrigger.ItemToPreview as Bag;
            if (bag != null && bag.IsStashBag())
            {
                bagDisplay.ActionText = "Take";
            }
            else if (bag != null)
            {
                bagDisplay.ActionText = LocalizationManager.Instance.GetLoc("Interaction_Item_Equip");
            }
        }

        private bool ShowStashPanel(Character character, Bag bag)
        {
            if (bag.IsUsable())
            {

                bag.Container.SpecialType = ItemContainer.SpecialContainerTypes.Stash;
                bag.Container.ShowContent(character);
                return false;
            }
            return true;
        }
        //private void OnReceiveItemParentChangeRequestAfter(Bag bag, string[] _arg2)
        //{
        //    if (!BagVisualizer.IsBagScaled(bag) && IsBagLinked(bag))
        //    {
        //        BagVisualizer.ScaleBag(bag);
        //        BagVisualizer.StandBagUp(bag);
        //        _instances.UnityPlugin.StartCoroutine(AfterBagLandedCoroutine(bag, () => BagVisualizer.FreezeBag(bag)));
        //    }
        //}

        private void ScaleRotateFreezeFallingPack((string characterUID, string bagUID) args)
        {
            _instances.TryGetStashPackWorldData(out var stashPackWorldData);
            var bag = stashPackWorldData.GetStashPack(args.bagUID)?.StashBag;

            //All Clients need to remove lantern slot and scale
            if (bag != null)
            {
                BagVisualizer.RemoveLanternSlot(bag);
                BagVisualizer.ScaleBag(bag);
            }

            //Only master needs to continully stand up the bag and freeze it wehn it falls.
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                DoAfterBagLoaded(args.bagUID, (loadedBag) =>
                    DoWhileBagFalling(loadedBag.UID
                        , (stationaryBag) => BagVisualizer.StandBagUp(stationaryBag)
                        , (stationaryBag) => BagVisualizer.FreezeBag(stationaryBag)
                        )
                );
            }
        }

        private void ScaleBag(Bag bag)
        {
            if (!BagVisualizer.IsBagScaled(bag) && IsBagLinked(bag))
            {
                BagVisualizer.ScaleBag(bag);
            }
        }

        private void DropBagItemBefore(Character character, Bag bag)
        {
            bag.DropInPlace = false;
            if (WillBagBeUsable(bag))
            {
                BagVisualizer.ScaleBag(bag);
                BagVisualizer.StandBagUp(bag);
            }
        }
        //private void DropBagItemAfter(Character character, Bag bag)
        //{
        //    if (!PhotonNetwork.isNonMasterClientInRoom)
        //    {
        //        AfterBagLandedCoroutine(bag.UID, () =>
        //        {
        //            if (IsBagLinked(bag))
        //            {
        //                BagVisualizer.FreezeBag(bag);
        //            }
        //        });
        //    }
        //}
        /// <summary>
        /// TODO: Test if this is ever necessary. More of a failsafe, but probably not really that important to freeze a bag.
        /// </summary>
        /// <param name="bagUID"></param>
        private void FreezeIfNotFrozen(string bagUID)
        {
            _instances.TryGetStashPackWorldData(out var stashPackWorldData);
            var bag = stashPackWorldData.GetStashPack(bagUID)?.StashBag;
            if (bag != null)
            {
                DoAfterBagLanded(bag.UID, () =>
                {
                    if (IsBagLinked(bag))
                    {
                        BagVisualizer.FreezeBag(bag);
                    }
                });
            }
        }

        private string GetPersonalizedBagDisplayName(Bag bag, string displayName)
        {
            //crafting menu
            if (string.IsNullOrWhiteSpace(bag.UID))
            {
                return displayName;
            }

            const string unlinkedSuffix = " *Unlinked*";

            if (!IsBagLinked(bag))
            {
                return displayName + unlinkedSuffix;
            }

            var player = GetPlayerSystem(bag.PreviousOwnerUID);

            if (player == default)
            {
                return displayName + unlinkedSuffix;
            }

            return player.ControlledCharacter.Name + "'s " + displayName;
        }
        //protected void OnBagUnclaimed(string characterUID, Bag bag)
        //{
        //    Logger.LogTrace($"{nameof(BagDisplayActions)}::{nameof(OnBagUnclaimed)}: Triggered for character ({characterUID}) and {bag.Name} ({bag.UID}).");
        //    if (BagVisualizer.IsBagScaled(bag))
        //    {
        //        BagVisualizer.UnscaleBag(bag);
        //        BagVisualizer.ThawBag(bag);
        //        Logger.LogDebug($"{nameof(BagDisplayActions)}::{nameof(OnBagUnclaimed)}: Reset scaling and unfroze character's ({characterUID}) {bag.Name} ({bag.UID}).");
        //    }
        //}
        private bool IsBagLinked(Bag bag)
        {
            var player = GetPlayerSystem(bag.PreviousOwnerUID);
            if (player == default
                || IsHomeStashInWorld(player.ControlledCharacter, bag))
            {
                //Logger.LogTrace($"{nameof(BagDisplayActions)}::{nameof(IsBagLinked)}: Owner '{bag.PreviousOwnerUID}' of {bag.Name} ({bag.UID}) is not in the game. Bag is not linked.");
                return false;
            }

            return BagStateService.IsLinked(bag.UID) && IsCurrentSceneStashPackEnabled();
        }
        private bool WillBagBeUsable(Bag bag)
        {
            if (bag.OwnerCharacter == null || !IsPlayerCharacterInGame(bag.OwnerCharacter.UID)
                || (BagStateService.IsBagDisabled(bag.UID) && IsLocalPlayerCharacter(bag.OwnerCharacter.UID))
                || !IsCurrentSceneStashPackEnabled())
            {
                return false;
            }

            return true;
        }
    }
}
