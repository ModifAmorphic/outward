using ModifAmorphic.Outward.Internal;
using System.Collections.Generic;

namespace ModifAmorphic.Outward.Extensions
{
    public static class ControlsInputExtensions
    {
        public static Dictionary<int, RewiredInputs> GetPlayerInputManager(this ControlsInput controlsInput)
        {
            return ReflectUtil.GetReflectedPrivateStaticField<Dictionary<int, RewiredInputs>, ControlsInput>(ControlsInputFieldNames.PlayerInputManager, controlsInput);
        }
        public static void SetPlayerInputManager(this ControlsInput controlsInput, Dictionary<int, RewiredInputs> value)
        {
            ReflectUtil.SetReflectedPrivateField(value, ControlsInputFieldNames.PlayerInputManager, controlsInput);
        }

        static class ControlsInputFieldNames
        {
            public const string PlayerInputManager = "m_playerInputManager";
        }
    }
}
