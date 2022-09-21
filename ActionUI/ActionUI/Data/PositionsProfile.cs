using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public class PositionsProfile
    {
        public List<UIPositions> Positions { get; set; } = new List<UIPositions>();

        public void AddOrReplacePosition(UIPositions position)
        {
            if (Positions.Contains(position))
                Positions.Remove(position);
            Positions.Add(position);
        }

        public bool RemovePosition(UIPositions position) => Positions.Remove(position);
    }
}
