using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    internal class LightQualityBinder : LateScriptBinder
    {
        public override string ScriptName => "LightQuality";

        public int CastShadowsLevel = 3;
        public Vector2 EnabledDistances = new Vector2(-1f, -1f);
        public bool ClipCheck;
        public bool Break;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(CastShadowsLevel), CastShadowsLevel);
            BoundComponent.SetField(BoundType, nameof(EnabledDistances), EnabledDistances);
            BoundComponent.SetField(BoundType, nameof(ClipCheck), ClipCheck);
            BoundComponent.SetField(BoundType, nameof(Break), Break);
        }

    }
}
