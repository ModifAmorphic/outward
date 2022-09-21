using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionUI.Extensions
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
