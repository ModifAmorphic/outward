## __Alpha Version__
- - - -
  
    
# ThunderStore with r2modman Installation Instructions
## Dependencies
- Download and install [r2modman](https://thunderstore.io/package/ebkr/r2modman/)
  - For instructions, see the [Thunderstore](https://outward.fandom.com/wiki/Installing_Mods#Thunderstore) of the Outward Wiki
- Install SideLoader by sinai-dev [Thunderstore](https://outward.thunderstore.io/package/sinai-dev/SideLoader/)
- Install Outward Config Manager by Mefino [Thunderstore](https://outward.thunderstore.io/package/Mefino/Outward_Config_Manager/)

## First-time Configuration
- Click "Start modded" in [r2modman](https://thunderstore.io/package/ebkr/r2modman/).
- Press F5 to open up the Configuration Manager.
- Expand the __Extra Quickslots X.X.X__ section.
- Set the first setting, __ExtraQuickSlots__, to the amount of Extra Quickslots you want. For example, entering 3 will add 3 quickslots to the existing 8 in the base game for a total of 11.
- __Restart the game__.  (Exit and "Start modded" in [r2modman](https://thunderstore.io/package/ebkr/r2modman/) again)
  - Restarting is required whenever this value, __ExtraQuickSlots__, is changed before it takes effect.

## UI / Alignment Options
- __CenterQuickslotPanel__ - Just like it sounds, moves the QuickSlot bar to the horizontal center of the screen. You will probably need to make additional adjustments as the QuickSlot and Stability Bars will likely overlap.
- QuickSlotBarAlignmentOption
  - __MoveQuickSlotAboveStability__: Moves the QuickSlot Bar above the Stability Bar.
  - __MoveStabilityAboveQuickSlot__: Moves the Stability Bar above the QuickSlot Bar.

## Configuring Hotkeys
- The Extra Quick Slots will be at the top of the __Quick Slots__ section of the Keyboard settings.
- Configure them to whatever you like, keeping in mind keys may already be assigned elsewhere.
- Assign Items or Skills to your new slots.


---


# Change Logs

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

