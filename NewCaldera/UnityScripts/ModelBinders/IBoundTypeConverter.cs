using System;

namespace ModifAmorphic.Outward.UnityScripts
{
    public interface IBoundTypeConverter
    {
        Type GetBoundType();
        object ToBoundType();
    }
}