using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Extensions
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

        public static void DeCloneNames(this GameObject gameObject, bool recursive = false)
        {
            gameObject.name = gameObject.name.Replace("(Clone)", "");
            gameObject.transform.name = gameObject.transform.name.Replace("(Clone)", "");

            foreach (Transform t in gameObject.transform)
            {
                if (recursive)
                {
                    t.gameObject.DeCloneNames(recursive);
                }
                else
                {
                    t.gameObject.name = gameObject.name.Replace("(Clone)", "");
                    t.name = gameObject.transform.name.Replace("(Clone)", "");
                }
            }
        }
    }
}
