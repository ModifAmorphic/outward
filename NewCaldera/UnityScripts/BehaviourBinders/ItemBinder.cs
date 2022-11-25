using ModifAmorphic.Outward.UnityScripts.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
   

    public class ItemBinder : LateScriptBinder
    {
        public override string ScriptName => "Item";

        [SerializeField][JsonProperty]
        private string m_visualPrefabPath;
        [SerializeField]
        [JsonProperty]
        private string m_specialVisualPrefabDefaultPath;
        [SerializeField]
        [JsonProperty]
        private string m_specialVisualPrefabFemalePath;
        public bool IsUsable;
        public bool SetupEffectsOnOwnerChange;
        public bool ForceDisallowUseWithoutOwning;
        [SerializeField]
        [JsonProperty]
        private string m_itemIconPath;
        [SerializeField][JsonProperty]
        protected Sprite m_lockedIcon;
        [SerializeField][JsonProperty]
        public bool HasDynamicQuickSlotIcon;
        [SerializeField][JsonProperty]
        private Sprite m_overrideSigil;
        [SerializeField][JsonProperty]
        private string m_UID;
        public SaveTypes SaveType;
        public bool ForceNonSavable;
        public bool ForceUpdateParentChangeInit;
        public int ItemID;
        [SerializeField][JsonProperty]
        protected string m_name = "Item";
        [SerializeField][JsonProperty]
        protected DLCs m_DLCID;
        [SerializeField][JsonProperty]
        protected List<DLCs> m_additionalDLCIDs = new List<DLCs>();
        [HideInInspector]
        private string m_lastNameLang = "";
        [HideInInspector]
        protected string m_lastDescLang = "";
        [SerializeField][JsonProperty]
        protected float OverrideDistanceActiveness = -1f;
        public float InteractionColliderRadius = 0.1f;
        public BagCategorySlotType BagCategorySlot;
        public Vector3 BagSlotAdjustedPos = Vector3.zero;
        public Vector3 BagSlotAdjustedRotation = Vector3.zero;
        public bool IsPickable = true;
        public bool CanBePutInInventory = true;
        public bool HasPhysicsWhenWorld;
        public bool GroupItemInDisplay;
        public bool IsSkillItem;
        public bool IsForceInteractible;
        public bool HideInInventory;
        public int QtyRemovedOnUse = 1;
        [SerializeField][JsonProperty]
        private float m_currentDurability;
        [SerializeField][JsonProperty]
        public BehaviorOnNoDurabilityType BehaviorOnNoDurability;
        [SerializeField][JsonProperty]
        //protected Item m_itemOnNoDurability;
        protected int m_itemIdOnNoDurability = -1;
        [SerializeField][JsonProperty]
        private string m_onBreakNotificationOverride;
        //[SerializeField][JsonProperty]
        //protected InteractionActivator m_interactionActivator;
        //[SerializeField][JsonProperty]
        //protected InteractionTriggerBase m_interactionTrigger;
        [SerializeField][JsonProperty]
        private float m_overrideInteractionRange = -1f;
        public bool DisallowQuickSlotSubstitution;
        [SerializeField][JsonProperty]
        private bool m_stuck;
        
        public SpellCastType PickUpAnim = SpellCastType.NONE;
        public SpellCastType DroppAnim = SpellCastType.NONE;
        public bool DropInPlace;
        [SerializeField][JsonProperty]
        private SpellCastType m_activateEffectAnimType = SpellCastType.NONE;
        [SerializeField][JsonProperty]
        private SpellCastType m_alternateActivateEffectAnimType = SpellCastType.NONE;
        public int AlternateAnimHasSkillID = -1;
        public bool CastLocomotionEnabled;
        public SpellCastModifier CastModifier;
        public float MobileCastMovementMult = -1f;
        public bool MobileCastPersistToCastDone;
        public int CastSheathRequired = 1;
        public int CastSheathRequiredAlternate = -1;
        public CastTakeTypes CastTakeType;
        [Header("Buy/Sell")]
        public bool IsSellable = true;
        [SerializeField][JsonProperty]
        private float m_overrideSellModifier = -1f;
        [SerializeField][JsonProperty]
        private float m_overrideBuyModifier = -1f;
        public string SellWarning;
        public bool RepairedInRest = true;
        public Lit StartLitStatus;
        public bool IsGenerated;
        public MonoBehaviour[] ToDisableWhenInInventory = new MonoBehaviour[0];
        //[Header("Sound")]
        //public SoundPlayer m_UseSound;
        //public SoundPlayer m_onEffectSound;
        public int LegacyItemID = -1;

        public string VisualPrefabPath {get => m_visualPrefabPath; set => m_visualPrefabPath = value; }
        public int ItemIdOnNoDurability => m_itemIdOnNoDurability;

        protected override void Init()
        {
            base.Init();
            BindFields();
            var prefabManager = ModifScriptsManager.Instance?.PrefabManager;
            if (prefabManager != null)
            {

            }
        }

        public void SetIsPrefab(bool isPrefab) => BoundComponent.SetField(BoundType, "m_isPrefab", isPrefab);

        private void BindFields()
        {
            var itemType = OutwardAssembly.Types.Item;
            var characterType = OutwardAssembly.Types.Character;

            BoundComponent.SetField(BoundType, nameof(m_visualPrefabPath), m_visualPrefabPath);
            BoundComponent.SetField(BoundType, nameof(m_specialVisualPrefabDefaultPath), m_specialVisualPrefabDefaultPath);
            BoundComponent.SetField(BoundType, nameof(m_specialVisualPrefabFemalePath), m_specialVisualPrefabFemalePath);
            BoundComponent.SetField(BoundType, nameof(IsUsable), IsUsable);
            BoundComponent.SetField(BoundType, nameof(SetupEffectsOnOwnerChange), SetupEffectsOnOwnerChange);
            BoundComponent.SetField(BoundType, nameof(ForceDisallowUseWithoutOwning), ForceDisallowUseWithoutOwning);
            BoundComponent.SetField(BoundType, nameof(m_itemIconPath), m_itemIconPath);
            BoundComponent.SetField(BoundType, nameof(m_lockedIcon), m_lockedIcon);
            BoundComponent.SetField(BoundType, nameof(HasDynamicQuickSlotIcon), HasDynamicQuickSlotIcon);
            BoundComponent.SetField(BoundType, nameof(m_overrideSigil), m_overrideSigil);
            BoundComponent.SetField(BoundType, nameof(m_UID), m_UID);

            BoundComponent.SetField(BoundType, nameof(SaveType), SaveType.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(ForceNonSavable), ForceNonSavable);
            BoundComponent.SetField(BoundType, nameof(ForceUpdateParentChangeInit), ForceUpdateParentChangeInit);
            BoundComponent.SetField(BoundType, nameof(ItemID), ItemID);
            BoundComponent.SetField(BoundType, nameof(m_name), m_name);

            BoundComponent.SetField(BoundType, nameof(m_DLCID), m_DLCID.ToOutwardEnumValue());
            if (m_additionalDLCIDs != null && m_additionalDLCIDs.Any())
            {
                var dlcIdType = OutwardAssembly.Types.Enums.DLCs;
                var dlcIds = ReflectionExtensions.CreateList(dlcIdType);
                BoundComponent.SetField(BoundType, nameof(ToDisableWhenInInventory), ToDisableWhenInInventory);
                for (int i = 0; i < m_additionalDLCIDs.Count; i++)
                {
                    var dlcid = Enum.ToObject(dlcIdType, m_additionalDLCIDs[i]);
                    dlcIds.Add(dlcid);
                }
                BoundComponent.SetField(BoundType, nameof(m_additionalDLCIDs), dlcIds);
            }
            BoundComponent.SetField(BoundType, nameof(OverrideDistanceActiveness), OverrideDistanceActiveness);
            BoundComponent.SetField(BoundType, nameof(InteractionColliderRadius), InteractionColliderRadius);
            BoundComponent.SetField(BoundType, nameof(BagCategorySlot), BagCategorySlot.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(BagSlotAdjustedPos), BagSlotAdjustedPos);
            BoundComponent.SetField(BoundType, nameof(BagSlotAdjustedRotation), BagSlotAdjustedRotation);
            BoundComponent.SetField(BoundType, nameof(IsPickable), IsPickable);
            BoundComponent.SetField(BoundType, nameof(CanBePutInInventory), CanBePutInInventory);
            BoundComponent.SetField(BoundType, nameof(HasPhysicsWhenWorld), HasPhysicsWhenWorld);
            BoundComponent.SetField(BoundType, nameof(GroupItemInDisplay), GroupItemInDisplay);
            BoundComponent.SetField(BoundType, nameof(IsSkillItem), IsSkillItem);
            BoundComponent.SetField(BoundType, nameof(IsForceInteractible), IsForceInteractible);
            BoundComponent.SetField(BoundType, nameof(HideInInventory), HideInInventory);
            BoundComponent.SetField(BoundType, nameof(QtyRemovedOnUse), QtyRemovedOnUse);
            BoundComponent.SetField(BoundType, nameof(m_currentDurability), m_currentDurability);
            BoundComponent.SetField(BoundType, nameof(BehaviorOnNoDurability), BehaviorOnNoDurability.ToOutwardEnumValue());
            //item convert
            if (m_itemIdOnNoDurability != -1)
            {
                var itemOnNoDurability = ModifScriptsManager.Instance.PrefabManager.GetItemPrefab(m_itemIdOnNoDurability);
                BoundComponent.SetField(BoundType, "m_itemOnNoDurability", itemOnNoDurability);
            }
            BoundComponent.SetField(BoundType, nameof(m_onBreakNotificationOverride), m_onBreakNotificationOverride);
            BoundComponent.SetField(BoundType, nameof(m_overrideInteractionRange), m_overrideInteractionRange);
            BoundComponent.SetField(BoundType, nameof(DisallowQuickSlotSubstitution), DisallowQuickSlotSubstitution);
            BoundComponent.SetField(BoundType, nameof(m_stuck), m_stuck);
            BoundComponent.SetField(BoundType, nameof(PickUpAnim), PickUpAnim.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(DroppAnim), DroppAnim.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(DropInPlace), DropInPlace);
            BoundComponent.SetField(BoundType, nameof(m_activateEffectAnimType), m_activateEffectAnimType.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(m_alternateActivateEffectAnimType), m_alternateActivateEffectAnimType.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(AlternateAnimHasSkillID), AlternateAnimHasSkillID);
            BoundComponent.SetField(BoundType, nameof(CastLocomotionEnabled), CastLocomotionEnabled);
            BoundComponent.SetField(BoundType, nameof(CastModifier), CastModifier.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(MobileCastMovementMult), MobileCastMovementMult);
            BoundComponent.SetField(BoundType, nameof(MobileCastPersistToCastDone), MobileCastPersistToCastDone);
            BoundComponent.SetField(BoundType, nameof(CastSheathRequired), CastSheathRequired);
            BoundComponent.SetField(BoundType, nameof(CastSheathRequiredAlternate), CastSheathRequiredAlternate);
            BoundComponent.SetField(BoundType, nameof(CastTakeType), CastTakeType.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(IsSellable), IsSellable);
            BoundComponent.SetField(BoundType, nameof(m_overrideSellModifier), m_overrideSellModifier);
            BoundComponent.SetField(BoundType, nameof(m_overrideBuyModifier), m_overrideBuyModifier);
            BoundComponent.SetField(BoundType, nameof(SellWarning), SellWarning);
            BoundComponent.SetField(BoundType, nameof(RepairedInRest), RepairedInRest);
            BoundComponent.SetField(BoundType, nameof(StartLitStatus), StartLitStatus.ToOutwardEnumValue());
            BoundComponent.SetField(BoundType, nameof(IsGenerated), IsGenerated);
            BoundComponent.SetField(BoundType, nameof(ToDisableWhenInInventory), ToDisableWhenInInventory);
            BoundComponent.SetField(BoundType, nameof(LegacyItemID), LegacyItemID);
        }
    }
}
