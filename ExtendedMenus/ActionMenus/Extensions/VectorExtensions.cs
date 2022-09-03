using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Extensions
{
    public static class VectorExtensions
    {
        public static UIPosition2D ToUIPosition2D(this Vector2 vector)
        {
            return new UIPosition2D()
            {
                X = vector.x,
                Y = vector.y,
            };
        }

        public static UIPosition2D ToUIPosition2D(this Vector3 vector)
        {
            return new UIPosition2D()
            {
                X = vector.x,
                Y = vector.y,
            };
        }
    }
}
