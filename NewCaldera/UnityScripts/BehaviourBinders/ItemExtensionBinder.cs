using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    public abstract class ItemExtensionBinder : LateScriptBinder
    {
        public override string ScriptName => "ItemExtension";

        public bool Savable;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(Savable), Savable);
        }
    }
}
