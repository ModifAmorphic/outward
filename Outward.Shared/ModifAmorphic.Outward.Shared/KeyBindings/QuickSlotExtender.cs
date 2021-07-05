using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;
using ModifAmorphic.Outward.KeyBindings.Listeners;
using SideLoader;
using HarmonyLib;
using ModifAmorphic.Outward.Models;
using ModifAmorphic.Outward.Events;

namespace ModifAmorphic.Outward.KeyBindings
{
    public class QuickSlotExtender
    {
        private readonly Logging.Logger logger;
        private CustomKeybindings _customKeyBindings;

        private readonly int _startingId;
        private bool _lazyInitNeeded = true;
        private const string ActionNamePrefix = "QS_Instant";
        private const string ActionKeyPrefix = "InputAction_";
        private const string MenuSlotNoKey = "{ExtraSlotNumber}";
        private const string MenuDescriptionDefaultFormat = "Ex Quick Slot {ExtraSlotNumber}";

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public QuickSlotExtender(int startingId)
        {
            string loggerName = Assembly.GetCallingAssembly().ToString();
            this.logger = Logging.InternalLoggerFactory.GetLogger(Logging.LogLevel.Info, loggerName);
            this._startingId = startingId;
        }
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public QuickSlotExtender(int startingId, Logging.Logger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.logger = Logging.InternalLoggerFactory.GetLogger(logger);
            this._startingId = startingId;
        }
        public QuickSlotExtender(int startingId, string loggerName, Logging.LogLevel logLevel = Logging.LogLevel.Info)
        {
            if (string.IsNullOrEmpty(loggerName?.Trim()))
                throw new ArgumentNullException(nameof(loggerName));

            this.logger = Logging.InternalLoggerFactory.GetLogger(logLevel, loggerName);
            this._startingId = startingId;
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

        /// <summary>
        /// Add's a new quickslot to 
        /// </summary>
        /// <param name="quickSlotId"></param>
        /// <param name="description"></param>
        public void ExtendQuickSlots(Queue<string> menuDescriptions)
        {
            try
            {
                LazyInit();
                int extendAmount = menuDescriptions.Count;
                int x = 0;
                var exQuickSlots = new List<ExtendedQuickSlot>();

                while (menuDescriptions.Count > 0)
                {
                    int quickSlotId = x + this._startingId;
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

                logger.LogTrace($"RaiseSlotsChanged Event. StartId = {_startingId}, QuickSlotsToAdd = {exQuickSlots.Count}");
                //Raise event that slots changed so subscribers can react
                QuickSlotExtenderEvents.RaiseSlotsChanged(this, new QuickSlotExtendedArgs(_startingId, exQuickSlots));
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
