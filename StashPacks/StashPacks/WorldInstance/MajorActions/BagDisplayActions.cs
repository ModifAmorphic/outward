using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using ModifAmorphic.Outward.StashPacks.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance.MajorActions
{
    class BagDisplayActions : MajorBagActions
    {
        public BagDisplayActions(InstanceFactory instances, Func<IModifLogger> getLogger) : base(instances, getLogger)
        {
        }

        public override void SubscribeToEvents()
        {
            ItemEvents.DisplayNameAfter += GetPersonalizedBagDisplayName;
            CharacterInventoryEvents.DropBagItemBefore += DropBagItemBefore;
            CharacterInventoryEvents.DropBagItemAfter += DropBagItemAfter;
        }
        private void DropBagItemBefore(Character character, Bag bag)
        {
            BagVisualizer.ScaleBag(bag);
            bag.DropInPlace = false;
            BagVisualizer.StandBagUp(bag);
        }
        private void DropBagItemAfter(Character character, Bag bag)
        {
            _instances.UnityPlugin.StartCoroutine(AfterBagLandedCoroutine(bag, () => BagVisualizer.FreezeBag(bag)));
        }

        private string GetPersonalizedBagDisplayName(Bag bag, string displayName)
        {
            var characterUID = bag.OwnerCharacter?.UID.ToString() ?? bag.PreviousOwnerUID;
            if (string.IsNullOrEmpty(characterUID))
                return displayName;

            var player = Global.Lobby.PlayersInLobby.FirstOrDefault(ps => ps.CharUID == characterUID);
            if (player == default)
                return displayName;

            if (displayName.StartsWith(player.ControlledCharacter.Name + "'s "))
                return displayName;

            return player.ControlledCharacter.Name + "'s " + displayName;
        }
        protected void UnpersonalizeBag(ref Bag bag)
        {
            bag.GetType().GetField($"m_localizedName", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(bag, null);
        }
    }
}
