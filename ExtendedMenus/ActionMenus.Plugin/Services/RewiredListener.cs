using BepInEx;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Plugin.Services
{
    internal class RewiredListener
    {
        private readonly ConfigSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly LevelCoroutines _coroutine;


        public RewiredListener(BaseUnityPlugin baseUnityPlugin,
                                LevelCoroutines coroutine,
                                ConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _coroutine, _settings, _getLogger) = (baseUnityPlugin, coroutine, settings, getLogger);
            InputManager_BasePatches.BeforeInitialize += InitializeActonMenus;
        }

        private void InitializeActonMenus(InputManager_Base inputManager)
        {
            var actionCategoryMap = inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap");

            inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Add(RewiredConstants.ActionMenus.CategoryMap);
            inputManager.userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(RewiredConstants.ActionMenus.ActionCategory);
            actionCategoryMap.AddCategory(RewiredConstants.ActionMenus.ActionCategory.id);

            inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Add(RewiredConstants.ActionSlots.CategoryMap);
            inputManager.userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(RewiredConstants.ActionSlots.ActionCategory);
            inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddCategory(RewiredConstants.ActionSlots.ActionCategory.id);

            var actions = inputManager.userData.GetPrivateField<UserData, List<InputAction>>("actions");

            foreach (var action in RewiredConstants.ActionSlots.Actions)
            {
                Logger.LogDebug($"Adding action {action.name} with id {action.id}");
                inputManager.userData.GetPrivateField<UserData, List<InputAction>>("actions").Add(action);
                inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddAction(action.categoryId, action.id);
            }

            foreach (var action in RewiredConstants.ActionSlots.HotbarNavActions)
            {
                Logger.LogDebug($"Adding hotbar nav action {action.name} with id {action.id}");
                inputManager.userData.GetPrivateField<UserData, List<InputAction>>("actions").Add(action);
                inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddAction(action.categoryId, action.id);
            }
        }
    }
}
