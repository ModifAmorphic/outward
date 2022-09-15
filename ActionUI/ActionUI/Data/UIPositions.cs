using System;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public class UIPositions : IEquatable<UIPositions>, IComparable<UIPositions>
    {
        public string TransformPath { get; set; }
        public ActionMenus.UIPosition OriginPosition { get; set; }
        public ActionMenus.UIPosition ModifiedPosition { get; set; }

        public int CompareTo(UIPositions other)
        {
            if (other == null) return 1;

            return TransformPath.CompareTo(other.TransformPath);
        }

        public bool Equals(UIPositions other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return this.TransformPath.Equals(other.TransformPath, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if (ReferenceEquals(obj, null) || !(obj is UIPositions other))
            {
                return false;
            }
            else
            {
                return this.Equals(other);
            }
        }

        public static bool operator ==(UIPositions pos1, UIPositions pos2)
        {
            if (ReferenceEquals(pos1, null) && ReferenceEquals(pos1, null))
                return true;
            else if (ReferenceEquals(pos1, null) || ReferenceEquals(pos2, null))
                return false;

            return pos1.Equals(pos2);
        }

        public static bool operator !=(UIPositions pos1, UIPositions pos2)
        {
            if (ReferenceEquals(pos1, null) && ReferenceEquals(pos1, null))
                return false;
            else if (ReferenceEquals(pos1, null) || ReferenceEquals(pos2, null))
                return true;

            return !pos1.Equals(pos2);
        }

        public override int GetHashCode() => GetHashCode(this);

        public int GetHashCode(UIPositions uiPosition) => uiPosition.TransformPath.GetHashCode();
    }
}
