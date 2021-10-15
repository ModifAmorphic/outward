using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Transmorph.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Transmog.SaveData
{
    internal class TransmogRecipeData
    {
        private readonly object _updateLock = new object();

        private IModifLogger Logger => _loggerFactory.Invoke();
        private readonly Func<IModifLogger> _loggerFactory;

        private readonly string _recipeFilePath;
        private Dictionary<int, UID> _recipeCache;
        public TransmogRecipeData(string configDirectory, Func<IModifLogger> loggerFactory)
        {
            _recipeFilePath = Path.Combine(configDirectory, ModInfo.ModId + ".transmogrify.recipes");
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
        public void SaveRecipe(int visualItemID, UID uid)
        {
            if (_recipeCache.ContainsKey(visualItemID) && _recipeCache[visualItemID] == uid)
                return;
            lock (_updateLock)
            {
                if (_recipeCache.ContainsKey(visualItemID) && _recipeCache[visualItemID] != uid)
                    _recipeCache[visualItemID] = uid;
                else
                    _recipeCache.Add(visualItemID, uid);

                var sb = new StringBuilder();
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
                Logger.LogTrace($"Existing recipe file not found at '{_recipeFilePath}'. Creating a new one.");
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
            foreach (var line in recipeLines)
            {
                var fields = line.Split(',');
                if (fields.Length == 2)
                    recipes.Add(int.Parse(fields[0]), fields[1].Trim());
            }
            return recipes;
        }
        private void CreateSaveFile()
        {
            var sb = new StringBuilder();
            foreach (var kvp in TransmogSettings.StartingTransmogRecipes)
                sb.AppendLine(kvp.Key + ", " + kvp.Value);

            File.WriteAllText(_recipeFilePath, sb.ToString());

            Logger.LogInfo($"Initialized new '{_recipeFilePath}' file. Created with " +
                $"{TransmogSettings.StartingTransmogRecipes.Count} recipes");
        }
    }
}
