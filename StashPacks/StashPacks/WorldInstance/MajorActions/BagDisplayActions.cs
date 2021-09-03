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
            CharacterInventoryEvents.DropBagItemBefore += (c, bag) => DoAfterBagLoaded(bag.UID, (uid) => RemoveLanternSlot(bag));
            _instances.HostSettings.DisableBagScalingRotationChanged += (newValue) => ToggleBagScaleRotation(newValue);
            ToggleBagScaleRotation(_instances.HostSettings.DisableBagScalingRotation);
            //BagUnclaimed += OnBagUnclaimed;
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

        private void RemoveLanternSlot(Bag bag)
        {
            var m_loadedVisualField = typeof(Bag).GetField("m_loadedVisual", BindingFlags.NonPublic | BindingFlags.Instance);

            var loadedVisual = m_loadedVisualField.GetValue(bag) as ItemVisual;

            if (loadedVisual == null)
            {
                return;
            }

            var bagSlotVisuals = loadedVisual.GetComponentsInChildren<BagSlotVisual>();
            if (bagSlotVisuals != null)
            {
                var lanternSlots = bagSlotVisuals.Where(sv => sv.AuthorizedTypes.Contains(Item.BagCategorySlotType.Lantern)).ToList();
                for (var i = 0; i < lanternSlots.Count; i++)
                {
                    Logger.LogTrace($"{nameof(BagDisplayActions)}::{nameof(RemoveLanternSlot)}: " +
                        $"Destroying Lantern Slot {lanternSlots[i].name} of bag {bag.DisplayName} ({bag.UID})");
                    UnityEngine.Object.Destroy(lanternSlots[i]);
                }
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
            CharacterInventoryEvents.DropBagItemBefore += DropBagItemBefore;
            CharacterInventoryEvents.DropBagItemAfter += DropBagItemAfter;
            ItemContainerEvents.RefreshWeightAfter += ScaleBag;
            _instances.StashPackNet.DroppingStashPack += ScaleRotateFreezeOthersStashPack;
            _instances.StashPackNet.StashPackLinkChanged += (args) => FreezeIfNotFrozen(args.bagUID);
            //ItemEvents.OnReceiveItemParentChangeRequestAfter += OnReceiveItemParentChangeRequestAfter;
        }

        private void DisableScaleRotation()
        {
            CharacterInventoryEvents.DropBagItemBefore -= DropBagItemBefore;
            CharacterInventoryEvents.DropBagItemAfter -= DropBagItemAfter;
            ItemContainerEvents.RefreshWeightAfter -= ScaleBag;
            //ItemEvents.OnReceiveItemParentChangeRequestAfter -= OnReceiveItemParentChangeRequestAfter;
            _instances.StashPackNet.DroppingStashPack -= ScaleRotateFreezeOthersStashPack;
            _instances.StashPackNet.StashPackLinkChanged -= (args) => FreezeIfNotFrozen(args.bagUID);
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

        private void ScaleRotateFreezeOthersStashPack((string characterUID, string bagUID) args)
        {
            DoAfterBagLoaded(args.bagUID, (bag) =>
            {
                if (!BagVisualizer.IsBagScaled(bag) && IsBagLinked(bag))
                {
                    RemoveLanternSlot(bag);
                    BagVisualizer.ScaleBag(bag);
                    if (!PhotonNetwork.isNonMasterClientInRoom)
                    {
                        BagVisualizer.StandBagUp(bag);
                        _instances.UnityPlugin.StartCoroutine(AfterBagLandedCoroutine(bag, () => BagVisualizer.FreezeBag(bag)));
                    }
                }
            });

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
        private void DropBagItemAfter(Character character, Bag bag)
        {
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                _instances.UnityPlugin.StartCoroutine(AfterBagLandedCoroutine(bag, () =>
                {
                    if (IsBagLinked(bag))
                    {
                        BagVisualizer.FreezeBag(bag);
                    }
                }));
            }
        }

        private void FreezeIfNotFrozen(string bagUID)
        {
            _instances.TryGetStashPackWorldData(out var stashPackWorldData);
            var bag = stashPackWorldData.GetStashPack(bagUID)?.StashBag;
            if (bag != null)
            {
                _instances.UnityPlugin.StartCoroutine(AfterBagLandedCoroutine(bag, () =>
                {
                    if (IsBagLinked(bag))
                    {
                        BagVisualizer.FreezeBag(bag);
                    }
                }));
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
