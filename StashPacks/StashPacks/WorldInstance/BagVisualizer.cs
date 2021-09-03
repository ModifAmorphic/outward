using ModifAmorphic.Outward.StashPacks.Settings;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.WorldInstance
{
    static class BagVisualizer
    {
        public static void BagDropping(Bag bag, Transform characterTransform)
        {
            var bagVisuals = GetBagSettings(bag);
            var bagTransform = bag.gameObject.transform;

            //bagTransform.localScale = bagVisuals.Scale;
            bagTransform.position = (characterTransform.forward * 1.5f) + characterTransform.position;
            bagTransform.position = bagTransform.up * 1.8f + bagTransform.position;
            bagTransform.LookAt(characterTransform.position);
            var flippedY = bagTransform.eulerAngles.y - 180 < 0 ? bagTransform.eulerAngles.y + 180 : bagTransform.eulerAngles.y - 180;
            bagTransform.eulerAngles = new Vector3(bagVisuals.RotationToWorld.x, flippedY, bagTransform.eulerAngles.z + bagVisuals.RotationToWorld.z);

        }
        public static void StandBagUp(Bag bag)
        {
            var bagTransform = bag.gameObject?.transform;
            if (bagTransform != null)
            {
                if (bagTransform.eulerAngles.x < 270f - 10f || bagTransform.eulerAngles.x > 270f + 10f)
                {
                    bagTransform.eulerAngles = new Vector3(270f, bagTransform.eulerAngles.y, bagTransform.eulerAngles.z);
                    var rigidBody = bag.GetComponent<Rigidbody>();
                    if (rigidBody != null)
                    {
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionX;
                    }
                }
            }
        }
        public static void ScaleBag(Bag bag)
        {
            var bagVisuals = GetBagSettings(bag);
            var bagTransform = bag.gameObject.transform;

            bagTransform.localScale = bagVisuals.Scale;
        }
        public static void UnscaleBag(Bag bag)
        {
            var bagTransform = bag.gameObject.transform;

            bagTransform.localScale = new Vector3(1f, 1f, 1f);
        }
        public static void FreezeBag(Bag bag)
        {
            var rigidBody = bag.GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeAll; //RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
        public static void ThawBag(Bag bag)
        {
            var rigidBody = bag.GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.None; //RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
        public static bool IsBagScaled(Bag bag)
        {
            var bagVisuals = GetBagSettings(bag);
            var bagTransform = bag.gameObject.transform;

            return bagTransform.localScale == bagVisuals.Scale;
        }
        public static StashBagVisual GetBagSettings(Bag bag)
        {
            var area = StashPacksConstants.StashBackpackAreas[bag.ItemID];
            return StashPacksConstants.StashBagVisuals[area];
        }
    }
}
