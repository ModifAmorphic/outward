# Summary
StashPacks adds special backpacks that link to player stashes. Drop a StashPack on the ground to access a stash from almost anywhere, including other people's games!
&nbsp;
- - - -
&nbsp;

# Compatibility
- This mod is not compatible with Shared_Stashes. It will not load if the Shared_Stashes mod is also enabled.

# Instructions
- Craft a StashPack (see description for tips).
- Be in a major town or have "Enable StashPacks for All Scenes" option enabled.
- Drop the StashPack from your inventory. Stash Items will load automatically.
- Access your Stash from the StashPack.
- Take your StashPack to automatically unlink it from the stash, or leave it in town for the next time you visit.
- Changes to any stash will be saved the next time the game saves.

# Description
Each Stash Backpack shares its origin with an area backpack in the list below. A special stashpack recipe is found wherever the area  backpack is found. If the area backpack or its recipe is sold at a vendor, then the recipe for the StashPack will be sold there as well.
## Stash Backpacks
  - Cierzo - [Mefino's Trade Backpack](https://outward.fandom.com/wiki/Mefino%27s_Trade_Backpack)
  - Berg - [Zhorn's Hunting Backpack](https://outward.fandom.com/wiki/Zhorn%27s_Hunting_Backpack)
  - Monsoon - [Glowstone Backpack](https://outward.fandom.com/wiki/Glowstone_Backpack)
  - Levant - [Brass-Wolf Backpack](https://outward.fandom.com/wiki/Brass-Wolf_Backpack)
  - Harmattan - [Boozu Hide Backpack](https://outward.fandom.com/wiki/Boozu_Hide_Backpack)
  - New Sirocco - [Weaver’s Backpack](https://outward.fandom.com/wiki/Weaver%E2%80%99s_Backpack)
 

StashPacks works by retrieving the contents of a stash whenever a stashpack is dropped on the ground.  Changes to a pack's contents are kept track of until the game saves, at which time those changes are applied to each stash area's save. (A ".bak" backup is made prior to updating the save file in your character's SaveGames\[CharacterID] folder).  You can bring your stashpack to another player's game and access your personal stash(es) from there as well. Any changes made to your stashpack will also be made in your game's home stash.  If another player picks up your pack and drops it, it will now link to their stash. But don't worry, any changes made to your stash prior to that are not lost. **A StashPack does not work when used in its home area if you are the host (or single player).** I may attempt to add this in a future release, but left it out for now due to complexity reasons.


# Important Notes
- StashPacks work in multiplayer, even when you're not the host, allowing you to access your stashes while in someone else's game.
- A StashPack will not work in its home scenario/area if you are the host. This is by design. Use the Stash in the area instead.
- StashPacks are not usable from inside your inventory, or other containers.
- Pressing the equip button when a StashPack is on the ground will automatically add it to your inventory.
- There's an option to disable crafting from any StashPacks you have on the ground (it is on by default).
- By default, stash packs are only usable in towns. There is a configuration option to enable them in all areas. Don't forget most areas outside towns reset after 7 days though!
- Duplicate dropped stashpacks will not work for the same player. This just means if you have the same bag dropped more than once, only the last bag you dropped will be linked to its home stash.
- If you "cheat" a stashpack into the game, make sure you pick it up before using. Stashpacks only work if they've been "owned" before.
- In multiplayer, the host controls a couple settings for compatibility. The Enabling/Disabling of stashpack functionality outside of towns and the scaling and rotation of bags.


# Change Logs
## Latest Release ${PACKAGE_VERSION}
- Fixed bug where taking a pack before it fully synced could cause it to stop working as a stash pack.
- Lots of fine tuning, internal rework of timed events around bags dropping, syncing and being picked up.
- Fixed issue with error being logged when main host left the game.
- Corrected a few typos.
- Removed Dependency on Mefino-Outward_Config_Manager.
- Fixed bug with ConfigurationManager install being required. A ConfigurationManager is now truly optional.
- *Internal* - Using BepInEx logger.
- *Internal* - ModifAmorphic.Outward.dll dependency packaged inside mod dll instead of a separate file.

## Initial Release 1.0.1
- Initial release.
