using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class BuildProcessVisualBinder : LateScriptBinder
    {
        public override string ScriptName => "BuildProcessVisual";

        public int ContructionPhaseIndex;
        public Transform Visuals;
        public Transform NPCs;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(ContructionPhaseIndex), ContructionPhaseIndex);
            BoundComponent.SetField(BoundType, nameof(Visuals), Visuals);
            BoundComponent.SetField(BoundType, nameof(NPCs), NPCs);
        }
    }
}
