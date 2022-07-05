using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class GameObjectExtensions
    {
        public static string GetPath(this GameObject gameObject)
        {
            var path = gameObject.name;
            var parent = gameObject.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return "/" + path;
        }
        /// <summary>
        /// Sets the <paramref name="gameObject"/> and all if its children to the <paramref name="state"/>.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> and objects children on which to invoke <see cref="GameObject.SetActive(bool)"/> on.</param>
        /// <param name="state"></param>
        /// <returns>A collection of GameObjects who had their activeSelf changed.</returns>
        public static IList<GameObject> SetActiveRecursive(this GameObject gameObject, bool state)
        {
            var changed = new List<GameObject>();

            if (gameObject.activeSelf != state)
                changed.Add(gameObject);

            //if deactivating, then set parent to inactive first.
            if (!state)
                gameObject.SetActive(state);

            foreach (Transform child in gameObject.transform)
            {
                changed.AddRange(SetActiveRecursive(child.gameObject, state));
            }

            //if activating, then set children to active prior to parent.
            if (state)
                gameObject.SetActive(state);

            return changed;
        }

        public static GameObject DeCloneNames(this GameObject gameObject, bool recursive = false)
        {
            //gameObject.name = gameObject.name.Replace("(Clone)", "");
            //gameObject.transform.name = gameObject.transform.name.Replace("(Clone)", "");

            //foreach (Transform t in gameObject.transform)
            //{
            //    if (recursive)
            //    {
            //        t.gameObject.DeCloneNames(recursive);
            //    }
            //    else
            //    {
            //        t.gameObject.name = gameObject.name.Replace("(Clone)", "");
            //        t.name = gameObject.transform.name.Replace("(Clone)", "");
            //    }
            //}

            return gameObject;
        }
    }
}
