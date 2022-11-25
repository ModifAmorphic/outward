using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts
{
    [Serializable]
    public class BuildingResourceValuesBinder : IBoundTypeConverter
    {
        public int Funds;
        public int Food;
        public int Timber;
        public int Stone;

        public static Type GetBindingType() => OutwardAssembly.Types.BuildingResourceValues;
        public Type GetBoundType() => GetBindingType();

        public object ToBoundType()
        {
            var resourceValues = Activator.CreateInstance(GetBoundType(), Funds, Food, Timber, Stone);

            return resourceValues;
        }
    }
}
