using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.BehaviourBinders
{
    [DisallowMultipleComponent]
    public class MultipleUsageBinder : ItemExtensionBinder
    {
        public override string ScriptName => "MultipleUsage";

        public bool AutoStack;
        public int m_maxStackAmount = 3;
        public bool AppliedOnPrice = true;
        public bool AppliedOnWeight = true;
        [SerializeField]
        private int m_currentStack = 1;
        [SerializeField]
        private bool m_moveStackAsOne;


        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(AutoStack), AutoStack);
            BoundComponent.SetField(BoundType, nameof(m_maxStackAmount), m_maxStackAmount);
            BoundComponent.SetField(BoundType, nameof(AppliedOnPrice), AppliedOnPrice);
            BoundComponent.SetField(BoundType, nameof(AppliedOnWeight), AppliedOnWeight);
            BoundComponent.SetField(BoundType, nameof(m_currentStack), m_currentStack);
            BoundComponent.SetField(BoundType, nameof(m_moveStackAsOne), m_moveStackAsOne);
        }
    }
}
