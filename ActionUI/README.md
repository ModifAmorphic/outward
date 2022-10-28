# ![Action UI](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/Banner.png?raw=true)

## Expand Quickslots, Create Equipment Sets, Show Equipment Durability, Reposition HUD UI Elements, Stash and Storage Improvements

***

- Add Highly Configurable Multiple Hotbars and Action Slots (Quick Slots)
  - Combat Mode: Extra Slots without ruining game balance.
  - Scale size of hotbars.
  - Add additional action slots, rows of slots and/or new hotbars.
  - See [wiki](https://github.com/ModifAmorphic/outward/wiki/Action-UI#hotbar-settings) for more.
- Create Weapon and Armor Sets. Equip entire sets with a single hotkey.
- Reposition UI Elements with Drag and Drop
- Equipment Durability UI displays when equipment is damaged.
- Use stash while crafting, selling, equiping gear or in the character menu.
- See the [Action UI Wiki](https://github.com/ModifAmorphic/outward/wiki/Action-UI) for more details
- For Manual Installations, download the latest "ModifAmorphic-ActionUI-*.Standalone-BepInEx.zip" version under [ModifAmorphic Releases](https://github.com/ModifAmorphic/outward/releases)

***

## Highly Configurabe Hotbars and Action Slots

![Hotbar Settings](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/HotbarSettingsView_small.png?raw=true)

![Hotbars](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/Hotbar.png?raw=true)

### Drag Actions to Slots right from Inventory!

[ ![Assign Actions YouTube Video](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/AssignActionSlotVideo.png?raw=true) ](https://youtu.be/nJT76DLFIqw)

***

## Equipment Sets and Quickslotable Skills

![Equipment Sets Settings](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetSettingsView_small.png?raw=true)

### Create and Customize Equipment Sets

![Equipment Sets](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetsMenu_small.png?raw=true)

### New Equipment Set Skills, slottable in Outward's Quickslots or ActionUI's Action Slots.

![Equipment Set Skills](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetSkills_small.png?raw=true)

***

## Reposition UI Elements

[ ![UI Positioning YouTube Video](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/UIPositioningVideo.png?raw=true) ](https://youtu.be/zoY1qEdeATg)

***

## Equipment Durability UI

![Durability Display](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/DurabilityDisplay.png?raw=true)

***

## Storage Improvements

![Storage Settings](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/StorageSettingsView_small.png?raw=true)

***

## Latest Release ${PACKAGE_VERSION}
- Stash Updates and Fixes
  - Renamed section to "Storage" from "Stash".
  - Added new option to settings to display item currency values in stash and character inventory.
  - Added "Move to Stash" button to item context menu. Button is only displayed when stash use is enabled.
  - Added new "Open Stash" button in the character inventory to the right of the "Stash" label. Clicking opens the same Stash Panel that shown when opening a stash in a player home.
  - Pressing Ctrl and Left Clicking on an item in the character's inventory (backpack or pouch) will now move it to the stash.
  - Clicking on an item in the stash will now move it to the backpack or pouch.
  - Fix food decay in multiplayer. Client's will now have food decay based on the hosts configuration.
    - Previously, food in client stashes was decaying at it's base rate, using neither client nor hosts stash configuration.
- Durability Display fixes
  - Durability display should now track equipment when first loading, without needing to unequip and equip.
  - Fix for Durability Display not displaying after first save is loaded without switching gear.
- Action Slots Updates and Fixes
  - Added new "Hide Left Navigation" option in the Hotkey Settings menu that hides the UI element left of the hotbar which displays the current hotbar number and hotkeys.
  - "Show HUD" in Outward's setting menu will now also hide Hotbars when disabled.
  - Added "Scale Hotbars %" to adjust size of hotbars.
  - Empty Action Slots without a hotkey bound will now only show when the "Action UI" settings menu is open or Hotkeys are being assigned.
  - Action Slots no longer stop tracking stack counts after switching from keyboard to controller and back to keyboard.
  - Fix for Action UI settings not displaying correctly when Action Slots are disabled.
- Equipment Set Fixes
  - Equipment sets should now unequip to stash correctly when hosting a game.
  - Several fixes for swapping between 2h and 1h weapon sets, or sets with empty slots.

## Release 1.1.2
  - Characters should no longer be stuck in a T-pose when joining another game.
  - Equipment Sets equip from stash fix. Set items could be equipped from stash even with the option disabled if the character or merchant stash options where enabled.
  - Action UI settings menu should now open correctly after a split screen session.
  - Fixed bug where user would be prompted to move items from stash to the stash when using the Stash Chest.
  - Fixed issue where a 2h weapon set would be unusable if the set was saved in 1h mode using the Mixed Grip mod.

## Release 1.1.1
  - Fix for Action Slot hotkeys not saving across play sessions.
    - Technically, they were saving but Outward was overriding them with an older copy on load. Fix ensures Outward no longer stores it's own copy in the "Player{0}_Keymappings.xml" file. Existing older mappings should automatically be removed on the next game exit.
  - Corrected display issue with Hotkey Capture. Capture text was displaying "Backspace" and "Tab" when keys "8" or "9" were pressed.

## Release 1.1.0
  - Added new Stash settings menu that enables the stash in several character menus. In town only by default.
    - Added stash to the bottom of the inventory menu.
    - Added stash to the bottom of the equipment menu. Shown when selecting different equipment for an equipment slot.
    - Added stash to the bottom of the merchant menu.
    - Added option to enable preservation of food in the stash.
    - Removed Crafting from stash option from main settings. It's under the Stash
  - Action Slots hotbar should now be hidden when a controller is used.
  - Equipment will no longer be equipped from stash if the "Unequip Sets to Stash" option is enabled but the "Equip Sets from Stash" is not.
  - Equipment Set fixes for multiplayer
    - Equipment set skills should now be added to the character when a new equipment set is created by a non host player.
  - Major rework around how modules are applied and profile data is loaded and saved.
    - Added force save of profile data whenever a new profile is created or a different profile is selected.
    - Saving all profile data on exiting a save.
    - Changes to how several modifications to UIs are made to be more resilient and only apply when necessary.
    - Refactored startup routines so that if one sub module fails others will still attempt to start.
    - Fix for Hotbars failing to load if more than one profile was manually deleted from the config folder.

## Release 1.0.5
  - Multiplayer Fixes
    - ActionUI should function in multiplayer games.
    - Splitscreen should now work when player 2 is assigned the keyboard.
  - Equipping a set with a 2h Weapon will now only play the unequip/equip animation once. Was previously playing twice, once for each hand.
  - Fixes for sets erroring when a set slot was empty.
  - Items with stack amounts (arrows, potions, etc) should now display the quantity when added to an action slot without requiring a scene change / reload.
  - Hidden Action Slots are now visible and assignable when the character menu is open.
  - Fixed dropdown menus.

## Release 1.0.4
  - Bug fix for "floating" backpacks.
    - Backpacks or items could remain stuck floating in the air if the item was slotted one of the action slots higher than Outwards base quickslots. Outward technically has 11 quickslots, so any amount of action slots higher than 11 could cause this issue if an item was slotted to it.

## Release 1.0.3
  - Fix for drag and drop issue. Skills could be dragged and destroyed. Items couldn't be dragged to action slots.

## Release 1.0.2
  - Weapon and Equipment Sets functionality added.
  - Craft from stash now limited to in town only by default with an option to enable it from any scene.
  - Optimized settings menu so Action Slots were only reloaded for action slots changes. Should remove most of the lag from changing other menu options.
  - Stack counts now show for ammunition (arrows, bullets).
  - Empty Action Slots no longer show as disabled if a disabled skill / item was removed from the slot.
  - Fixed bug where equipment showed the broken icon in an action slot if the scene was loaded without the item in the characters inventory.
  - Switching to controller and back to keyboard should no longer cause action slots to stop updating cooldowns, stack counts, and enabled status.
  - Fixed several issues when transitioning between the main menu and continuing other saves after the first character save is loaded.
  - Durability Display should no properly track equipped boots.
  - Fixed issue where items could not be drag and dropped from the inventory menu.

## Release 1.0.1
  - Fixes profile character switching bug. 
    - Going back to the main menu and continuing a different character's was causing the wrong ActionUI profile to be loaded. The profile for the first character loaded after launching the game was always being used.
  - Fixes for navigating settings menus with keyboard or controller.
  - Added Craft from Stash option.

## Initial Release 1.0.0
  - Initial Release
  - Keyboard Quickslots replaced with Custom Hotbar and Action Slots
  - Durability Display UI
  - Drag and Drop Positioning of UI