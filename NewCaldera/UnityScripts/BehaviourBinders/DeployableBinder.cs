using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class DeployableBinder : LateScriptBinder
    {
        public override string ScriptName => "Deployable";

        [SerializeField]
        private int m_requiredSkillID = -1;
        public DeployStates State;

        public bool DestroyOnNoPackState;
        public int PackedStatePrefabItemID = -1;
        public int DeployedStatePrefabItemID = -1;
        public SpellCastType CastAnim = SpellCastType.SetupGround;
        public Vector3 DisassembleOffset = Vector3.zero;
        public bool AutoTake = true;
        public Sounds DeploySound;
        public Sounds DisassembleSound;

        //BasicDeployable Fields
        public Vector3 DeploymentOffset = Vector3.forward;
        public Vector3 DeploymentDirection = Vector3.right;
        public bool CantDeployInNoBedZones;

        protected override void Init()
        {
            base.Init();
            if (m_requiredSkillID != -1)
            {
                var requiredSkill = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(m_requiredSkillID);
                BoundComponent.SetField(BoundType, "m_requiredSkill", requiredSkill);
            }
            
            BoundComponent.SetField(BoundType, nameof(State), State.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(DestroyOnNoPackState), DestroyOnNoPackState);
            
            if (PackedStatePrefabItemID != -1)
            {
                var packedStateItemPrefab = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(PackedStatePrefabItemID);
                if (packedStateItemPrefab == null)
                    ModifScriptsManager.Instance.Logger.LogWarning($"{name} PackedStateItemPrefab could not be set. ItemID {PackedStatePrefabItemID} was not found.");
                BoundComponent.SetField(BoundType, "PackedStateItemPrefab", packedStateItemPrefab);
            }
            
            if (DeployedStatePrefabItemID != -1)
            {
                var deployedStateItemPrefab = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(DeployedStatePrefabItemID);
                if (deployedStateItemPrefab == null)
                    ModifScriptsManager.Instance.Logger.LogWarning($"{name} DeployedStateItemPrefab could not be set. ItemID {DeployedStatePrefabItemID} was not found.");
                BoundComponent.SetField(BoundType, "DeployedStateItemPrefab", deployedStateItemPrefab);
            }
            
            BoundComponent.SetField(BoundType, nameof(CastAnim), CastAnim.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(DisassembleOffset), DisassembleOffset);
            BoundComponent.SetField(BoundType, nameof(AutoTake), AutoTake);
            BoundComponent.SetField(BoundType, nameof(DeploySound), DeploySound.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(DisassembleSound), DisassembleSound.ToOutwardEnumValue());

            BoundComponent.SetField(BoundType, nameof(DeploymentOffset), DeploymentOffset);
            BoundComponent.SetField(BoundType, nameof(DeploymentDirection), DeploymentDirection);
            BoundComponent.SetField(BoundType, nameof(CantDeployInNoBedZones), CantDeployInNoBedZones);
        }
    }
}
