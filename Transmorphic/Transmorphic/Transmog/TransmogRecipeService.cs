using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Menu;
using ModifAmorphic.Outward.Transmorphic.Patches;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using ModifAmorphic.Outward.Transmorphic.Transmog.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    internal class TransmogRecipeService
    {
        private readonly TransmogSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;
        private readonly LevelCoroutines _coroutine;
        private readonly TransmogRecipeGenerator _recipeGen;
        private readonly CustomCraftingModule _craftingModule;
        private readonly TransmogRecipeData _recipeSaveData;
        private readonly PreFabricator _preFabricator;

        public TransmogRecipeService(BaseUnityPlugin baseUnityPlugin,
                                TransmogRecipeGenerator recipeGen,
                                CustomCraftingModule craftingModule,
                                PreFabricator preFabricator,
                                LevelCoroutines coroutine,
                                TransmogRecipeData recipeSaveData,
                                TransmogSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _recipeGen, _craftingModule, _preFabricator, _coroutine, _recipeSaveData, _settings, _getLogger) = 
                (baseUnityPlugin, recipeGen, craftingModule, preFabricator, coroutine, recipeSaveData, settings, getLogger);

            if (!SideLoaderEx.TryHookOnPacksLoaded(this, LoadRecipesFromSave))
                TmogRecipeManagerPatches.LoadCraftingRecipeAfter += (r) => LoadRecipesFromSave();
            TmogNetworkLevelLoaderPatches.MidLoadLevelAfter += (n) => _coroutine.InvokeAfterLevelAndPlayersLoaded(n, LearnCharacterRecipesFromSave, 300, 1);
            TmogCharacterEquipmentPatches.EquipItemBefore += (equipArgs) => CheckAddTmogRecipe(equipArgs.Character.Inventory, equipArgs.Equipment);
            TmogCharacterRecipeKnowledgePatches.LearnRecipeBefore += TryLearnTransmogRecipe;

            //try to learn recipes
            _settings.AllCharactersLearnRecipesEnabled += LearnCharacterRecipesFromSave;
        }

        private void TryLearnTransmogRecipe(CharacterRecipeKnowledge knowledge, TransmogRecipe recipe)
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
                    string loc = LocalizationManager.Instance.GetLoc("Notification_Item_RecipeLearnt", recipe.RecipeName);
                    character.CharacterUI.ShowInfoNotification(loc, ItemManager.Instance.RecipeLearntIcon, _itemLayout: true);
                }
                //Exclude acheivement tracking since these are so simple to come by
                //int num = knownUIDs.Count - StartingRecipeCount;
                //AchievementManager.Instance.SetStat(AchievementManager.AchievementStat.NewRecipeLearned, num);
                //if (!StoreManager.Instance.CanTrackAchievementProgress && num >= 50)
                //{
                //    AchievementManager.Instance.SetAchievementAsCompleted(AchievementManager.Achievement.Encyclopedic_30);
                //}
            }
        }

        #region Event Subscription Targets
        private void LoadRecipesFromSave()
        {

            AddRemoverRecipe();

            var saves = _recipeSaveData.GetAllRecipes();
            foreach (var r in saves)
            {
                try
                {
                    AddOrGetRecipe(r.Key, r.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"Could not load recipe {r.Key} - {r.Value} from save!", ex);
                }
            }
        }
        private void AddRemoverRecipe()
        {
            if (!ResourcesPrefabManager.Instance.ContainsItemPrefab(TransmogSettings.RemoveRecipe.ResultItemID.ToString()))
            {
                var removerResult = _preFabricator.CreatePrefab(TransmogSettings.RemoveRecipe.SourceResultItemID,
                                                                TransmogSettings.RemoveRecipe.ResultItemID,
                                                                TransmogSettings.RemoveRecipe.ResultItemName,
                                                                TransmogSettings.RemoveRecipe.ResultItemDesc)
                              .ConfigureItemIcon(TransmogSettings.RemoveRecipe.IconFile);
                Logger.LogInfo($"Added Transmog Remover Recipe placeholder prefab {removerResult.ItemID} - {removerResult.DisplayName}.");
            }

            //Add remover recipe
            if (!GetRecipeExists(TransmogSettings.RemoveRecipe.UID))
            {
                _craftingModule.RegisterRecipe<TransmogrifyMenu>(_recipeGen.GetRemoverRecipe());
            }
        }
        private void LearnCharacterRecipesFromSave()
        {
            var characters = SplitScreenManager.Instance.LocalPlayers.Select(p => p.AssignedCharacter);

            string logMessage = $"Learning Transmog Removal and Starter Recipes for {characters?.Count()??0} characters.";
            if (_settings.AllCharactersLearnRecipes)
                logMessage += " Setting [AllCharactersLearnRecipes] is enabled. Characters will learn any and all recipes discovered by other character saves.";
            Logger.LogInfo(logMessage);
            //learn remover recipe
            foreach (var c in characters)
            {
                TryLearnRecipe(c.Inventory, _recipeGen.GetRemoverRecipe());
            }

            if (_settings.AllCharactersLearnRecipes)
            {
                var saves = _recipeSaveData.GetAllRecipes();
                LearnRecipes(characters, saves);
            }

            var starterRecipes = TransmogSettings.StartingTransmogRecipes;
            LearnRecipes(characters, starterRecipes);
        }
        private void CheckAddTmogRecipe(CharacterInventory inventory, Equipment equipment)
        {
            if (equipment.IsTransmogIngredient())
                _baseUnityPlugin.StartCoroutine(
                _coroutine.InvokeAfter(() => true, () => AddLearnRecipe(inventory, equipment), 5, 0f)
                );
        }
        private void AddLearnRecipe(CharacterInventory inventory, Equipment equipment)
        {
            var recipe = AddOrGetRecipe(equipment);
            TryLearnRecipe(inventory, recipe);
        }
        private bool TryLearnRecipe(CharacterInventory inventory, Recipe recipe)
        {
            if (!inventory.RecipeKnowledge.IsRecipeLearned(recipe.UID))
            {
                inventory.RecipeKnowledge.LearnRecipe(recipe);
                Logger.LogInfo($"Character Learned new Transmogrify Recipe {recipe.Name}.");
                return true;
            }
            return false;
        }
        #endregion

        private static Dictionary<string, Recipe> _recipesRef;
        /// <summary>
        /// Checks if a transmog recipe for the provided ItemID.
        /// </summary>
        /// <param name="visualItemID"></param>
        /// <returns></returns>
        public bool GetRecipeExists(int visualItemID)
        {
            if (_recipesRef == null)
                _recipesRef = RecipeManager.Instance.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            if (!_recipesRef.Any())
                throw new InvalidOperationException("Cannot check if a recipe exists before the RecipeManager has loaded recipes.");

            return _recipesRef.Values.Any(r => r.RecipeID == _recipeGen.GetRecipeID(visualItemID));
        }
        public bool GetRecipeExists(string recipeUID)
        {
            if (_recipesRef == null)
                _recipesRef = RecipeManager.Instance.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            if (!_recipesRef.Any())
                throw new InvalidOperationException("Cannot check if a recipe exists before the RecipeManager has loaded recipes.");

            return _recipesRef.ContainsKey(recipeUID);
        }
        public bool TryGetTransmogRecipe(string UID, out TransmogRecipe recipe)
        {
            if (_recipesRef == null)
                _recipesRef = RecipeManager.Instance.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            if (!_recipesRef?.Any() ?? false)
                throw new InvalidOperationException("Cannot check if a recipe exists before the RecipeManager has loaded recipes.");

            if (_recipesRef.TryGetValue(UID, out var baseRecipe))
            {
                //intentional so exception thrown if not a TransmogRecipe
                recipe = (TransmogRecipe)baseRecipe;
                return true;
            }
            recipe = default;
            return false;
        }

        public bool TryGetTransmogRecipe(int visualItemID, out TransmogRecipe recipe)
        {
            if (_recipesRef == null)
                _recipesRef = RecipeManager.Instance.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            if (!_recipesRef?.Any() ?? false)
                throw new InvalidOperationException("Cannot check if a recipe exists before the RecipeManager has loaded recipes.");

            recipe = (TransmogRecipe)_recipesRef.Values.FirstOrDefault(r => r.RecipeID == _recipeGen.GetRecipeID(visualItemID));
            return recipe != default;
        }

        public TransmogRecipe AddOrGetRecipe(Equipment equipment)
        {
            if (TransmogSettings.ExcludedItemIDs.Contains(equipment.ItemID))
                throw new ArgumentException($"Visual for ItemID {equipment.ItemID} is excluded from Transmogs. It probably doesn't work correctly.", nameof(equipment));

            if (!TryGetTransmogRecipe(equipment.ItemID, out var recipe))
            {
                return AddRecipe(equipment, UID.Generate());
            }

            return recipe;
        }
        public TransmogRecipe AddOrGetRecipe(int visualItemID, string uid)
        {
            if (visualItemID == 0)
                throw new ArgumentException("0 is not a valid ItemID.", nameof(visualItemID));

            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentException("Must be a valid UID.", nameof(uid));

            if (TransmogSettings.ExcludedItemIDs.Contains(visualItemID))
                throw new ArgumentException($"Visual for ItemID {visualItemID} is excluded from Transmogs. It probably doesn't work correctly.", nameof(visualItemID));

            //Ensure valid UID.
            var recipeUid = UID.Decode(uid);

            if (!TryGetTransmogRecipe(recipeUid.ToString(), out var recipe))
            {
                var equipment = (ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString())) as Equipment;
                return AddRecipe(equipment, recipeUid);
            }

            return recipe;
        }
        public TransmogRecipe AddRecipe(Equipment equipment, UID recipeUID)
        {
            string equipType;
            TransmogRecipe recipe;
            if (equipment is Armor armor)
            {
                recipe = _recipeGen.GetTransmogArmorRecipe(armor);
                equipType = "Armor";
            }
            else if (equipment is Weapon weapon)
            {
                recipe = _recipeGen.GetTransmogWeaponRecipe(weapon);
                equipType = "Weapon";
            }
            else if (equipment is Bag bag)
            {
                recipe = _recipeGen.GetTransmogBagRecipe(bag);
                equipType = "Bag";
            }
            else if (equipment.HasTag(TransmogSettings.LanternTag))
            {
                recipe = _recipeGen.GetTransmogLanternRecipe(equipment);
                equipType = "Lantern";
            }
            else if (equipment.HasTag(TransmogSettings.LexiconTag))
            {
                recipe = _recipeGen.GetTransmogLexiconRecipe(equipment);
                equipType = "Lexicon";
            }
            else
                throw new ArgumentException($"Equipment '{equipment?.DisplayName}' for visualItemID {equipment.ItemID} is not an Armor, Weapon, Bag or Lexicon.", nameof(equipment.ItemID));

            recipe.SetUID(recipeUID);

            _recipeSaveData.SaveRecipe(recipe.VisualItemID, recipe.UID);
            _craftingModule.RegisterRecipe<TransmogrifyMenu>(recipe);
            Logger.LogInfo($"Registered new Transmogrify recipe for {equipment.ItemID} - {equipment.DisplayName}. Equipment was a type of {equipType}");
            return recipe;
        }
        
        private void LearnRecipes(IEnumerable<Character> characters, IEnumerable<KeyValuePair<int, UID>> recipes)
        {
            foreach (var r in recipes)
            {
                if (TryGetTransmogRecipe(r.Value, out var recipe))
                {
                    if (recipe.VisualItemID != r.Key)
                    {
                        Logger.LogError($"Recipe '{recipe.Name}' ({recipe.UID}) VisualItemID {recipe.VisualItemID} does not match ItemID {r.Value} of the saved recipe! This recipe will not be learned");
                        continue;
                    }
                    foreach (var c in characters)
                    {
                        TryLearnRecipe(c.Inventory, recipe);
                    }
                }
            }
        }
    }
}
