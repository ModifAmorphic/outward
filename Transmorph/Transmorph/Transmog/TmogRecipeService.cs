using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorph.Extensions;
using ModifAmorphic.Outward.Transmorph.Menu;
using ModifAmorphic.Outward.Transmorph.Settings;
using ModifAmorphic.Outward.Transmorph.Transmog.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ModifCoroutine _coroutine;
        private readonly IDynamicResultService _armorResultService;
        private readonly IDynamicResultService _weaponResultService;
        private readonly CustomCraftingModule _craftingModule;

        public TmogRecipeService(BaseUnityPlugin baseUnityPlugin, IDynamicResultService armorResultService, IDynamicResultService weaponResultService,
                                CustomCraftingModule craftingModule, ModifCoroutine coroutine,
                                TransmorphConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _armorResultService, _weaponResultService, _craftingModule, _coroutine, _settings, _getLogger) = 
                (baseUnityPlugin, armorResultService, weaponResultService, craftingModule, coroutine, settings, getLogger);

            TmogCharacterEquipmentPatches.EquipItemBefore += (equipArgs) => CheckAddTmogRecipe(equipArgs.Character.Inventory, equipArgs.Equipment);
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
            if (!TryGetTransmogRecipe(equipment.ItemID, out var recipe))
            {
                if (equipment is Armor)
                    recipe = GetTransmogArmorRecipe(equipment.ItemID);
                else if (equipment is Weapon)
                    recipe = GetTransmogWeaponRecipe(equipment.ItemID);
                else
                    throw new ArgumentException($"Equipment Item {equipment?.ItemID} - {equipment?.DisplayName} is not an Armor or Weapon type.", nameof(equipment));

                _craftingModule.RegisterRecipe<TransmogrifyMenu>(recipe);
                Logger.LogInfo($"Registered new Transmogrify recipe for {equipment.ItemID} - {equipment.DisplayName}. Equipment was a type of " +
                    $"{((equipment is Armor)? "Armor" : (equipment is Weapon) ? "Weapon" : "Unknown")}");
            }

            if (!inventory.RecipeKnowledge.IsRecipeLearned(recipe.UID))
            {
                inventory.RecipeKnowledge.LearnRecipe(recipe);
                Logger.LogInfo($"Character Learned new Transmogrify Recipe {recipe.Name}.");
            }
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
        public bool TryGetTransmogRecipe(int visualItemID, out TransmogRecipe recipe)
        {
            if (_recipesRef == null)
                _recipesRef = RecipeManager.Instance.GetPrivateField<RecipeManager, Dictionary<string, Recipe>>("m_recipes");
            if (!_recipesRef?.Any()??false)
                throw new InvalidOperationException("Cannot check if a recipe exists before the RecipeManager has loaded recipes.");

            recipe = _recipesRef.Values.FirstOrDefault(r => r.RecipeID == GetRecipeID(visualItemID)) as TransmogRecipe;

            return recipe != default;
        }
        public TransmogArmorRecipe GetTransmogArmorRecipe(int visualItemID)
        {
            var transmogSource = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString());
            if (transmogSource == null || !(transmogSource is Armor armorSource))
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
            var weaponTag = weaponSource.Type.ToWeaponTag();
            TagSourceManager.Instance.TryAddTag(weaponTag, true);

            return ConfigureTransmogRecipe(weaponSource, weaponTag, ScriptableObject.CreateInstance<TransmogWeaponRecipe>(), _weaponResultService)
                .SetWeaponType(weaponSource.Type);
        }
        private TransmogRecipe GetTransmogRecipe(int visualItemID)
        {
            var transmogSource = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString());

            var tagSource = new TagSourceSelector(transmogSource.Tags[0]);
            

            var recipe = ScriptableObject.CreateInstance<TransmogArmorRecipe>()
                .SetRecipeIDEx(GetRecipeID(visualItemID))
                .SetUID(UID.Generate());
            
            return recipe
                .SetNames("Transmog - " + transmogSource.DisplayName)
                .AddIngredient(new TagSourceSelector(transmogSource.Tags[0]))
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab(TransmogSettings.RecipeSecondaryItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(TransmogSettings.RecipeSecondaryItemID.ToString()))
                .SetVisualItemID(visualItemID)
                .AddResult(visualItemID);
        }

        public T ConfigureTransmogRecipe<T>(Item transmogSource, Tag transmogTag, T recipe, IDynamicResultService resultService) where T : TransmogRecipe
        {
            recipe
                .SetRecipeIDEx(GetRecipeID(transmogSource.ItemID))
                .SetUID(UID.Generate())
                .SetVisualItemID(transmogSource.ItemID)
                .SetNames("Transmog - " + transmogSource.DisplayName)
                .AddIngredient(new TagSourceSelector(transmogTag))
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab(TransmogSettings.RecipeSecondaryItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(TransmogSettings.RecipeSecondaryItemID.ToString()))
                .AddDynamicResult(resultService, transmogSource.ItemID, 1);

            return recipe;
        }

        public int GetRecipeID(int itemID)
        {
            return TransmogSettings.RecipeStartID - itemID;
        }
        public TransmogRecipe AddOrGetRecipe(int visualItemID)
        {
            var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID);
            if (!(prefab is Armor || prefab is Weapon))
            {
                //Logger.LogWarning($"{nameof(TmogRecipeService)}::{nameof(TryAddRecipe)}(): Tried to add a recipe for non Armor or Weapon type ItemID {visualItemID} - {prefab?.DisplayName}.");
                throw new ArgumentException($"ItemID {visualItemID} - '{prefab?.DisplayName}' must be an Armor or Weapon type.", nameof(visualItemID));
            }

            if (!TryGetTransmogRecipe(visualItemID, out var tmogRecipe))
            {
                tmogRecipe = (prefab is Armor) ?
                    (TransmogRecipe)GetTransmogArmorRecipe(visualItemID) : GetTransmogWeaponRecipe(visualItemID);

                _craftingModule.RegisterRecipe<TransmogrifyMenu>(tmogRecipe);

                Logger.LogInfo($"Registered new Transmogrify recipe for {visualItemID} - {prefab.DisplayName}.");
            }
            return tmogRecipe;
        }
    }
}
