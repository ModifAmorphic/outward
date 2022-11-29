using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    public class TODMaterialGradientBinder : LateScriptBinder
    {
        public override string ScriptName => "TODMaterialGradient";

        public string ColorName = "_EmissionColor";
        public float UpdateTime = 1f;
        public List<Material> Materials = new List<Material>();
        public bool ModifyOriginalMaterial;
        public Gradient Gradient;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(ColorName), ColorName);
            BoundComponent.SetField(BoundType, nameof(UpdateTime), UpdateTime);
            BoundComponent.SetField(BoundType, nameof(Materials), Materials);
            BoundComponent.SetField(BoundType, nameof(ModifyOriginalMaterial), ModifyOriginalMaterial);
            BoundComponent.SetField(BoundType, nameof(Gradient), Gradient);
        }
    }
}
