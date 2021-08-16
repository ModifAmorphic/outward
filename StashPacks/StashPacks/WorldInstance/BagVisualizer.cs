using ModifAmorphic.Outward.StashPacks.Settings;
using System;
using System.Collections.Generic;
using System.Text;
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
        public static void ScaleBag(Bag bag)
        {
            var bagVisuals = GetBagSettings(bag);
            var bagTransform = bag.gameObject.transform;

            bagTransform.localScale = bagVisuals.Scale;
        }
        public static void BagLanded(Bag bag, Transform characterTransform)
        {
            //var bagVisuals = GetBagSettings(bag);

            //var bagTransform = bag.gameObject.transform;

            //bagTransform.LookAt(characterTransform.position);
            //bagTransform.eulerAngles = new Vector3(bagVisuals.RotationToWorld.x, bagTransform.eulerAngles.y + bagVisuals.RotationToWorld.y - 180, bagTransform.eulerAngles.z + bagVisuals.RotationToWorld.z);

            //var itemVisual = bag.CurrentVisual;
            //var meshRenderer = itemVisual.GetComponentInChildren<MeshRenderer>();
            //if (meshRenderer != null)
            //{
            //    var bagSize = Matrix4x4.Scale(bagTransform.localScale) * meshRenderer.bounds.size;
            //    var boxCollider = itemVisual.GetComponent<BoxCollider>();

            //    if (boxCollider != null)
            //    {
            //        boxCollider.size = new Vector3(bagSize.x, bagSize.y, bagSize.z);
            //    }
            //}

            var rigidBody = bag.GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeAll; //RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        }
        public static void BagLoaded(Bag bag)
        {
            ScaleBag(bag);
            var bagVisuals = GetBagSettings(bag);


            var bagTransform = bag.gameObject.transform;


            bagTransform.eulerAngles = new Vector3(bagVisuals.RotationToWorld.x, bagTransform.eulerAngles.y, bagTransform.eulerAngles.z);

            //var itemVisual = bag.CurrentVisual;
            //var meshRenderer = itemVisual.GetComponentInChildren<MeshRenderer>();
            //if (meshRenderer != null)
            //{
            //    var bagSize = Matrix4x4.Scale(bagTransform.localScale) * meshRenderer.bounds.size;
            //    var boxCollider = itemVisual.GetComponent<BoxCollider>();

            //    if (boxCollider != null)
            //    {
            //        boxCollider.size = new Vector3(bagSize.x, bagSize.y, bagSize.z);
            //    }
            //}

            var rigidBody = bag.GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeAll; //RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        }
        public static StashBagVisual GetBagSettings(Bag bag)
        {
            var area = StashPacksConstants.StashBackpackAreas[bag.ItemID];
            return StashPacksConstants.StashBagVisuals[area];
        }
    }
}
