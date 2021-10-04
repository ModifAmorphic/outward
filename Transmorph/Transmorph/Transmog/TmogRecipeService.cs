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
        private readonly IDynamicResultService _armorResultService;
        private readonly IDynamicResultService _weaponResultService;

        public TmogRecipeService(BaseUnityPlugin baseUnityPlugin, IDynamicResultService armorResultService, IDynamicResultService weaponResultService, TransmorphConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _armorResultService, _weaponResultService, _settings, _getLogger) = (baseUnityPlugin, armorResultService, weaponResultService, settings, getLogger);
            //TransmogCraftingMenuPatches.GenerateResultOverride += GenerateResultOverride;
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

        public TransmogArmorRecipe GetTransmogArmorRecipe(int visualItemID)
        {
            var transmogSource = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString());
            if (transmogSource == null || !(transmogSource is Armor armorSource))
            {
                return null;
            }

            return ConfigureTransmogRecipe(armorSource, ScriptableObject.CreateInstance<TransmogArmorRecipe>(), _armorResultService)
                .SetEquipmentSlot(armorSource.EquipSlot);
        }
        public TransmogWeaponRecipe GetTransmogWeaponRecipe(int visualItemID)
        {
            var transmogSource = ResourcesPrefabManager.Instance.GetItemPrefab(visualItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(visualItemID.ToString());
            if (transmogSource == null || !(transmogSource is Weapon weaponSource))
            {
                return null;
            }

            return ConfigureTransmogRecipe(weaponSource, ScriptableObject.CreateInstance<TransmogWeaponRecipe>(), _weaponResultService)
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
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab(TransmorphConstants.TransmogSecondaryItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(TransmorphConstants.TransmogSecondaryItemID.ToString()))
                .SetVisualItemID(visualItemID)
                .AddResult(visualItemID);
        }

        public T ConfigureTransmogRecipe<T>(Item transmogSource, T recipe, IDynamicResultService resultService) where T : TransmogRecipe
        {
            recipe
                .SetRecipeIDEx(GetRecipeID(transmogSource.ItemID))
                .SetUID(UID.Generate())
                .SetVisualItemID(transmogSource.ItemID)
                .SetNames("Transmog - " + transmogSource.DisplayName)
                .AddIngredient(new TagSourceSelector(transmogSource.Tags[0]))
                .AddIngredient(ResourcesPrefabManager.Instance.GetItemPrefab(TransmorphConstants.TransmogSecondaryItemID) ?? ResourcesPrefabManager.Instance.GenerateItem(TransmorphConstants.TransmogSecondaryItemID.ToString()))
                .AddDynamicResult(resultService, transmogSource.ItemID, 1);

            return recipe;
        }

        public int GetRecipeID(int itemID)
        {
            return TransmorphConstants.TransmogRecipeStartID - itemID;
        }
  

    }
}
