# ![Action UI](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/Banner.png?raw=true)

## ⚡ Equipment Sets, Additional Quickslots, UI Positioning, Durability Display ⚡

***

- Add Multiple Hotbars and Action Slots (Quick Slots)
  - Combat Mode: Extra Slots without ruining game balance.
- Create Weapon and Armor Sets. Equip entire sets with a single hotkey.
- Reposition UI Elements with Drag and Drop
- New Durability UI Displays Equipment
- See the [Action UI Wiki](https://github.com/ModifAmorphic/outward/wiki/Action-UI) for more details

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

## Latest Release ${PACKAGE_VERSION}
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