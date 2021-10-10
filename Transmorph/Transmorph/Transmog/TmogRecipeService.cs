using BepInEx;
using ModifAmorphic.Outward.Coroutines;
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
        private readonly TransmorphConfigSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;
        private readonly LevelCoroutines _coroutine;
        private readonly IDynamicResultService _armorResultService;
        private readonly IDynamicResultService _weaponResultService;
        private readonly CustomCraftingModule _craftingModule;
        private readonly TransmogRecipeData _recipeSaveData;

        public TmogRecipeService(BaseUnityPlugin baseUnityPlugin, IDynamicResultService armorResultService, IDynamicResultService weaponResultService,
                                CustomCraftingModule craftingModule, LevelCoroutines coroutine,
                                TransmogRecipeData recipeSaveData,
                                TransmorphConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _armorResultService, _weaponResultService, _craftingModule, _coroutine, _recipeSaveData, _settings, _getLogger) = 
                (baseUnityPlugin, armorResultService, weaponResultService, craftingModule, coroutine, recipeSaveData, settings, getLogger);

            if (!TryHookSideLoaderOnPacksLoaded(LoadRecipesFromSave))
                TmogRecipeManagerPatches.LoadCraftingRecipeAfter += (r) => LoadRecipesFromSave();
            TmogNetworkLevelLoaderPatches.MidLoadLevelAfter += (n) => _coroutine.InvokeAfterLevelAndPlayersLoaded(n, LearnCharacterRecipesFromSave, 300, 1);
            TmogCharacterEquipmentPatches.EquipItemBefore += (equipArgs) => CheckAddTmogRecipe(equipArgs.Character.Inventory, equipArgs.Equipment);
            TmogCharacterRecipeKnowledgePatches.LearnRecipeBefore += TryLearnTransmogRecipe;
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
        private void LearnCharacterRecipesFromSave()
        {
            var characters = SplitScreenManager.Instance.LocalPlayers.Select(p => p.AssignedCharacter);
            if (!_settings.AllCharactersLearnRecipes.Value)
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

        public T ConfigureTransmogRecipe<T>(Item transmogSource, Tag transmogTag, T recipe, IDynamicResultService resultService) where T : TransmogRecipe
        {
            recipe
                .SetRecipeIDEx(GetRecipeID(transmogSource.ItemID))
                .SetUID(UID.Generate())
                .SetVisualItemID(transmogSource.ItemID)
                .SetNames("Transmogrify - " + transmogSource.DisplayName)
                .AddIngredient(new TagSourceSelector(transmogTag))
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab(TransmogSettings.RecipeSecondaryItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(TransmogSettings.RecipeSecondaryItemID.ToString()))
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
        private bool TryHookSideLoaderOnPacksLoaded(Action subscriber)
        {
            try
            {
                var slAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Equals("SideLoader", StringComparison.InvariantCultureIgnoreCase));
                if (slAssembly == default)
                    return false;
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Assembly {slAssembly}");

                var slType = slAssembly.GetTypes()
                    .FirstOrDefault(t => t.FullName.Equals("SideLoader.SL", StringComparison.InvariantCultureIgnoreCase));
                if (slType == default)
                    return false;
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Type {slType}");

                var onPacksLoaded = slType.GetEvent("OnPacksLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Event {onPacksLoaded}");
                var eventDelegateType = onPacksLoaded.EventHandlerType;
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got EventHandlerType {eventDelegateType}");

                var mcall = ((Expression<Action>)(() => LoadRecipesFromSave())).Body as MethodCallExpression;


                var slDelegate = Delegate.CreateDelegate(eventDelegateType, this, subscriber.Method);
                Logger.LogDebug($"TryHookSideLoaderOnPacksLoaded: Got Delegate {slDelegate}");
                onPacksLoaded
                    .GetAddMethod()
                    .Invoke(null, new object[] { slDelegate });

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException("Error trying to retrieve SideLoader OnPacksLoaded event.", ex);
            }
            return false;

            //foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    if (asm.GetName().Name.Contains("SideLoader") || asm.FullName.Contains("SideLoader"))
            //    {
            //        logger.LogDebug($"{asm.FullName} | {asm.GetName().Name}: types");
            //        foreach (var type in asm.GetTypes())
            //        {
            //            //if (!string.IsNullOrEmpty(nameFilter) && !type.FullName.ContainsIgnoreCase(nameFilter))
            //            //    continue;
            //            logger.LogDebug($"{asm.GetName().Name}: {type.FullName} | {type.Name}");
            //        }
            //    }
            //}
        }
    }
}
