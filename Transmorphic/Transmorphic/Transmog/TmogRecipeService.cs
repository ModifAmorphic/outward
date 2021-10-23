using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Extensions;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Patches;
using ModifAmorphic.Outward.Transmorph.Settings;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using ModifAmorphic.Outward.Transmorph.Transmog.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Transmorph.Transmog
{
    internal class TmogRecipeService
    {
        private readonly TransmogSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;
        private readonly LevelCoroutines _coroutine;
        private readonly IDynamicResultService _removerResultService;
        private readonly IDynamicResultService _armorResultService;
        private readonly IDynamicResultService _weaponResultService;
        private readonly CustomCraftingModule _craftingModule;
        private readonly TransmogRecipeData _recipeSaveData;
        private readonly PreFabricator _preFabricator;

        public TmogRecipeService(BaseUnityPlugin baseUnityPlugin,
                                IDynamicResultService removerResultService,
                                IDynamicResultService armorResultService,
                                IDynamicResultService weaponResultService,
                                CustomCraftingModule craftingModule,
                                PreFabricator preFabricator,
                                LevelCoroutines coroutine,
                                TransmogRecipeData recipeSaveData,
                                TransmogSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _removerResultService, _armorResultService, _weaponResultService, _craftingModule, _preFabricator, _coroutine, _recipeSaveData, _settings, _getLogger) = 
                (baseUnityPlugin, removerResultService, armorResultService, weaponResultService, craftingModule, preFabricator, coroutine, recipeSaveData, settings, getLogger);

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
                _craftingModule.RegisterRecipe<TransmogrifyMenu>(GetRemoverRecipe());
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
                TryLearnRecipe(c.Inventory, GetRemoverRecipe());
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
            if (equipment is Armor || equipment is Weapon)
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
        public TransmogRecipe AddOrGetRecipe(Equipment equipment)
        {
            if (!TryGetTransmogRecipe(equipment.ItemID, out var recipe))
            {
                if (equipment is Armor armor)
                    recipe = GetTransmogArmorRecipe(armor);
                else if (equipment is Weapon weapon)
                    recipe = GetTransmogWeaponRecipe(weapon);
                else
                    throw new ArgumentException($"Equipment Item {equipment?.ItemID} - {equipment?.DisplayName} is not an Armor or Weapon type.", nameof(equipment));

                _recipeSaveData.SaveRecipe(recipe.VisualItemID, recipe.UID);
                _craftingModule.RegisterRecipe<TransmogrifyMenu>(recipe);
                Logger.LogInfo($"Registered new Transmogrify recipe for {equipment.ItemID} - {equipment.DisplayName}. Equipment was a type of " +
                    $"{((equipment is Armor) ? "Armor" : (equipment is Weapon) ? "Weapon" : "Unknown")}");
            }

            return recipe;
        }
        public TransmogRecipe AddOrGetRecipe(int visualItemID, string uid)
        {
            if (visualItemID == 0)
                throw new ArgumentException("0 is not a valid ItemID.", nameof(visualItemID));

            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentException("Must be a valid UID.", nameof(uid));

            //Ensure valid UID.
            var recipeUid = UID.Decode(uid);

            if (!TryGetTransmogRecipe(recipeUid.ToString(), out var recipe))
            {
                var equipment = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString()) as Equipment;
                if (equipment is Armor armor)
                    recipe = GetTransmogArmorRecipe(armor);
                else if (equipment is Weapon weapon)
                    recipe = GetTransmogWeaponRecipe(weapon);
                else
                    throw new ArgumentException($"Equipment '{equipment?.DisplayName}' for visualItemID {visualItemID} is not an Armor or Weapon type.", nameof(visualItemID));

                recipe.SetUID(recipeUid);

                _recipeSaveData.SaveRecipe(recipe.VisualItemID, recipe.UID);
                _craftingModule.RegisterRecipe<TransmogrifyMenu>(recipe);
                Logger.LogInfo($"Registered new Transmogrify recipe for {equipment.ItemID} - {equipment.DisplayName}. Equipment was a type of " +
                    $"{((equipment is Armor) ? "Armor" : (equipment is Weapon) ? "Weapon" : "Unknown")}");
            }

            return recipe;
        }

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

            return _recipesRef.Values.Any(r => r.RecipeID == GetRecipeID(visualItemID));
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

            recipe = (TransmogRecipe)_recipesRef.Values.FirstOrDefault(r => r.RecipeID == GetRecipeID(visualItemID));
            return recipe != default;
        }
        public TransmogArmorRecipe GetTransmogArmorRecipe(int visualItemID)
        {
            var transmogSource = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString());
            if (transmogSource == null || !(transmogSource is Armor armorSource))
            {
                return null;
            }
            return GetTransmogArmorRecipe(armorSource);
        }
        public TransmogArmorRecipe GetTransmogArmorRecipe(Armor armorSource)
        {
            if (armorSource == null)
            {
                return null;
            }

            var armorTag = armorSource.EquipSlot.ToArmorTag();
            TagSourceManager.Instance.TryAddTag(armorTag, true);
            return ConfigureTransmogRecipe(armorSource, armorTag, ScriptableObject.CreateInstance<TransmogArmorRecipe>(), _armorResultService)
                .SetEquipmentSlot(armorSource.EquipSlot);
        }
        public TransmogWeaponRecipe GetTransmogWeaponRecipe(int visualItemID)
        {
            var transmogSource = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString());
            if (transmogSource == null || !(transmogSource is Weapon weaponSource))
            {
                return null;
            }
            return GetTransmogWeaponRecipe(weaponSource);
        }
        public TransmogWeaponRecipe GetTransmogWeaponRecipe(Weapon weaponSource)
        {
            if (weaponSource == null)
            {
                return null;
            }
            var weaponTag = weaponSource.Type.ToWeaponTag();
            TagSourceManager.Instance.TryAddTag(weaponTag, true);

            return ConfigureTransmogRecipe(weaponSource, weaponTag, ScriptableObject.CreateInstance<TransmogWeaponRecipe>(), _weaponResultService)
                .SetWeaponType(weaponSource.Type);
        }
        public TransmogRemoverRecipe GetRemoverRecipe()
        {
            var secondaryIngredient = ResourcesPrefabManager.Instance.GetItemPrefab(TransmogSettings.RemoveRecipe.SecondIngredientID) ?? 
                ResourcesPrefabManager.Instance.GenerateItem(TransmogSettings.RemoveRecipe.SecondIngredientID.ToString());
            TagSourceManager.Instance.TryAddTag(TransmogSettings.RemoveRecipe.TransmogTag, true);
            var recipe = ScriptableObject.CreateInstance<TransmogRemoverRecipe>();
            recipe.SetRecipeIDEx(TransmogSettings.RemoveRecipe.RecipeID)
                .SetUID(TransmogSettings.RemoveRecipe.UID)
                .SetNames(TransmogSettings.RemoveRecipe.RecipeName)
                .AddIngredient(new TagSourceSelector(TransmogSettings.RemoveRecipe.TransmogTag))
                .AddIngredient(secondaryIngredient)
                .AddDynamicResult(_removerResultService, -1303000001);

            return recipe;
        }
        public T ConfigureTransmogRecipe<T>(Item transmogSource, Tag transmogTag, T recipe, IDynamicResultService resultService) where T : TransmogRecipe
        {
            recipe
                .SetRecipeIDEx(GetRecipeID(transmogSource.ItemID))
                .SetUID(UID.Generate())
                .SetVisualItemID(transmogSource.ItemID)
                .SetNames("Transmogrify - " + transmogSource.DisplayName)
                .AddIngredient(new TagSourceSelector(transmogTag))
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab(TransmogSettings.TransmogRecipeSecondaryItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(TransmogSettings.TransmogRecipeSecondaryItemID.ToString()))
                .AddDynamicResult(resultService, transmogSource.ItemID, 1);

            return recipe;
        }

        public int GetRecipeID(int itemID)
        {
            return TransmogSettings.RecipeStartID - itemID;
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
