using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SideLoader;
using ModifAmorphic.Outward.Models;
using ModifAmorphic.Outward.Events;
using System.Linq;

namespace ModifAmorphic.Outward.KeyBindings
{
    public class QuickSlotExtender
    {
        private readonly Logging.Logger logger;
        private CustomKeybindings _customKeyBindings;

        private bool _lazyInitNeeded = true;
        private const string ActionNamePrefix = "QS_Instant";
        private const string ActionKeyPrefix = "InputAction_";
        private const string MenuSlotNoKey = "{ExtraSlotNumber}";
        private const string MenuDescriptionDefaultFormat = "Ex Quick Slot {ExtraSlotNumber}";

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public QuickSlotExtender()
        {
            this.logger = Logging.InternalLoggerFactory.GetLogger(Logging.LogLevel.Info, SharedPlugin.ModName);
        }
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public QuickSlotExtender(Logging.Logger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        private void LazyInit()
        {
            if (_lazyInitNeeded)
            {
                if (_customKeyBindings == null)
                    this._customKeyBindings = new CustomKeybindings(); //new CustomKeyBindings(this.logger);
                _lazyInitNeeded = false;
            }
        }

        
        public void ExtendQuickSlots(Queue<string> menuDescriptions)
        {
            try
            {
                LazyInit();
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
                        logger.LogWarning($"{nameof(ExtendQuickSlots)}(): Empty menu description found in queue {nameof(menuDescriptions)}. Defaulting menu message text for extra slot {exSlotNo}.");
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
                    logger.LogDebug($"Adding quickslot - id: {quickSlotId}; actionName: '{actionName}'; description: {description}");
                    x++;
                }

                logger.LogTrace($"RaiseSlotsChanged Event. StartId = {startingId}, QuickSlotsToAdd = {exQuickSlots.Count}");
                //Raise event that slots changed so subscribers can react
                QuickSlotExtenderEvents.RaiseSlotsChanged(this, new QuickSlotExtendedArgs(startingId, exQuickSlots));
            }
            catch (Exception ex)
            {
                logger.LogException($"Exception in {nameof(QuickSlotExtender)}.{nameof(ExtendQuickSlots)}(Queue<string> menuDescriptions). Extend failed.", ex);
                throw;
            }
        }

        public void ExtendQuickSlots(int extendAmount, string menuDescriptionFormat)
        {
            var menuDescriptions = new Queue<string>();
            logger.LogDebug($"Extending quickslots by {extendAmount}.");
            try
            {
                string menuFormat = menuDescriptionFormat;
                if (extendAmount < 1)
                    throw new ArgumentOutOfRangeException(nameof(extendAmount), $"Value of {nameof(extendAmount)} must be greater than 0.  Value was {extendAmount}.");
                if (string.IsNullOrEmpty(menuDescriptionFormat?.Trim()) || !menuDescriptionFormat.Contains(MenuSlotNoKey))
                {
                    logger.LogWarning($"{nameof(ExtendQuickSlots)}(): '{MenuSlotNoKey}' replacer not found in {nameof(menuDescriptionFormat)} parameter. Using default menu formatting.");
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
                logger.LogException($"Exception in {nameof(QuickSlotExtender)}.{nameof(ExtendQuickSlots)}(int extendAmount, string menuDescriptionFormat). Extend failed.", ex);
                throw;
            }
            ExtendQuickSlots(menuDescriptions);
        }
    }
}
