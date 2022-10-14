using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Results;
using ModifAmorphic.Outward.Transmorphic.Enchanting.SaveData;
using ModifAmorphic.Outward.Transmorphic.Menu;
using ModifAmorphic.Outward.Transmorphic.Patches;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes
{
    internal class EnchantRecipeService
    {
        private readonly EnchantingSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;
        private readonly EnchantRecipeGenerator _recipeGenerator;
        private readonly EnchantPrefabs _enchantPrefabs;
        private readonly CustomCraftingModule _craftingModule;
        private readonly LevelCoroutines _coroutine;
        private readonly EnchantRecipeData _saveData;

        private static Dictionary<string, Recipe> _loadedRecipesRef;
        private Dictionary<int, EnchantRecipe> _itemEnchantRecipes = new Dictionary<int, EnchantRecipe>();

        public EnchantRecipeService(BaseUnityPlugin baseUnityPlugin,
                                EnchantRecipeGenerator recipeGenerator,
                                EnchantPrefabs enchantPrefabs,
                                CustomCraftingModule craftingModule,
                                LevelCoroutines coroutine,
                                EnchantRecipeData saveData,
                                EnchantingSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _recipeGenerator, _enchantPrefabs, _craftingModule, _coroutine, _saveData, _settings, _getLogger) =
                (baseUnityPlugin, recipeGenerator, enchantPrefabs, craftingModule, coroutine, saveData, settings, getLogger);
            if (!SideLoaderEx.TryHookOnPacksLoaded(this, RegisterEnchantRecipes))
                EnchantRecipeManagerPatches.LoadEnchantingRecipeAfter += (r) => RegisterEnchantRecipes();
            
            //EnchantItemManagerPatches.AfterRequestItemInitialization += RemoveEnchantPrefabs;

            //TransmorphNetworkLevelLoaderPatches.MidLoadLevelAfter += (n) => _coroutine.InvokeAfterLevelAndPlayersLoaded(n, LearnEnchantRecipes, 300, 1);
            
            craftingModule.CraftingMenuEvents.MenuHiding += RemoveTemporaryItems;
            //NetworkLevelLoader.Instance.onOverallLoadingDone += LearnEnchantRecipes;
            TransmorphNetworkLevelLoaderPatches.MidLoadLevelAfter += (n) => _coroutine.InvokeAfterLevelAndPlayersLoaded(n, LearnEnchantRecipes, 300, 1);
            EnchantCharacterRecipeKnowledgePatches.LearnRecipeBefore += TryLearnRecipeWithoutAchievements;
            //NetworkLevelLoader.Instance.onGameplayLoadingDone += () => preFabricator.ModifItemPrefabs.gameObject.SetActive(false);
            //NetworkLevelLoader.Instance.onAllPlayersLoadingDone += () => preFabricator.ModifItemPrefabs.gameObject.SetActive(false);
        }

        private void RemoveEnchantPrefabs(Item item)
        {
            if (item == null || item.ItemID > EnchantingSettings.RecipeResultStartID || item.ItemID < _enchantPrefabs.LastResultID)
                return;

            if (!(item is Equipment equipment))
                return;

            List<Item> itemsToInitialize = ItemManager.Instance.GetPrivateField<ItemManager, List<Item>>("m_itemsToInitialize");
            bool removedInit = false;
            if (itemsToInitialize.Contains(item))
            {
                itemsToInitialize.Remove(item);
                removedInit = true;
            }

            bool removeWorldItem = false;
            if (ItemManager.Instance.WorldItems.ContainsKey(item.UID))
            {
                ItemManager.Instance.WorldItems.Remove(item.UID);
                removeWorldItem = true;
                
            }
            if (removedInit || removeWorldItem)
            {
                var removeMessage = removedInit ? $"Removed equipment {equipment.name} from initialization." : string.Empty;
                removeMessage += removeWorldItem ? $" Removed equipment {equipment.name} from WorldItems." : string.Empty;
                Logger.LogDebug(removeMessage);
            }
        }

        private void RegisterEnchantRecipes()
        {
            var recipes = RecipeManager.Instance.GetEnchantmentRecipes();
            var savedUids = _saveData.GetAllRecipes();
            int recipesRegistered = 0;
            foreach (var r in recipes)
            {
                var recipeId = EnchantRecipeGenerator.GetRecipeID(r.RecipeID);
                
                if (!TryGetLoadedRecipe(recipeId, out EnchantRecipe enchantRecipe))
                {
                    enchantRecipe = _recipeGenerator.GetEnchantRecipe(r);
                    
                    if (!savedUids.TryGetValue(enchantRecipe.RecipeID, out var recipeUID))
                    {
                        _saveData.SaveRecipe(enchantRecipe);
                    }
                    else
                    {
                        enchantRecipe.SetUID(recipeUID);
                    }
                    _craftingModule.RegisterRecipe<EnchantingMenu>(enchantRecipe);
                    recipesRegistered++;
                    _itemEnchantRecipes.Add(enchantRecipe.Results[0].RefItem.ItemID, enchantRecipe);
                }
            }
            //ApplyPrefabEnchants();

            //_coroutine.DoNextFrame(() => _preFabricator.ModifItemPrefabs.gameObject.SetActive(false));
        }

        private void ApplyPrefabEnchants()
        {
            _coroutine.DoNextFrame(() =>
                {
                    foreach (var recipe in _itemEnchantRecipes.Values)
                    {
                        var equipment = (Equipment)recipe.Results[0].RefItem;

                        //Logger.LogDebug($"Calling ProcessInit() on equipment {equipment.name}.");
                        equipment.ProcessInit();
                        
                        if (!equipment.ActiveEnchantmentIDs.Contains(recipe.BaseEnchantmentRecipe.ResultID))
                        {
                            equipment.AddEnchantment(recipe.BaseEnchantmentRecipe.ResultID);
                            Logger.LogDebug($"Added enchantment recipe {recipe.BaseEnchantmentRecipe.name} to equipment {equipment.name}.");
                        }
                        //equipment.gameObject.SetActive(false);
                    }
                });
        }

        public bool TryGetLoadedRecipe<T>(int recipeID, out T recipe) where T : Recipe
        {
            if (_loadedRecipesRef == null)
                _loadedRecipesRef = RecipeManager.Instance.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            if (!_loadedRecipesRef?.Any() ?? false)
                throw new InvalidOperationException("Cannot check if a recipe exists before the RecipeManager has loaded recipes.");
            
            recipe = (T)_loadedRecipesRef.Values.FirstOrDefault(r => r.RecipeID == recipeID);
            Logger.LogTrace($"TryGetLoadedRecipe: Result: " +
                $"{(recipe != default ? "Searched for " + recipeID + ". Found " + recipe.RecipeID + " - " + recipe.Name : recipeID + " - Not Found.")}");
            return recipe != default;
        }

        private void DelayedLearnCharacterRecipes(Character character, List<Recipe> recipes)
        {
            bool recipesLoaded() => character.Inventory.RecipeKnowledge.GetPrivateField<CharacterRecipeKnowledge, bool>("m_recipeLoaded");
            _coroutine.DoWhen(recipesLoaded, () => LearnCharacterRecipes(character, recipes), 300, .25f);
        }
        private void LearnCharacterRecipes(Character character, List<Recipe> recipes)
        {
            foreach (var r in recipes)
                TryLearnRecipe(character.Inventory, r);
        }
        private void LearnEnchantRecipes()
        {
            var characters = SplitScreenManager.Instance.LocalPlayers.Select(p => p.AssignedCharacter);
            var allRecipes = _craftingModule.GetRegisteredRecipes<EnchantingMenu>();
            foreach (var c in characters)
            {
                DelayedLearnCharacterRecipes(c, allRecipes);
                //foreach(var r in allRecipes)
                //    TryLearnRecipe(c.Inventory, r);
            }
        }
        private bool TryLearnRecipe(CharacterInventory inventory, Recipe recipe)
        {
            Logger.LogDebug($"Trying to learn recipe {recipe.name}.");
            if (!inventory.RecipeKnowledge.IsRecipeLearned(recipe.UID))
            {
                inventory.RecipeKnowledge.LearnRecipe(recipe);
                Logger.LogInfo($"Character learned new Enchanting Recipe {recipe.Name}.");
                return true;
            }
            return false;
        }
        private void TryLearnRecipeWithoutAchievements(CharacterRecipeKnowledge knowledge, EnchantRecipe recipe)
        {
            if (knowledge.IsRecipeLearned(recipe.UID))
                return;

            var knownUIDs = knowledge.GetPrivateField<CharacterKnowledge, List<string>>("m_learnedItemUIDs");
            var character = knowledge.GetPrivateField<CharacterKnowledge, Character>("m_character");

            knownUIDs.Add(recipe.UID);
            if (NetworkLevelLoader.Instance.IsOverallLoadingDone && character.Initialized)
            {
                if ((bool)character && (bool)character.CharacterUI)
                {
                    string loc = LocalizationManager.Instance.GetLoc("Notification_Item_RecipeLearnt", recipe.Name);
                    //character.CharacterUI.ShowInfoNotification(loc, ItemManager.Instance.RecipeLearntIcon, _itemLayout: true);
                }
                //Exclude acheivement tracking since these are so simple to come by
            }
        }

        private void RemoveTemporaryItems(CustomCraftingMenu menu)
        {
            if (!(menu is EnchantingMenu))
                return;

            _enchantPrefabs.RemoveTemporaryItems();
            ResetRecipesResults();
        }
        private void ResetRecipesResults()
        {
            foreach (var recipe in _craftingModule.GetRegisteredRecipes<EnchantingMenu>())
            {
                var enchantRecipe = (EnchantRecipe)recipe;
                enchantRecipe.SetResults(enchantRecipe.GetDefaultCraftingResults());
            }
        }
        private void PatchRecipeItems(ref Dictionary<int, EnchantmentRecipeItem> recipesItems)
        {
            //fix for Formless EnchantmentRecipeItem containing 2 Helm Recipes instead of 1 Helm, 1 Boots
            if (recipesItems.TryGetValue(75, out var formless))
                recipesItems.Add(76, formless);

            //fix for Filter EnchantmentRecipeItem Missing Helm and Boots Recipes
            if (recipesItems.TryGetValue(52, out var filter))
            {
                recipesItems.Add(53, filter); //53_Filter(Helmet)_EnchantmentRecipe
                recipesItems.Add(54, filter); //54_Filter(Boots)_EnchantmentRecipe
            }
        }
        
    }
}
