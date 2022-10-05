# ![Action UI](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/Banner.png?raw=true)
⚡ Additional Quickslots, UI Positioning, Durability Display ⚡

Replaces the Outward's built in quickslots with a highly configurable one from this mod. The new Hotbar adds additional slots (referred to as action slots), multiple hotbars and even dividing a hotbar into multiple rows.  In addition, this mod enables moving of many UI elements in the base game and those added by Action UI.  There's also a new durability UI to display worn and broken equipment.

## Configuration Menus

Action UI has many configuration options available. To view them, open the in game pause menu and click on the "Action UI" button. Configuration of Action UI is split between two menus, the main UI settings menu and the configure Hotbars menu.

![Action UI Menu Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/PauseMenu.png?raw=true)

### Action UI Settings
The main Action UI menu contains settings for creating or editing profiles, enabling or disabling Action Slots and Durability Components and moving or resetting UI positions as show below.

![Action UI Settings](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/MainSettingsMenu.png?raw=true)

#### Profile
Create, select or rename an existing profile with the profile setting options. A Profile contains all of the configuration changes that were made with Action UI.  This includes UI positioning, hotkeys, hotbar configuration and slot assignments in addition to all of the settings listed on the Main and Hotbar settings menus.

##### Default Profile
Action UI automatically creates a default "Profile 1" profile the first time the Action UI mod is ran. This profile can be renamed using the rename button ![Rename Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/RenameButton.png?raw=true). For starting out, this single default profile is all you'll need. A new profile can always be created later, which will carry over all the changes made to the first default profile.

##### New Profile
A New Profile can be created by selecting the "[New Profile]" option from the profile drop down. Upon selection of "[New Profile]", enter the name of profile you wish to be created. If you are unable to create a new profile, make sure your name meets [Windows File Naming Conventions](https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions). Creating a new profile saves all of the current changes, essentially making a copy of the existing profile as starting point. Any additional changes made will only be saved to the selected profile.

##### Renaming a Profile
Select the profile you wish to rename from the drop down, click the the rename button ![Rename Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/RenameButton.png?raw=true) and type in a new name. That's it. Like new profiles, renaming a profile must follow the same [Windows File Naming Conventions](https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions).

#### Component Toggles
- The "Action Slots (Restart)" option enables or disables Action UI's configurable Hotbars.  A **game restart** is required for this to take effect. Disabling enables Outward's built in Quick Slots.
- "Durability Display" toggles the durability display on and off. No restart required.
- "Equipment Sets" toggles Equipment Sets addon menu and skills.
- "Craft from Stash" allows use of items in crafting menus directly from the character's stash.
  - "Outside Towns" enables "Craft from Stash" anywhere.

***

### Hotbar Settings
Add multiple Hotbars, split a Hotbar into rows, add additional Action Slots (Quickslots), and configure Action Slot hotkeys and appearance.

![Hotbar Settings](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/HotbarSettingsView.png?raw=true)

#### Multiple Hotbars, Rows, Slots
Setting | Description
--- | ---
![Hotbars Setting](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/HotbarsSetting.png?raw=true) | Multiple Hotbar setting. Increasing this value adds additional Hotbars that can be switched to using the next, previous or Hotbar Hotkeys.
![Hotbar Rows](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/HotbarRowsSetting.png?raw=true) | Adds an additional row of Action Slots to each row. Each row will contain the number of Action Slots configure with the Action Slots setting. For example, if "Action Slots" is 11 and "Hotbar Rows" is 2, then 2 rows of 11 Action Slots will be shown for each Hotbar.
![ActionSlots](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ActionSlotsSetting.png?raw=true) | Adds an additional row of Action Slots to each row. Each row will contain the number of Action Slots configure with the Action Slots setting. For example, if "Action Slots" is 11 and "Hotbar Rows" is 2, then 2 rows of 11 Action Slots will be shown for each Hotbar.

![Hotbar Example](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/Hotbar.png?raw=true)

#### Show Cooldown Timer
Enables a numeric countdown display for each Action Slot representing the time left before an action becomes available to use.
- Precision Time shows the tenths decimal place if the time remaining is less than 10.

#### Empty Slots
Configures how an Action Slot is displayed if no action is assigned to it.
Option | Image | Description
--- | --- | ---
Transparent | ![Transparent Action Slot](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ActionSlotEmptyTransparent.png?raw=true) | Empty Action Slots have a transparent background.
Image | ![Empty Image Action Slot](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ActionSlotEmptyImage.png?raw=true) | A solid black image is shown as a placeholder in each empty Action Slot.
Hidden | N/A | The Action Slot is hidden completely. To assign an action to a hidden slot, change this setting to either the Transparent or Image option and assign the action then switch back to Hidden.

#### Combat Mode
Combat Mode disables the use of all extra Hotbars and Action Slots beyond the first Hotbar and 8 Action Slots whenever a character enters combat. If you wish to have all Action Slots available in and out of combat at all times, then disable this setting.

Why does Combat mode exist? Outward is balanced around only having 8 actions available in combat, the 8 assignable Quick Slots in the base game.  This mode was created to maintain that balance while still allowing additional actions to be hotkeyed outside of combat.  Basically, this mode exists to alleviate some of the tedium of the digging around in the character menus to set traps, cast boons, deploy tents, campfires, cooking pots, etc outside of combat, which is one of my least favorite parts of Outward.

***

### Hotkey Capture Mode
Hotkeys for switching Hotbars and triggering Action Slots are configured through Hotkey Capture Mode.  This mode is enabled by clicking the "Set Hotkeys button ![Set Hotkeys Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/SetHotkeys.png?raw=true) on the [Hotbar Settings](https://github.com/ModifAmorphic/outward/wiki/Action-UI#hotbar-settings) menu.  Once in this mode, simply click the Hotkey button you wish to change as shown below and press a key or mouse button you wish to assign.

![Hotkey Capture](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/HotbarCapture.png?raw=true)

***

### Equipment Set Settings
Configure various Equipment Set options, from allowing armor swaps in combat to equipping directly from the character's stash.

![Equipment Set Settings](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetSettingsView.png?raw=true)

#### Use Armor Sets in combat
Allows swapping of equipment sets while in combat.

#### Skip weapon animations on equip
Skips equipping and unequipping of main and off hand equipment.

#### Equip Sets from Stash
Armor or Weapon Sets will be equipped directly from the character's stash. Also makes items from the stash available in Outward's Equipment menu. Requires the character be in a town where a stash is present unless the "Stash available anywhere" option is enabled.

##### Stash available anywhere
"Equip Sets from Stash" enabled outside towns.

#### Unequip Sets from Stash
Armor or Weapon Sets will be unequipped directly to the character's stash. Equipment that is not assigned to an Armor or Weapon Set will not be sent to the character's stash however. Instead, it will be sent to the character's inventory. Requires the character be in a town where a stash is present unless the "Stash available anywhere" option is enabled.

##### Stash available anywhere
"Unequip Sets from Stash" enabled outside towns.

***

## Assigning Actions / QuickSlots
There are 2 ways to assign an action to an Action Slot.  The first is to open up Outward's inventory or skill menu and simply drag the item or skill to the desired slot. Not all items are slottable, so if you are unable assign an action to a slot this is probably why.  To clear an existing action from an Action Slot, right-click the slot.

![Drag and Drop to Action Slots](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ActionSlotsDragging.png?raw=true)

The second method of assigning actions is to use Action UI's Action Viewer menu.  To open the viewer, first open up an Outward inventory (or any other menu that gives you a mouse pointer besides the pause menu.  Then click on the Action Slot you wish to assign.  The Action Viewer will be displayed with a list of tabs on the left to switch between different action categories.  Clicking on an action assigns that action the Action Slot.

![Action Viewer](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ActionViewer.png?raw=true)

### Assign Action Slots Video
[ ![Assign Actions YouTube Video](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/AssignActionSlotVideo.png?raw=true) ](https://youtu.be/nJT76DLFIqw)

***

## Equipment Sets
Armor and Weapon Sets are managed using an addon to Outward's built in Equipment Menu, broken into a separate view for each. New equipment sets are created using the ![Add Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/AddButton.png?raw=true), which prompts for the name of the new set.  Sets can be renamed with the ![Rename Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/RenameButton.png?raw=true). Any existing set can be altered by first selecting it in the dropdown, changing gear in the equipment menu, and saving.  Delete simply deletes the existing set and the skill associated with it.

![Equipment Sets Addon Menu](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetsMenu.png?raw=true)

### Icon Setting
The Icon setting allows you to select which Equipment Icon should be shown as the Equipment Set Skill created for each set.

### Equipment Set Skills
Creating an Equipment set in the Equipment Menu also adds a corresponding Skill for the owning character. These skills can be found under Outward's Skills menu, in the Active Skills tab. Once added, these skills can be assigned to an Action Slot or one of Outward's Keyboard or Controller Quickslots. Equipment Set skills have an [Equipment Set Icon](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetIcon.png?raw=true) icon in the bottom right corner to help distinguish them from other skills.

![Equipment Set Skills](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/EquipmentSetSkills.png?raw=true)

***

## UI Positioning Mode
Action Menu's includes a UI Positioning Mode that allows for many UI elements in Outward to be positioned by dragging and dropping them to wherever you want.  To enter UI Positioning Mode by clicking on the "Move UI Elements" ![Move UI Elements](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/MoveUIElements.png?raw=true) button in the Action UI Settings Menu [Action UI Settings Menu](https://github.com/ModifAmorphic/outward/wiki/Action-UI#action-ui-settings).

![UI Positioning Mode](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/UIPositioning.png?raw=true)

Click the Reset Button ![Reset Button](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ResetUIElement.png?raw=true) of an individual UI element to rest it back to it's original position.  To reset all UI Positions of all UI Elements, click the "Reset UI Positions" button [Reset UI Positions](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/ResetUI.png?raw=true) in the Action UI Settings Menu [Action UI Settings Menu](https://github.com/ModifAmorphic/outward/wiki/Action-UI#action-ui-settings).

### UI Positioning Video
[ ![UI Positioning YouTube Video](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/UIPositioningVideo.png?raw=true) ](https://youtu.be/zoY1qEdeATg)

***

## Durability Display
Action UI adds a display to track equipment durability without needing to examine each individual piece.

![Durability Display](https://github.com/ModifAmorphic/outward/blob/master/ActionUI/WikiReadmeAssets/DurabilityDisplay.png?raw=true)

Durability Threshold | Stat Potency | Color
--- | --- | ---
100% to 50% | 1.0x (100%) | Gray or Not Displayed
50% to 25% | 0.8x (80%) | Yellow
25% to 0% | 0.5x (50%) | Orange
Broken (0%) | Broken (0%) | Red

For more details on how durability works, consult the [Outward Wiki](https://outward.fandom.com/wiki/Equipment#Durability)