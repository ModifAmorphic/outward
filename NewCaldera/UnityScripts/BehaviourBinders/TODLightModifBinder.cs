using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    public class TODLightModifBinder : LateScriptBinder
    {
        public override string ScriptName => "TODLightModif";

        public float UpdateTime = 1f;
        public Gradient ColorGradient;
        public Transform TransToRotate;
        public Vector3[] Rotations = new Vector3[3];


        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(UpdateTime), UpdateTime);
            BoundComponent.SetField(BoundType, nameof(ColorGradient), ColorGradient);
            BoundComponent.SetField(BoundType, nameof(TransToRotate), TransToRotate);
            BoundComponent.SetField(BoundType, nameof(Rotations), Rotations);
        }
    }
}
