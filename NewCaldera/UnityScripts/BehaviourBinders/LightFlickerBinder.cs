using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    public class LightFlickerBinder : LateScriptBinder
    {
        public override string ScriptName => "LightFlicker";

        public float IntensityFlicker = 0.05f;
        public float IntensityFlickerSpeed = 0.5f;
        public float PositionFlicker = 0.025f;
        public float PositionFlickerSpeed = 0.1f;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(IntensityFlicker), IntensityFlicker);
            BoundComponent.SetField(BoundType, nameof(IntensityFlickerSpeed), IntensityFlickerSpeed);
            BoundComponent.SetField(BoundType, nameof(PositionFlicker), PositionFlicker);
            BoundComponent.SetField(BoundType, nameof(PositionFlickerSpeed), PositionFlickerSpeed);
        }

    }
}
