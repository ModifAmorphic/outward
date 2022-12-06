using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class BuildingVisualBinder : ItemVisualBinder
    {
        public override string ScriptName => "BuildingVisual";
        [Header("Building")]
        public Transform[] Phases = new Transform[0];
        public Transform[] Upgrades = new Transform[0];
        public Transform LedgerPosTrans;
        public BuildProcessVisualBinder[] Processes = new BuildProcessVisualBinder[0];
        public Transform[] IgnoreOnPreviewVisuals = new Transform[0];
        public Transform[] DisableOnUpgrade = new Transform[0];
        public Transform[] EnableOnContinueAndBuildingIsOperational = new Transform[0];

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(Phases), Phases);
            BoundComponent.SetField(BoundType, nameof(Upgrades), Upgrades);
            BoundComponent.SetField(BoundType, nameof(LedgerPosTrans), LedgerPosTrans);

            if (Processes != null && Processes.Any())
            {
                var reqs = Array.CreateInstance(Processes.First().BoundType, Processes.Length);
                for (int i = 0; i < Processes.Length; i++)
                {
                    reqs.SetValue(Processes[i].BoundComponent, i);
                }
                BoundComponent.SetField(BoundType, nameof(Processes), reqs);
            }

            BoundComponent.SetField(BoundType, nameof(IgnoreOnPreviewVisuals), IgnoreOnPreviewVisuals);
            BoundComponent.SetField(BoundType, nameof(DisableOnUpgrade), DisableOnUpgrade);
            BoundComponent.SetField(BoundType, nameof(EnableOnContinueAndBuildingIsOperational), EnableOnContinueAndBuildingIsOperational);
            var awakeInit = BoundComponent.GetMethod(BoundType, "AwakeInit", new object[0]);
            awakeInit.Invoke(BoundComponent, new object[0]);
        }
    }
}
