# Summary
Extra QuickSlots enables additional Keyboard QuickSlots and repositioning of the QuickSlot and Stability Bars.

1. Set the amount of extra keyboard Quick Slots.
2. Restart the game.
3. Set the positions of both bars.

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

&nbsp;
# Change Logs

## Latest Version ${PACKAGE_VERSION}
- Fix for localization error occuring on startup.

## Version ${1.0.3}
- Built using Definitive Edition Dlls.

## Version 1.0.2
- Removed Dependency on Mefino-Outward_Config_Manager.
- Fixed bug with ConfigurationManager install being required. A ConfigurationManager is now truly optional.

## Version 1.0.1
- Repositioning of stability bar should now work consistently without requiring a restart.
- Quickslot bar absolute positioning now uses the left bottom corner instead of the center X position.
- Updated internal logging to BepInEx logger.
- Other Minor changes for release.

## Beta Version 0.4.0
- Beta release of Extra QuickSlots!
- Implemented minumum versions into config. Config to be recreated from scratch if minimum version is not met.
  - Noticed issues when updating from older versions of this mod caused by my inability to name anything correctly the first, second or third time. Decided on a clean start for Beta.
- Reworked to get rid of the "sidecar" shared mod that was loading alongside with ExtraSlots with its own configuration.

