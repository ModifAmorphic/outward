//using HarmonyLib;
//using ModifAmorphic.Outward.Extensions;
//using ModifAmorphic.Outward.Logging;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace ModifAmorphic.Outward.ActionUI.Patches
//{
//    [HarmonyPatch(typeof(Skill))]
//    internal static class SkillPatches
//    {
//        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

//        public delegate float GetAdjustedReduceDurabilityDelegate(Item item, Character character, float durabilityLost);
//        public static Dictionary<int, GetAdjustedReduceDurabilityDelegate> GetAdjustedReduceDurability = new Dictionary<int, GetAdjustedReduceDurabilityDelegate>();

//        [HarmonyPatch(nameof(Item.ReduceDurability))]
//        [HarmonyPatch(new Type[] { typeof(float) })]
//        [HarmonyPrefix]
//        private static bool InCooldownPrefix(Skill __instance, ref float _durabilityLost)
//        {
//            try
//            {
//                //Let host character process their own durability
//                if (!__instance.IsPerishable || __instance.OwnerCharacter  == null || !__instance.OwnerCharacter.IsLocalPlayer || __instance.OwnerCharacter.IsAI || !__instance.IsInContainer || __instance.OwnerCharacter.IsHostCharacter())
//                    return;

//                var character = __instance.OwnerCharacter;
//                var lastParentTransform = __instance.GetPrivateField<Item, Transform>("m_lastParentTrans");
//                var container = __instance.GetPrivateField<Item, ItemContainer>("m_lastParentItemContainer");
                
//                //ignore items not in the owner character's stash
//                if (lastParentTransform == null || container == null || container.UID != character.Stash?.UID)
//                    return;
//#if DEBUG
//                Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ReduceDurabilityPrefix)}(): Invoked for character {character?.UID}, Item {__instance.name}, DurabilityLost {_durabilityLost}. Invoking {nameof(GetAdjustedReduceDurability)}");
//#endif

//                var playerId = character.OwnerPlayerSys.PlayerID;

//                if (GetAdjustedReduceDurability.TryGetValue(playerId, out var getAdjustedReduceDurability))
//                {
//                    var originalValue = _durabilityLost;
//                    _durabilityLost = getAdjustedReduceDurability(__instance, character, _durabilityLost);
//#if DEBUG
//                    Logger.LogTrace($"{nameof(ItemPatches)}::{nameof(ReduceDurabilityPrefix)}(): PlayerID {playerId}: Adjusted durability of item {__instance.name} from {originalValue} to {_durabilityLost}.");
//#endif
//                }
                
//            }
//            catch (Exception ex)
//            {
//                Logger.LogException($"{nameof(ItemPatches)}::{nameof(ReduceDurabilityPrefix)}(): Exception calling {nameof(GetAdjustedReduceDurability)} Delegate.", ex);
//            }
//        }
//    }
//}
