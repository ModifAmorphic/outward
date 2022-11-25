using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public class ItemVisualBinder : LateScriptBinder
    {
        public override string ScriptName => "ItemVisual";

        public Transform ItemHighlightTrans;
        public Vector3 CastTakeOffset = Vector3.zero;
        public Vector3 CastTakeRot = Vector3.zero;
        public float CastTakeScale = 1f;
        public bool IsSpecialVisual;
        public bool StoreOnDisable;
        public Sprite ItemIcon;
        [SerializeField][JsonProperty]
        private int m_itemID = -1;
        public Collider[] IgnoredColliders;
        public Collider[] IgnoredDisableColliders;
        public bool AutoPlayAnimation;
        public Collider InteractionCol;
        public bool ForceDestroyOnPutBack;

        public int ItemID => m_itemID;

        protected override void Init()
        {
            base.Init();

            BoundComponent.SetField(BoundType, nameof(ItemHighlightTrans), ItemHighlightTrans);
            BoundComponent.SetField(BoundType, nameof(CastTakeOffset), CastTakeOffset);
            BoundComponent.SetField(BoundType, nameof(CastTakeRot), CastTakeRot);
            BoundComponent.SetField(BoundType, nameof(CastTakeScale), CastTakeScale);
            BoundComponent.SetField(BoundType, nameof(IsSpecialVisual), IsSpecialVisual);
            BoundComponent.SetField(BoundType, nameof(StoreOnDisable), StoreOnDisable);
            BoundComponent.SetField(BoundType, nameof(ItemIcon), ItemIcon);
            BoundComponent.SetField(BoundType, nameof(m_itemID), m_itemID);
            BoundComponent.SetField(BoundType, nameof(IgnoredColliders), IgnoredColliders);
            BoundComponent.SetField(BoundType, nameof(IgnoredDisableColliders), IgnoredDisableColliders);
            BoundComponent.SetField(BoundType, nameof(AutoPlayAnimation), AutoPlayAnimation);
            BoundComponent.SetField(BoundType, nameof(InteractionCol), InteractionCol);
            BoundComponent.SetField(BoundType, nameof(ForceDestroyOnPutBack), ForceDestroyOnPutBack);
        }

    }
}
