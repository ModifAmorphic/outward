using ModifAmorphic.Outward.UnityScripts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class BuildingBinder : ItemBinder, ISerializationCallbackReceiver
    {
        public override string ScriptName => "Building";

        [Header("Building Data")]
        public BuildingTypes BuildingType;

        [SerializeField][JsonProperty]
        private ConstructionPhaseBinder[] m_constructionPhases;
        [SerializeField][JsonProperty]
        [HideInInspector]
        private string _constructionPhasesJson;

        public ConstructionPhaseBinder[] ConstructionPhases => m_constructionPhases;

        [SerializeField][JsonProperty]
        private ConstructionPhaseBinder[] m_upgradePhases;
        [SerializeField]
        [JsonProperty]
        [HideInInspector]
        private string _upgradePhasesJson;

        public ConstructionPhaseBinder[] UpgradePhases => m_upgradePhases;

        [SerializeField][JsonProperty]
        private int m_currentBasicPhaseIndex;
        
        [SerializeField][JsonProperty]
        private int m_currentUpgradePhaseIndex = -1;

        [SerializeField]
        [JsonProperty]
        private BuildingLimits _buildingLimits;
        [SerializeField]
        [JsonProperty]
        [HideInInspector]
        private string _buildingLimitsJson;


        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(BuildingType), BuildingType.ToOutwardEnumValue());
            if (m_constructionPhases != null && m_constructionPhases.Any())
            {
                var phases = Array.CreateInstance(ConstructionPhaseBinder.GetBindingType(), m_constructionPhases.Length);
                for (int i = 0; i < m_constructionPhases.Length; i++)
                {
                    phases.SetValue(m_constructionPhases[i].ToBoundType(), i);
                }
                BoundComponent.SetField(BoundType, nameof(m_constructionPhases), phases);
            }
            else if (m_constructionPhases == null)
            {
                Debug.LogWarning("m_constructionPhases not set!");
            }

            if (m_upgradePhases != null && m_upgradePhases.Any())
            {
                var phases = Array.CreateInstance(ConstructionPhaseBinder.GetBindingType(), m_upgradePhases.Length);
                for (int i = 0; i < m_constructionPhases.Length; i++)
                {
                    phases.SetValue(m_upgradePhases[i].ToBoundType(), i);
                }
                BoundComponent.SetField(BoundType, nameof(m_upgradePhases), phases);
            }

            BoundComponent.SetField(BoundType, nameof(m_currentBasicPhaseIndex), m_currentBasicPhaseIndex);
            BoundComponent.SetField(BoundType, nameof(m_currentUpgradePhaseIndex), m_currentUpgradePhaseIndex);

            if (_buildingLimits.BuildLimit > 1)
                ModifScriptsManager.Instance.BuildLimitsManager.AddBuildLimit(ItemID, _buildingLimits);
        }

        public void OnBeforeSerialize()
        {
#if DEBUG
            if (Application.isEditor)
            {
                _constructionPhasesJson = JsonConvert.SerializeObject(m_constructionPhases);
                _upgradePhasesJson = JsonConvert.SerializeObject(m_upgradePhases);
                _buildingLimitsJson = JsonConvert.SerializeObject(_buildingLimits);
            }
#endif
        }
        public void OnAfterDeserialize()
        {
#if DEBUG
            if (!Application.isEditor)
            {
#endif
                if (!string.IsNullOrWhiteSpace(_constructionPhasesJson))
                    m_constructionPhases = JsonConvert.DeserializeObject<ConstructionPhaseBinder[]>(_constructionPhasesJson);
                else
                    m_constructionPhases = new ConstructionPhaseBinder[0];

                if (!string.IsNullOrWhiteSpace(_upgradePhasesJson))
                    m_upgradePhases = JsonConvert.DeserializeObject<ConstructionPhaseBinder[]>(_upgradePhasesJson);
                else
                    m_upgradePhases = new ConstructionPhaseBinder[0];

                if (!string.IsNullOrWhiteSpace(_buildingLimitsJson))
                    _buildingLimits = JsonConvert.DeserializeObject<BuildingLimits>(_buildingLimitsJson);
#if DEBUG
            }
#endif
        }
    }
}
