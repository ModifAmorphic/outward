using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings;
using ModifAmorphic.Outward.Modules.QuickSlots.KeyBindings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ModifAmorphic.Outward.Modules.QuickSlots
{
    public class QuickSlotExtender : IModifModule
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly QsMenuExtender _qsMenuExtender;

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(CharacterQuickSlotManagerPatches),
            typeof(KeyboardQuickSlotPanelPatches),
            typeof(LocalCharacterControlPatches),
            typeof(QsLocalizationManagerPatches)
        };
        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(CharacterQuickSlotManagerPatches),
            typeof(KeyboardQuickSlotPanelPatches),
            typeof(LocalCharacterControlPatches),
            typeof(QsLocalizationManagerPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(CharacterQuickSlotManagerPatches),
            typeof(KeyboardQuickSlotPanelPatches),
            typeof(LocalCharacterControlPatches),
            typeof(QsLocalizationManagerPatches)
        };

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        internal QuickSlotExtender(QsMenuExtender qsMenuExtender, Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            this._qsMenuExtender = qsMenuExtender;
        }

        /// <summary>
        /// Generates Keybinding descriptions and adds additional QuickSlot keybindings to the "Quick Slot" section under the Settings --> Keyboard menu.
        /// Raises the <see cref="QuickSlotExtenderEvents.RaiseSlotsChanged(object, QuickSlotExtendedArgs)"/> event when complete.
        /// </summary>
        /// <param name="extendAmount">Amount of additional quickslots to add</param>
        /// <param name="menuDescriptionFormat">Format containing '{MenuSlotNoKey}' token to be replaced used as the label/description under the "Quick Slot" section for each additonal quickslot added. 
        /// For example, "Ex Slot {MenuSlotNoKey}" will label a new Quick Slot keybinding with a description of "Ex Slot 1", "Ex Slot 2", etc for the total number of <paramref name="extendAmount"/></param>
        public void ExtendQuickSlots(int extendAmount, string menuDescriptionFormat)
        {
            //Add new keybindings to the "Quick Slots" section of the Settings --> Keyboard menu.
            var extQslots = _qsMenuExtender.ExtendQuickSlots(extendAmount, menuDescriptionFormat);

            //Configure patches
            //I believe this configures the Character UI, where actions are assigned to individual quickslots
            CharacterQuickSlotManagerPatches.Configure(extQslots.Count);
            //Sets quickslots to be added to the keyboard panel, which is the quickslot bar shown when playing the game (not in menus).
            KeyboardQuickSlotPanelPatches.Configure(extQslots.Count, extQslots.First().QuickSlotId);

            //Provide quickslot defs so keypresses can be recognized and routed accordingly.
            LocalCharacterControlPatches.Configure(extQslots);

            QsLocalizationManagerPatches.Configure(extQslots);
        }
    }
}
