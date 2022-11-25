using ModifAmorphic.Outward.UnityScripts.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class DeployableColliderDetectionBinder : LateScriptBinder
    {
        public override string ScriptName => "DeployableColliderDetection";

        public int DeployedItemID = -1;
        public BuildingVisualBinder BuildingVisual;

        protected override void Init()
        {
            base.Init();

            if (DeployedItemID != -1)
            {
                var deployedItem = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(DeployedItemID);
                BoundComponent.SetField(BoundType, "DeployedItem", deployedItem);
            }

            if (BuildingVisual != null)
            {
                BoundComponent.SetField(BoundType, nameof(BuildingVisual), BuildingVisual.BoundComponent);
            }
        }
    }
}
