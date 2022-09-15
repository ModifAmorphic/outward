using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionMenus.Extensions
{
    internal static class GameObjectExtensions
    {
        internal static void Destroy(this GameObject gameObject)
        {
            if (Application.isEditor)
                UnityEngine.Object.DestroyImmediate(gameObject);
            else
                UnityEngine.Object.Destroy(gameObject);

            gameObject = null;
        }
    }
}
