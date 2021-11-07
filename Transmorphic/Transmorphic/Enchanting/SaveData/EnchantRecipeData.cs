using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.SaveData
{
    internal class EnchantRecipeData
    {
        private readonly object _updateLock = new object();

        const string RecipeFileHeader = "RecipeID, UID";
        private IModifLogger Logger => _loggerFactory.Invoke();
        private readonly Func<IModifLogger> _loggerFactory;

        private readonly string _recipeFilePath;
        private Dictionary<int, UID> _recipeCache;
        public EnchantRecipeData(string configDirectory, Func<IModifLogger> loggerFactory)
        {
            _recipeFilePath = Path.Combine(configDirectory, ModInfo.ModId + ".enchanting.recipes");
            _loggerFactory = loggerFactory;
        }

        
        public IReadOnlyDictionary<int, UID> GetAllRecipes()
        {
            if (_recipeCache == null)
            {
                LoadRecipesCache();
            }
            return _recipeCache;
        }
        public void SaveRecipe(EnchantRecipe recipe)
        {
            if (_recipeCache.ContainsKey(recipe.RecipeID) && _recipeCache[recipe.RecipeID] == recipe.UID)
                return;
            lock (_updateLock)
            {
                if (_recipeCache.ContainsKey(recipe.RecipeID) && _recipeCache[recipe.RecipeID] == recipe.UID)
                    _recipeCache[recipe.RecipeID] = recipe.UID;
                else
                    _recipeCache.Add(recipe.RecipeID, recipe.UID);

                var sb = new StringBuilder();
                sb.AppendLine(RecipeFileHeader);
                foreach (var kvp in _recipeCache)
                    sb.AppendLine(kvp.Key + ", " + kvp.Value);

                File.WriteAllText(_recipeFilePath, sb.ToString());
            }
        }
        private void LoadRecipesCache()
        {
            lock (_updateLock)
            {
                _recipeCache = CreateGetRecipes();
            }
        }
        private Dictionary<int, UID> CreateGetRecipes()
        {
            if (!File.Exists(_recipeFilePath))
            {
                Logger.LogDebug($"Existing recipe file not found at '{_recipeFilePath}'. Creating a new one.");
                CreateSaveFile();
            }

            var recipes = GetRecipes();
            if (recipes.Count == 0)
            {
                Logger.LogWarning($"Existing recipe file found but contained no recipes! Creating a new one. Save file: '{_recipeFilePath}'.");
                CreateSaveFile();
                recipes = GetRecipes();
            }

            Logger.LogDebug($"recipes null? {recipes == null}.");
            return recipes;
        }
        private Dictionary<int, UID> GetRecipes()
        {
            var recipes = new Dictionary<int, UID>();
            var recipeLines = File.ReadAllLines(_recipeFilePath);
            for (int lineNo = 1; lineNo < recipeLines.Length; lineNo++)
            {
                var fields = recipeLines[lineNo].Split(',');
                if (fields.Length == 2)
                    recipes.Add(int.Parse(fields[0]), fields[1].Trim());
            }
            return recipes;
        }
        private void CreateSaveFile()
        {

            File.WriteAllLines(_recipeFilePath, new string[1] { RecipeFileHeader });

            Logger.LogInfo($"Initialized new '{_recipeFilePath}' file.");
        }
    }
}
