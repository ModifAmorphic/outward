using Localizer;
using ModifAmorphic.Outward.Patches;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using ModifAmorphic.Outward.Extensions;
using System.Reflection;
using System.Collections.Concurrent;
using ModifAmorphic.Outward.Modules.Items.Patches;

namespace ModifAmorphic.Outward.Modules.Quests
{
    public class QuestPreFabricator : IModifModule
    {
        private readonly string _modId;
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();
        private readonly QuestPrefabService _itemPrefabService;

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(LocalizationManagerPatches),
            typeof(ItemPatches)
        };

        internal QuestPreFabricator(string modId, QuestPrefabService itemPrefabService, Func<IModifLogger> loggerFactory)
        {
            this._modId = modId;
            this._loggerFactory = loggerFactory;
            this._itemPrefabService = itemPrefabService;
        }

        public T CreatePrefab<T>(int baseItemID, int newItemID, string name, string description, bool setFields = false) where T : Quest
        {
            return _itemPrefabService.CreatePrefab<T>(baseItemID, newItemID, name, description, setFields);
        }
        public T CreatePrefab<T>(T basePrefab, int newItemID, string name, string description, bool setFields = false) where T : Quest
        {
            return _itemPrefabService.CreatePrefab<T>(basePrefab, newItemID, name, description, setFields);
        }
    }
}
