using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class ItemStatsBinder : LateScriptBinder
    {
        public override string ScriptName => "ItemStats";

        [Header("Base Stats")]
        //This is a real rabbit hole of classes that I skipped for now, as building Blueprints don't use it.
        //StatThreshold.m_thresholdLimits[].StatusEffecPrefabs
        //[SerializeField]
        //private StatThreshold EffectivenessPrefab;
        [SerializeField]
        private bool m_hasNoEffectivenessThreshold;
        [SerializeField]
        private int m_baseValue = 1;
        [SerializeField]
        private float m_rawWeight;
        [SerializeField]
        protected int m_baseMaxDurability = 100;
        public int StartingDurability = -1;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(m_hasNoEffectivenessThreshold), m_hasNoEffectivenessThreshold);
            BoundComponent.SetField(BoundType, nameof(m_baseValue), m_baseValue);
            BoundComponent.SetField(BoundType, nameof(m_rawWeight), m_rawWeight);
            BoundComponent.SetField(BoundType, nameof(m_baseMaxDurability), m_baseMaxDurability);
            BoundComponent.SetField(BoundType, nameof(StartingDurability), StartingDurability);
        }
    }
}
