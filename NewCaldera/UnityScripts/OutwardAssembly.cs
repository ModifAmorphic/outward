using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public static class OutwardAssembly
    {
        private static Assembly _assembly = null;
        public static Assembly GetAssembly()
        {
            if (_assembly != null)
                return _assembly;
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            _assembly = loadedAssemblies
                .FirstOrDefault(a => a.GetName().Name.Equals("Assembly-CSharp", StringComparison.InvariantCultureIgnoreCase));

            Debug.Log($"OutwardAssembly::GetAssembly() Got assembly '{_assembly?.GetName()}'");
            return _assembly;
        }

        public static Type GetType(string name) =>
            GetAssembly().GetTypes().FirstOrDefault(t => t.FullName.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public static Type GetNestedType(Type parentType, string typeName)
        {
            return parentType.GetNestedType(typeName, BindingFlags.Public | BindingFlags.NonPublic);
        }
        public static Type GetNestedType(string parentTypeName, string typeName)
        {
            return GetType(parentTypeName).GetNestedType(typeName, BindingFlags.Public | BindingFlags.NonPublic);
        }
        public static Type GetOutwardEnumType<T>() where T : Enum =>
            _enumToOutwardMappings.TryGetValue(typeof(T), out var outwardEnumType) ? outwardEnumType : null;

        public static Type GetLocalEnumType<T>(Type outwardEnumType) where T : Enum =>
            _outwardToEnumMappings.TryGetValue(outwardEnumType, out var localEnumType) ? localEnumType : null;

        public static class Types
        {
            public static Type Area => OutwardAssembly.GetType("Area");
            public static Type AreaManager => OutwardAssembly.GetType("AreaManager");
            public static Type BasicItemDrop => OutwardAssembly.GetType("BasicItemDrop");
            public static Type Blueprint => OutwardAssembly.GetType("Blueprint");
            public static Type Building => OutwardAssembly.GetType("Building");
            public static Type BuildingRequirement => OutwardAssembly.GetNestedType(Building, "BuildingRequirement");
            public static Type BuildingResourceValues => OutwardAssembly.GetType("BuildingResourceValues");
            public static Type BuildingResourcesManager => OutwardAssembly.GetType("BuildingResourcesManager");            
            public static Type BuildingVisual => OutwardAssembly.GetType("BuildingVisual");
            public static Type BuildingVisualPool => OutwardAssembly.GetType("BuildingVisualPool");
            public static Type Character => OutwardAssembly.GetType("Character");
            public static Type ConstructionPhase => OutwardAssembly.GetNestedType(Building, "ConstructionPhase");
            public static Type Deployable => OutwardAssembly.GetType("Deployable");
            public static Type Global => OutwardAssembly.GetType("Global");
            public static Type GlobalAudioManager => OutwardAssembly.GetType("GlobalAudioManager");
            public static Type GuaranteedDrop => OutwardAssembly.GetType("GuaranteedDrop");
            public static Type Item => OutwardAssembly.GetType("Item");
            public static Type ItemLocalization => OutwardAssembly.GetType("Localizer.ItemLocalization");            
            public static Type ItemQuantity => OutwardAssembly.GetType("ItemQuantity");
            public static Type ItemVisual => OutwardAssembly.GetType("ItemVisual");
            public static Type LocalizationManager => OutwardAssembly.GetType("LocalizationManager");
            public static Type OTWStoreAPI => OutwardAssembly.GetType("OTWStoreAPI");
            public static Type Merchant => OutwardAssembly.GetType("Merchant");
            public static Type QuestEventReference => OutwardAssembly.GetType("QuestEventReference");
            public static Type ResourcesPrefabManager => OutwardAssembly.GetType("ResourcesPrefabManager");
            public static Type SNPC => OutwardAssembly.GetType("SNPC");
            

            public static class Enums
            {
                public static Type AreaEnum => OutwardAssembly.GetNestedType(AreaManager, "AreaEnum");
                public static Type BagCategorySlotType => OutwardAssembly.GetNestedType(Item, "BagCategorySlotType");
                public static Type BehaviorOnNoDurabilityType => OutwardAssembly.GetNestedType(Item, "BehaviorOnNoDurabilityType");
                public static Type BuildingTypes => OutwardAssembly.GetNestedType(Building, "BuildingTypes");
                public static Type DeployStates => OutwardAssembly.GetNestedType(Deployable, "DeployStates");
                public static Type CastTakeTypes => OutwardAssembly.GetNestedType(Item, "CastTakeTypes");
                public static Type ConstructionPhaseTypes => OutwardAssembly.GetNestedType(ConstructionPhase, "Type");
                public static Type DLCs => OutwardAssembly.GetNestedType(OTWStoreAPI, "DLCs");
                public static Type Lit => OutwardAssembly.GetNestedType(Item, "Lit");
                public static Type SaveTypes => OutwardAssembly.GetNestedType(Item, "SaveTypes");
                public static Type Sounds => OutwardAssembly.GetNestedType(GlobalAudioManager, "Sounds");
                public static Type SpellCastModifier => OutwardAssembly.GetNestedType(Character, "SpellCastModifier");
                public static Type SpellCastType => OutwardAssembly.GetNestedType(Character, "SpellCastType");

            }
        }

        private static readonly Dictionary<Type, Type> _enumToOutwardMappings = new Dictionary<Type, Type>()
        {
            { typeof(AreaEnum), Types.Enums.AreaEnum },
            { typeof(BagCategorySlotType), Types.Enums.BagCategorySlotType },
            { typeof(BehaviorOnNoDurabilityType), Types.Enums.BehaviorOnNoDurabilityType },
            { typeof(BuildingTypes), Types.Enums.BuildingTypes },
            { typeof(CastTakeTypes), Types.Enums.CastTakeTypes },
            { typeof(ConstructionPhaseTypes), Types.Enums.ConstructionPhaseTypes },
            { typeof(DeployStates), Types.Enums.DeployStates },
            { typeof(DLCs), Types.Enums.DLCs },
            { typeof(Lit), Types.Enums.Lit },
            { typeof(SaveTypes), Types.Enums.SaveTypes },
            { typeof(SpellCastModifier), Types.Enums.SpellCastModifier },
            { typeof(SpellCastType), Types.Enums.SpellCastType },
            { typeof(Sounds), Types.Enums.Sounds },
        };

        private static readonly Dictionary<Type, Type> _outwardToEnumMappings = _enumToOutwardMappings.ToDictionary(x => x.Value, x => x.Key);

    }
}
