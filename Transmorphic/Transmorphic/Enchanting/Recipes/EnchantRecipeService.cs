using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
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
                TransmorphRecipeManagerPatches.LoadCraftingRecipeAfter += (r) => RegisterEnchantRecipes();
            TransmorphNetworkLevelLoaderPatches.MidLoadLevelAfter += (n) => _coroutine.InvokeAfterLevelAndPlayersLoaded(n, LearnEnchantRecipes, 300, 1);

            //_settings.AllEnchantRecipesLearnedEnabled += LearnEnchantRecipes;
            EnchantCharacterRecipeKnowledgePatches.LearnRecipeBefore += TryLearnRecipeWithoutAchievements;
            craftingModule.CraftingMenuEvents.MenuHiding += RemoveTemporaryItems;
        }

        private static Dictionary<string, Recipe> _loadedRecipesRef;
        private void RegisterEnchantRecipes()
        {
            var recipes = RecipeManager.Instance.GetEnchantmentRecipes();
            var savedUids = _saveData.GetAllRecipes();
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
                }
            }
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
        private void LearnEnchantRecipes()
        {
            //if (_settings.AllEnchantRecipesLearned)
            //{
            var characters = SplitScreenManager.Instance.LocalPlayers.Select(p => p.AssignedCharacter);
            var allRecipes = _craftingModule.GetRegisteredRecipes<EnchantingMenu>();
            Logger.LogInfo($"Setting [AllEnchantRecipesLearned] is enabled. {characters?.Count() ?? 0} Characters will learn all {allRecipes.Count} enchantment recipes.");
            foreach (var c in characters)
            {
                foreach(var r in allRecipes)
                    TryLearnRecipe(c.Inventory, r);
            }
            //}
        }
        private bool TryLearnRecipe(CharacterInventory inventory, Recipe recipe)
        {
            if (!inventory.RecipeKnowledge.IsRecipeLearned(recipe.UID))
            {
                inventory.RecipeKnowledge.LearnRecipe(recipe);
                Logger.LogInfo($"Character Learned new Enchanting Recipe {recipe.Name}.");
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
                    character.CharacterUI.ShowInfoNotification(loc, ItemManager.Instance.RecipeLearntIcon, _itemLayout: true);
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

        private Dictionary<int, EnchantmentRecipeItem> GetEnchantmentRecipeItem()
        {
            var field = typeof(ResourcesPrefabManager).GetField("ITEM_PREFABS", BindingFlags.NonPublic | BindingFlags.Static);
            var itemPrefabs = (Dictionary<string, Item>)field.GetValue(null);
            var recipeItems = itemPrefabs.Values.Select(p => p as EnchantmentRecipeItem).Where(r => r != null && r.Recipes?.Length > 0);
            var enchantsRecipes = new Dictionary<int, EnchantmentRecipeItem>();

            foreach (var i in recipeItems)
            {
                foreach (var r in i.Recipes)
                {
                    if (!enchantsRecipes.ContainsKey(r.RecipeID))
                        enchantsRecipes.Add(r.RecipeID, i);
                }
            }

            PatchRecipeItems(ref enchantsRecipes);
            return enchantsRecipes;
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
