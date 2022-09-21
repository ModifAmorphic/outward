using System.Collections.Generic;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public enum BarPositions
    {
        Top,
        Right,
        Bottom,
        Left
    }
    public interface IBarProgress
    {
        BarPositions BarPosition { get; }
        List<ColorRange> ColorRanges { get; }
        bool IsEnabled { get; }
        float GetProgress();
    }
}
