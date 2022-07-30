using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Extensions;
using Rewired;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Settings
{
    internal static class RewiredConstants
    {
        public static class ActionMenus
        {
            public const int CategoryMapId = 130000;
            public static InputMapCategory CategoryMap;
            public const int ActionCategoryId = 130001;
            public static InputCategory ActionCategory;

            static ActionMenus()
            {
                CategoryMap = GetMapCategory(
                    name: "ActionMenus",
                    descriptiveName: "ActionMenus",
                    id: CategoryMapId,
                    userAssignable: true);

                ActionCategory = GetActionCategory(
                    name: "ActionMenus",
                    descriptiveName: "ActionMenus",
                    id: ActionCategoryId,
                    userAssignable: true);
            }
        }

        public static class ActionSlots
        {
            public const int CategoryMapId = 131000;
            public static InputMapCategory CategoryMap;
            public const int ActionCategoryId = 131001;
            public static InputCategory ActionCategory;
            public static List<InputAction> Actions;
            public const string NameFormat = "ActionSlot_00";
            
            public const string NextHotbarAction = "NextHotbar";
            public const string PreviousHotbarAction = "PreviousHotbar";

            public const string SelectHotbarNameFormat = "SelectHotbar_00";

            public static readonly string DefaultKeyboardMapFile = Path.Combine(ActionMenuSettings.PluginPath, "Profiles", "Default", "Default-Keyboard-Map_ActionSlots.xml");

            static ActionSlots()
            {
                CategoryMap = GetMapCategory(
                    name: "ActionSlots",
                    descriptiveName: "ActionSlots",
                    id: CategoryMapId,
                    userAssignable: true);
                ActionCategory = GetActionCategory(
                    name: "ActionSlots",
                    descriptiveName: "ActionSlots",
                    id: ActionCategoryId,
                    userAssignable: true);
                Actions = GetSlotsActions(
                    nameFormat: NameFormat,
                    descriptiveNameFormat: "Action Slot ##0",
                    inputActionType: InputActionType.Button,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id,
                    startingId: 131500,
                    amount: 64);
                Actions.Add(GetAction(
                    id: 131100,
                    name: NextHotbarAction,
                    descriptiveName: "Next Hotbar",
                    inputActionType: InputActionType.Button,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id
                    ));
                Actions.Add(GetAction(
                    id: 131101,
                    name: PreviousHotbarAction,
                    descriptiveName: "Previous Hotbar",
                    inputActionType: InputActionType.Button,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id
                    ));
            }
        }
        private static InputMapCategory GetMapCategory(string name, string descriptiveName, int id, bool userAssignable)
        {
            var mapCategory = new InputMapCategory();
            mapCategory.SetPrivateField("_name", name);
            mapCategory.SetPrivateField("_descriptiveName", descriptiveName);
            mapCategory.SetPrivateField("_id", id);
            mapCategory.SetPrivateField("_userAssignable", userAssignable);

            return mapCategory;
        }
        private static InputCategory GetActionCategory(string name, string descriptiveName, int id, bool userAssignable)
        {
            var inputCategory = new InputCategory();
            inputCategory.SetPrivateField("_name", name);
            inputCategory.SetPrivateField("_descriptiveName", descriptiveName);
            inputCategory.SetPrivateField("_id", id);
            inputCategory.SetPrivateField("_userAssignable", userAssignable);

            return inputCategory;
        }

        private static List<InputAction> GetSlotsActions(string nameFormat, string descriptiveNameFormat, InputActionType inputActionType, bool userAssignable, int actionCategoryId, int startingId, int amount)
        {
            var actions = new List<InputAction>();

            for (int i = 0; i < amount; i++)
            {
                int slotNo = i + 1;
                var inputAction = GetAction(
                    id: i + startingId,
                    name: slotNo.ToString(nameFormat),
                    descriptiveName: slotNo.ToString(descriptiveNameFormat),
                    inputActionType: inputActionType,
                    userAssignable: userAssignable,
                    actionCategoryId: actionCategoryId
                    );
                actions.Add(inputAction);
            }

            return actions;   
        }
        public static InputAction GetAction(int id, string name, string descriptiveName, InputActionType inputActionType, bool userAssignable, int actionCategoryId)
        {
            var inputAction = new InputAction();
            
            inputAction.SetPrivateField("_id", id);
            inputAction.SetPrivateField("_name", name);
            inputAction.SetPrivateField("_descriptiveName", descriptiveName);
            inputAction.SetPrivateField("_type", inputActionType);
            inputAction.SetPrivateField("_userAssignable", userAssignable);
            inputAction.SetPrivateField("_categoryId", actionCategoryId);
            
            return inputAction;
        }
    }
}
