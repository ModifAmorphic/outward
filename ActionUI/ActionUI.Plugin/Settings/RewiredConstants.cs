using ModifAmorphic.Outward.Extensions;
using Rewired;
using System.Collections.Generic;
using System.IO;

namespace ModifAmorphic.Outward.ActionUI.Settings
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
            public static List<InputAction> HotbarNavActions;
            public const string NameFormat = "ActionSlot_00";
            public const string NavNameFormat = "Hotbar_00";

            public static InputAction NextHotbarAction;
            public static InputAction PreviousHotbarAction;

            public static InputAction NextHotbarAxisAction;
            public static InputAction PreviousHotbarAxisAction;

            public const string SelectHotbarNameFormat = "SelectHotbar_00";

            public static readonly string DefaultKeyboardMapFile = Path.Combine(ActionUISettings.PluginPath, "Profiles", "Default", "Default-Keyboard-Map_ActionSlots.xml");
            public static readonly string DefaultMouseMapFile = Path.Combine(ActionUISettings.PluginPath, "Profiles", "Default", "Default-Mouse-Map_ActionSlots.xml");
            public static readonly string DefaultJoystickMapFile = string.Empty;

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

                HotbarNavActions = GetHotbarNavActions(
                    nameFormat: NavNameFormat,
                    descriptiveNameFormat: "Hotbar ##0",
                    inputActionType: InputActionType.Button,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id,
                    startingId: 131200,
                    amount: 64);
                NextHotbarAction = GetAction(
                    id: 131100,
                    name: "NextHotbar",
                    descriptiveName: "Next Hotbar",
                    inputActionType: InputActionType.Button,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id
                    );
                PreviousHotbarAction = GetAction(
                    id: 131101,
                    name: "PreviousHotbar",
                    descriptiveName: "Previous Hotbar",
                    inputActionType: InputActionType.Button,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id
                    );
                NextHotbarAxisAction = GetAction(
                    id: 131102,
                    name: "NextHotbar+",
                    descriptiveName: "Next Hotbar",
                    inputActionType: InputActionType.Axis,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id
                    );
                PreviousHotbarAxisAction = GetAction(
                    id: 131103,
                    name: "PreviousHotbar-",
                    descriptiveName: "Previous Hotbar",
                    inputActionType: InputActionType.Axis,
                    userAssignable: true,
                    actionCategoryId: ActionCategory.id
                    );
                HotbarNavActions.Add(NextHotbarAction);
                HotbarNavActions.Add(PreviousHotbarAction);
                HotbarNavActions.Add(NextHotbarAxisAction);
                HotbarNavActions.Add(PreviousHotbarAxisAction);
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
        private static List<InputAction> GetHotbarNavActions(string nameFormat, string descriptiveNameFormat, InputActionType inputActionType, bool userAssignable, int actionCategoryId, int startingId, int amount)
        {
            var actions = new List<InputAction>();

            for (int i = 0; i < amount; i++)
            {
                int barNo = i + 1;
                var inputAction = GetAction(
                    id: i + startingId,
                    name: barNo.ToString(nameFormat),
                    descriptiveName: barNo.ToString(descriptiveNameFormat),
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
