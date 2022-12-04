using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    internal class AreaManager
    {
        public static object GetAreaManager() => ReflectionExtensions.GetStaticFieldValue(OutwardAssembly.Types.AreaManager, "Instance");
        public static bool TryGetCurrentArea(out AreaEnum area)
        {
            var areaManager = GetAreaManager();
            var currentArea = areaManager.GetPropertyValue(OutwardAssembly.Types.AreaManager, "CurrentArea");
            if (currentArea != null)
            {
                var sceneName = currentArea.GetFieldValue<string>(OutwardAssembly.Types.Area, "SceneName");
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    var outwardAreaId = (int)areaManager.GetMethodResult(OutwardAssembly.Types.AreaManager, "GetAreaIndexFromSceneName", sceneName);
                    area = (AreaEnum)Enum.ToObject(typeof(AreaEnum), outwardAreaId);
                    return true;
                }
            }
            area = default;
            return false;
        }
    }
}
