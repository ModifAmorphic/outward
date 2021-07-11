# Summary
Extra QuickSlots enables additional Keyboard QuickSlots and repositioning of the QuickSlot and Stability Bars.
## *** The Latest Beta version of this mod will reset all config settings back to their defaults! ***

1. Set the amount of extra keyboard Quick Slots.
2. Restart the game.
3. Set the positions of both bars.
\
&nbsp;
- - - -
# Configuration Instructions
## First-time Configuration
- Click "Start modded" in [r2modman](https://thunderstore.io/package/ebkr/r2modman/).
- Press F5 to open up the Configuration Manager.
- Expand the __Extra Quickslots X.X.X__ configuration.
- Set the first setting, __QuickSlots to add__, to the amount of Extra Quickslots you want. For example, entering 3 will add 3 quickslots to the existing 8 in the unmodded game for a total of 11.
- __Restart the game__.  (Exit and "Start modded" in [r2modman](https://thunderstore.io/package/ebkr/r2modman/) again)
  - Restarting is required whenever the __QuickSlots to add__ value is changed before it takes effect.

## UI / Alignment Options
- __Center QuickSlot bar__ - Centers the QuickSlot bar horizontaly. You will probably need to make additional adjustments as the QuickSlot and Stability Bars will likely overlap with this option.
- __QuickSlot and Stability Bar Alignment__
  - ___MoveQuickSlotAboveStability___: Moves the QuickSlot Bar above the Stability Bar. The __Y Offset QuickSlot Bar by__ setting allows for additional fine tuning of the QuickSlot Bar when this option is selected.
  - ___MoveStabilityAboveQuickSlot___: Moves the Stability Bar above the QuickSlot Bar. The __Y Offset Stability Bar by__ setting allows for additional fine tuning of the Stability Bar when this option is selected.
  - ___AbsolutePositioning___: Absolute X-Y coordinate positioning of the stability and quickslot bars.

## Configuring Hotkeys
- The Extra Quick Slots will be at the top of the __Quick Slots__ section of the __Keyboard__ settings, starting with "Ex Quickslot 1".
- Configure them to whatever you like, keeping in mind keys may already be assigned elsewhere.
- Assign Items or Skills to your new slots.

# ThunderStore with r2modman Installation Instructions
- Download and install [r2modman](https://thunderstore.io/package/ebkr/r2modman/)
  - For instructions, see the [Thunderstore](https://outward.fandom.com/wiki/Installing_Mods#Thunderstore) of the Outward Wiki
- Install SideLoader by sinai-dev [Thunderstore](https://outward.thunderstore.io/package/sinai-dev/SideLoader/)
- Install Outward Config Manager by Mefino [Thunderstore](https://outward.thunderstore.io/package/Mefino/Outward_Config_Manager/)
\
&nbsp;
# Change Logs

## Beta Version 0.4.0
- Beta release of Extra QuickSlots!
- Implemented minumum versions into config. Config to be recreated from scratch if minimum version is not met.
  - Noticed issues when updating from older versions of this mod caused by my inability to name anything correctly the first, second or third time. Decided on a clean start for Beta.
- Reworked to get rid of the "sidecar" shared mod that was loading alongside with ExtraSlots with its own configuration.

## Alpha Version 0.3.7
- Settings in ConfigurationManager use friendly display names instead of their internal variable / config names.
- Stability and Quickslot Bar positioning settings no longer advanced and show/hide depending on alignment setting selected.
- New Absolute Positoning option added to alignment options.
 - The QuickSlot* and Stability Bars will be moved to the bottom right corner the first time this new Alignment option is set.
   - *If the "Center QuickSlot bar" option is enabled, the X Position setting of the QuickSlot will be effectively ignored. Disable centering if you want to control the X position.
- *Internal* - Rewrite of how config settings and the configuration handler are managed. Again...

## Alpha Version 0.2.0
- Fix for infinite load issue when the  __"16 gamepad quickslots"__ setting was enabled in [Vheos Mod Pack](https://github.com/Vheos777/OutwardMods).
- UI Config changes.
  - Changing any UI setting should now be applied without restarting.
  - ExtraSlotsAlignmentOption renamed to __QuickSlotBarAlignmentOption__
  - Additional UI Options for offsetting Quickslot and Stability bars. (requires "Advance settings" to be enabled for now).
- All settings, except ExtraQuickSlots, should now be applied without requiring a restart.
- Lots of internal code cleanup

## Alpha Version 0.1.0
- Initial Release of Extra Quickslots

