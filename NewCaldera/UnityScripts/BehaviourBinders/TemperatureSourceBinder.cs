using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    public class TemperatureSourceBinder : LateScriptBinder
    {
        public override string ScriptName => "TemperatureSource";

        //[HideInInspector]
        public List<int> TemperaturePerIncrement = new List<int>();
        //[HideInInspector]
        public List<Vector2> DistanceRanges = new List<Vector2>();
        public bool DisableInInventory;


        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(TemperaturePerIncrement), TemperaturePerIncrement);
            BoundComponent.SetField(BoundType, nameof(DistanceRanges), DistanceRanges);
            BoundComponent.SetField(BoundType, nameof(DisableInInventory), DisableInInventory);
        }
    }
}
