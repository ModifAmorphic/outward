using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using Rewired;
using System.Collections.Generic;
using System.Text;
using ModifAmorphic.Outward.Extensions;
using Rewired.Data;
using Rewired.Data.Mapping;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{
    [HarmonyPatch(typeof(InputManager_Base))]
    public class InputManager_BasePatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static event Action<InputManager_Base> BeforeInitialize;
        [HarmonyPatch("Initialize", MethodType.Normal)]
        [HarmonyPrefix]
        private static void InitializePrefix(InputManager_Base __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(InputManager_BasePatches)}::{nameof(InitializePrefix)}(): Invoked. Invoking {nameof(BeforeInitialize)}(InputManager_Base).");
                BeforeInitialize?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InputManager_BasePatches)}::{nameof(InitializePrefix)}(): Exception Invoking {nameof(BeforeInitialize)}(InputManager_Base).", ex);
            }
        }
        private static int AddActionCategory(UserData userData, string categoryName)
        {
            var newCategory = userData.InvokePrivateMethod<UserData, InputCategory>("ktrkfyIlcFgnRMWiroMCIOVWfQua");
            newCategory.SetPrivateField("_name", categoryName);
            newCategory.SetPrivateField("_descriptiveName", categoryName);

            userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(newCategory);
            userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddCategory(newCategory.id);

            return newCategory.id;

        }
        private static int AddAction(int slotId, int categoryId, UserData userData)
        {
            var inputAction = userData.InvokePrivateMethod<UserData, InputAction>("PKUyIcoQeLkuCTdhUnVwqtkIVLm");

            inputAction.SetPrivateField("_name", "ActionSlot" + slotId);
            inputAction.SetPrivateField("_descriptiveName", "ActionSlot" + slotId);
            inputAction.SetPrivateField("_type", InputActionType.Button);
            inputAction.SetPrivateField("_userAssignable", true);
            inputAction.SetPrivateField("_categoryId", categoryId);

            userData.GetPrivateField<UserData, List<InputAction>>("actions").Add(inputAction);
            userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddAction(categoryId, inputAction.id);

            return inputAction.id;
        }

        public static event Action<InputManager_Base> AfterInitialize;
        [HarmonyPatch("Initialize", MethodType.Normal)]
        [HarmonyPostfix]
        private static void InitializePostfix(InputManager_Base __instance)
        {
            try
            {
                Logger.LogTrace($"{nameof(InputManager_BasePatches)}::{nameof(InitializePostfix)}(): Invoked. Invoking {nameof(AfterInitialize)}(InputManager_Base).");
                AfterInitialize?.Invoke(__instance);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(InputManager_BasePatches)}::{nameof(InitializePostfix)}(): Exception Invoking {nameof(AfterInitialize)}(InputManager_Base).", ex);
            }
        }
    }
}
