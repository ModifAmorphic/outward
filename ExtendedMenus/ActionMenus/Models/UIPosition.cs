using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public class UIPosition : IEquatable<UIPosition>
    {
        public UIPosition2D Position { get; set; }
        public UIPosition2D AnchoredPosition { get; set; }
        public UIPosition2D AnchoredMin { get; set; }
        public UIPosition2D AnchoredMax { get; set; }
        public UIPosition2D OffsetMin { get; set; }
        public UIPosition2D OffsetMax { get; set; }
        public UIPosition2D Pivot { get; set; }

        public bool Equals(UIPosition other)
        {
            if (other == null) return false;

            return AnchoredPosition.Equals(other.AnchoredPosition);
                //&& AnchoredPosition.Equals(other.AnchoredPosition)
                //&& AnchoredMin.Equals(other.AnchoredMin)
                //&& AnchoredMax.Equals(other.AnchoredMax)
                //&& OffsetMin.Equals(other.OffsetMin)
                //&& OffsetMax.Equals(other.OffsetMax)
                //&& Pivot.Equals(other.Pivot);
        }

        public override bool Equals(System.Object obj)
        {
            //Check for null and compare run-time types.
            if (ReferenceEquals(obj, null) || !(obj is UIPosition other))
            {
                return false;
            }
            else
            {
                return this.Equals(other);
            }
        }

        public static bool operator ==(UIPosition pos1, UIPosition pos2)
        {
            if (ReferenceEquals(pos1, null) && ReferenceEquals(pos1, null))
                return true;
            else if (ReferenceEquals(pos1, null) || ReferenceEquals(pos2, null))
                return false;

            return pos1.Equals(pos2);
        }

        public static bool operator !=(UIPosition pos1, UIPosition pos2)
        {
            if (ReferenceEquals(pos1, null) && ReferenceEquals(pos1, null))
                return false;
            else if (ReferenceEquals(pos1, null) || ReferenceEquals(pos2, null))
                return true;

            return !pos1.Equals(pos2);
        }
    }
    public class UIPosition2D : IEquatable<UIPosition2D>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public bool Equals(UIPosition2D other)
        {
            if (other == null) return false;

            return Mathf.Approximately(X, other.X) && Mathf.Approximately(Y, other.Y);
        }

        public override bool Equals(System.Object obj)
        {
            //Check for null and compare run-time types.
            if (ReferenceEquals(obj, null) || !(obj is UIPosition2D other))
            {
                return false;
            }
            else
            {
                return this.Equals(other);
            }
        }

        public static bool operator ==(UIPosition2D pos1, UIPosition2D pos2)
        {
            if (ReferenceEquals(pos1, null) && ReferenceEquals(pos1, null))
                return true;
            else if (ReferenceEquals(pos1, null) || ReferenceEquals(pos2, null))
                return false;

            return pos1.Equals(pos2);
        }

        public static bool operator !=(UIPosition2D pos1, UIPosition2D pos2)
        {
            if (ReferenceEquals(pos1, null) && ReferenceEquals(pos1, null))
                return false;
            else if (ReferenceEquals(pos1, null) || ReferenceEquals(pos2, null))
                return true;

            return !pos1.Equals(pos2);
        }

        public override int GetHashCode() => GetHashCode(this);

        private const double upperDouble = double.MaxValue / 2;
        public int GetHashCode(UIPosition2D uiPosition) => (((double)uiPosition.X) + upperDouble + ((double)uiPosition.Y)).GetHashCode();
    }
}
