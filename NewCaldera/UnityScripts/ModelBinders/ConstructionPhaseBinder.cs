using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    [Serializable]
    public class ConstructionPhaseBinder : IBoundTypeConverter
    {
        public string DebugName;
        public string NameLocKey;
        [Header("Construction Data")]
        public ConstructionPhaseTypes ConstructionType;
        public int HouseCountRequirements;
        [Tooltip("In game days")]
        [SerializeField][JsonProperty]
        private int m_constructionTime;
        [SerializeField][JsonProperty]
        private BuildingResourceValuesBinder m_constructionCosts;
        public BuildingRequirementBinder[] BuildingRequirements = new BuildingRequirementBinder[0];
        public ItemQuantityBinder RareMaterialRequirement;
        [Header("Completion Data")]
        public BuildingResourceValuesBinder UpkeepCosts;
        public BuildingResourceValuesBinder UpkeepProductions;
        public BuildingResourceValuesBinder CapacityBonus;
        public bool MultiplyProductionPerHouse;
        public int HousingValue;
        public QuestEventReferenceBinder QuestEventOnPhaseFinished;

        public BuildingResourceValuesBinder ConstructionCosts => m_constructionCosts;

        public static Type GetBindingType() => OutwardAssembly.Types.ConstructionPhase;
        public Type GetBoundType() => GetBindingType();

        public object ToBoundType()
        {
            var phaseType = GetBoundType();
            var phase = Activator.CreateInstance(phaseType);

            phase.SetField(phaseType, nameof(DebugName), DebugName);
            phase.SetField(phaseType, nameof(NameLocKey), NameLocKey);
            phase.SetField(phaseType, nameof(ConstructionType), ConstructionType.ToOutwardEnumValue());
            phase.SetField(phaseType, nameof(HouseCountRequirements), HouseCountRequirements);
            phase.SetField(phaseType, nameof(m_constructionTime), m_constructionTime);
            if (m_constructionCosts != null)
                phase.SetField(phaseType, nameof(m_constructionCosts), m_constructionCosts.ToBoundType());
            if (BuildingRequirements != null && BuildingRequirements.Any())
            {
                var reqs = Array.CreateInstance(BuildingRequirementBinder.GetBindingType(), BuildingRequirements.Length);
                for (int i = 0; i < BuildingRequirements.Length; i++)
                {
                    reqs.SetValue(BuildingRequirements[i].ToBoundType(), i);
                }
                phase.SetField(phaseType, nameof(BuildingRequirements), reqs);
            }

            if (RareMaterialRequirement != null)
                phase.SetField(phaseType, nameof(RareMaterialRequirement), RareMaterialRequirement.ToBoundType());
            if (UpkeepCosts != null)
                phase.SetField(phaseType, nameof(UpkeepCosts), UpkeepCosts.ToBoundType());
            if (UpkeepProductions != null)
                phase.SetField(phaseType, nameof(UpkeepProductions), UpkeepProductions.ToBoundType());
            if (CapacityBonus != null)
                phase.SetField(phaseType, nameof(CapacityBonus), CapacityBonus.ToBoundType());
            phase.SetField(phaseType, nameof(MultiplyProductionPerHouse), MultiplyProductionPerHouse);
            phase.SetField(phaseType, nameof(HousingValue), HousingValue);
            if (QuestEventOnPhaseFinished != null)
                phase.SetField(phaseType, nameof(QuestEventOnPhaseFinished), QuestEventOnPhaseFinished.ToBoundType());

            return phase;
        }

    }
}
