using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class BlueprintBinder : ItemBinder
    {
        public override string ScriptName => "Blueprint";

        protected override void Init()
        {
            base.Init();

            //var initCachedInfos = BoundComponent.GetMethod(BoundType, "InitCachedInfos", new object[0]);
            //initCachedInfos.Invoke(BoundComponent, null);
        }
    }
}
