using ModifAmorphic.Outward.Logging;
using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings.Services
{
    internal class QsMenuExtender
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();


        public QsMenuExtender(Func<IModifLogger> loggerFactory) =>
            (_loggerFactory) = (loggerFactory);


        private const string ActionNamePrefix = "QS_Instant";
        private const string ActionKeyPrefix = "InputAction_";
        private const string MenuSlotNoKey = "{ExtraSlotNumber}";
        private const string MenuDescriptionDefaultFormat = "Ex Quick Slot {ExtraSlotNumber}";

        /// <summary>
        /// Adds additional QuickSlot keybindings to the "Quick Slot" section under the Settings --> Keyboard menu.
        /// Raises the <see cref="QuickSlotExtenderEvents.RaiseSlotsChanged(object, QuickSlotExtendedArgs)"/> event when complete.
        /// </summary>
        /// <param name="menuDescriptions">Queue of menu descriptions to add.</param>
        public List<ExtendedQuickSlot> ExtendQuickSlots(Queue<string> menuDescriptions)
        {
            try
            {
                var extendAmount = menuDescriptions.Count;
                var x = 0;
                var exQuickSlots = new List<ExtendedQuickSlot>();
                var startingId = Enum.GetValues(typeof(QuickSlot.QuickSlotIDs)).Cast<int>().Max() + 1;

                while (menuDescriptions.Count > 0)
                {
                    int quickSlotId = x + startingId;
                    int exSlotNo = x + 1;
                    string actionName = ActionNamePrefix + quickSlotId.ToString();
                    string description = menuDescriptions.Dequeue();
                    if (string.IsNullOrEmpty(description?.Trim()))
                    {
                        Logger.LogWarning($"{nameof(ExtendQuickSlots)}(): Empty menu description found in queue {nameof(menuDescriptions)}. Defaulting menu message text for extra slot {exSlotNo}.");
                        description = MenuDescriptionDefaultFormat.Replace(MenuSlotNoKey, exSlotNo.ToString());
                    }

                    //keep track of what is being added to raise an event when we're done
                    exQuickSlots.Add(new ExtendedQuickSlot()
                    {
                        QuickSlotId = quickSlotId,
                        ActionName = actionName,
                        ActionKey = ActionKeyPrefix + actionName,
                        ActionDescription = description
                    });
                    //finally, add the slot already
                    CustomKeybindings.AddAction(actionName, KeybindingsCategory.QuickSlot, ControlType.Both, SideLoader.InputType.Button);

                    //_customKeyBindings.AddAction(actionName, description, KeybindingsCategory.QuickSlot, ControlType.Both, 5);
                    Logger.LogDebug($"Adding quickslot - id: {quickSlotId}; actionName: '{actionName}'; description: {description}");
                    x++;
                }

                Logger.LogTrace($"RaiseSlotsChanged Event. StartId = {startingId}, QuickSlotsToAdd = {exQuickSlots.Count}");
                //Raise event that slots changed so subscribers can react
                return exQuickSlots;
                //QuickSlotExtenderEvents.RaiseSlotsChanged(this, new QuickSlotExtendedArgs(startingId, exQuickSlots));
            }
            catch (Exception ex)
            {
                Logger.LogException($"Exception in {nameof(QuickSlotExtender)}.{nameof(ExtendQuickSlots)}(Queue<string> menuDescriptions). Extend failed.", ex);
                throw;
            }
        }
        /// <summary>
        /// Generates Keybinding descriptions and adds additional QuickSlot keybindings to the "Quick Slot" section under the Settings --> Keyboard menu.
        /// Raises the <see cref="QuickSlotExtenderEvents.RaiseSlotsChanged(object, QuickSlotExtendedArgs)"/> event when complete.
        /// </summary>
        /// <param name="extendAmount">Amount of additional quickslots to add</param>
        /// <param name="menuDescriptionFormat">Format containing '{MenuSlotNoKey}' token to be replaced used as the label/description under the "Quick Slot" section for each additonal quickslot added. 
        /// For example, "Ex Slot {MenuSlotNoKey}" will label a new Quick Slot keybinding with a description of "Ex Slot 1", "Ex Slot 2", etc for the total number of <paramref name="extendAmount"/></param>
        public List<ExtendedQuickSlot> ExtendQuickSlots(int extendAmount, string menuDescriptionFormat)
        {
            var menuDescriptions = new Queue<string>();
            Logger.LogDebug($"Extending quickslots by {extendAmount}.");
            try
            {
                string menuFormat = menuDescriptionFormat;
                if (extendAmount < 1)
                    throw new ArgumentOutOfRangeException(nameof(extendAmount), $"Value of {nameof(extendAmount)} must be greater than 0.  Value was {extendAmount}.");
                if (string.IsNullOrEmpty(menuDescriptionFormat?.Trim()) || !menuDescriptionFormat.Contains(MenuSlotNoKey))
                {
                    Logger.LogWarning($"{nameof(ExtendQuickSlots)}(): '{MenuSlotNoKey}' replacer not found in {nameof(menuDescriptionFormat)} parameter. Using default menu formatting.");
                    menuFormat = MenuDescriptionDefaultFormat;
                }
                for (int x = 0; x < extendAmount; x++)
                {
                    int exSlotNo = x + 1;
                    menuDescriptions.Enqueue(menuFormat.Replace(MenuSlotNoKey, exSlotNo.ToString()));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException($"Exception in {nameof(QuickSlotExtender)}.{nameof(ExtendQuickSlots)}(int extendAmount, string menuDescriptionFormat). Extend failed.", ex);
                throw;
            }
            return ExtendQuickSlots(menuDescriptions);
        }
    }
}
