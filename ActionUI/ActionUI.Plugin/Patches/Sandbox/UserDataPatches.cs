using HarmonyLib;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Patches
{
    [HarmonyPatch(typeof(UserData))]
    public class UserDataPatches
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        //[HarmonyPatch(nameof(UserData.AddAction), MethodType.Normal)]
        //[HarmonyPatch(new Type[] { typeof(int) })]
        //[HarmonyPostfix]
        //public static void AddActionPostfix(UserData __instance, List<InputAction> ___actions, int categoryId)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(UserDataPatches)}::{nameof(AddActionPostfix)}(): Invoked for categoryId {categoryId}." + // Invoking {nameof(InitializeBefore)}(InputManager_Base).");
        //            "\n\t Action Added:" +
        //            $"\n\t name: {___actions.Last().name}" +
        //            $"\n\t descriptiveName: {___actions.Last().descriptiveName}");
        //        //InitializeBefore.Invoke(__instance);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(UserDataPatches)}::{nameof(AddActionPostfix)}(): Exception", ex);
        //    }
        //}

        //[HarmonyPatch("UserData", MethodType.Constructor)]
        //[HarmonyPatch(new Type[] { typeof(bool) })]
        //[HarmonyPostfix]
        //public static void UserDataPostfix(UserData __instance, bool init)
        //{
        //    try
        //    {
        //        Logger.LogTrace($"{nameof(UserDataPatches)}::{nameof(UserDataPostfix)}(): Invoked. Adding new ActionCategory and Actions");
        //        int categoryId = AddActionCategory(__instance, "ActionMenus");
        //        for (int i = 0; i < 50; i++)
        //        {
        //            AddAction(i, categoryId, __instance);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException($"{nameof(UserDataPatches)}::{nameof(UserDataPostfix)}(): Exception", ex);
        //    }
        //}

        //private static int AddActionCategory(UserData userData, string categoryName)
        //{
        //    var newCategory = userData.InvokePrivateMethod<UserData, InputCategory>("ktrkfyIlcFgnRMWiroMCIOVWfQua");
        //    newCategory.SetPrivateField("_name", categoryName);
        //    newCategory.SetPrivateField("_descriptiveName", categoryName);

        //    userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(newCategory);
        //    userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddCategory(newCategory.id);

        //    return newCategory.id;

        //}
        //private static int AddAction(int slotId, int categoryId, UserData userData)
        //{
        //    var inputAction = userData.InvokePrivateMethod<UserData, InputAction>("PKUyIcoQeLkuCTdhUnVwqtkIVLm");

        //    inputAction.SetPrivateField("_name", "ActionSlot" + slotId);
        //    inputAction.SetPrivateField("_descriptiveName", "ActionSlot" + slotId);
        //    inputAction.SetPrivateField("_type", InputActionType.Button);
        //    inputAction.SetPrivateField("_userAssignable", true);
        //    inputAction.SetPrivateField("_categoryId", categoryId);

        //    userData.GetPrivateField<UserData, List<InputAction>>("actions").Add(inputAction);
        //    userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddAction(categoryId, inputAction.id);

        //    return inputAction.id;
        //}

        static bool outputList = false;
        [HarmonyPatch(nameof(UserData.GetActions_Copy), MethodType.Normal)]
        [HarmonyPostfix]
        public static void GetActions_CopyPostfix(UserData __instance, List<InputAction> __result)
        {
            try
            {
                Logger.LogTrace($"{nameof(UserDataPatches)}::{nameof(GetActions_CopyPostfix)}(): Invoked."); // Invoking {nameof(InitializeBefore)}(InputManager_Base).");


                if (!outputList)
                {
                    string logMsg = $"{nameof(UserDataPatches)}::{nameof(GetActions_CopyPostfix)}(): Output of result List<InputAction>:";
                    foreach (var a in __result)
                    {
                        logMsg += $"\n\t name: {a.name}" +
                                    $"\n\t\t descriptiveName: {a.descriptiveName}" +
                                    $"\n\t\t categoryId: {a.categoryId}" + 
                                    $"\n\t\t id: {a.id}";
                    }
                    Logger.LogDebug(logMsg);
                    outputList = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(UserDataPatches)}::{nameof(GetActions_CopyPostfix)}(): Exception", ex);
            }
        }
    }
}
