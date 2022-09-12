using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus
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
