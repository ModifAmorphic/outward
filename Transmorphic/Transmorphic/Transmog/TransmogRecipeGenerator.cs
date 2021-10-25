using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Settings;
using ModifAmorphic.Outward.Transmorphic.Transmog.Recipes;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    internal class TransmogRecipeGenerator
    {
        private readonly TransmogSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly IDynamicResultService _removerResultService;
        private readonly IDynamicResultService _armorResultService;
        private readonly IDynamicResultService _weaponResultService;
        private readonly IDynamicResultService _bagResultService;
        private readonly IDynamicResultService _lexiconResultService;
        private readonly IDynamicResultService _lanternResultService;

        public TransmogRecipeGenerator(
                                IDynamicResultService removerResultService,
                                IDynamicResultService armorResultService,
                                IDynamicResultService weaponResultService,
                                IDynamicResultService bagResultService,
                                IDynamicResultService lexiconResultService,
                                IDynamicResultService lanternResultService,
                                TransmogSettings settings, Func<IModifLogger> getLogger)
        {
            (_removerResultService, _armorResultService, _weaponResultService, _bagResultService, _lexiconResultService, _lanternResultService, _settings, _getLogger) =
                (removerResultService, armorResultService, weaponResultService, bagResultService, lexiconResultService, lanternResultService, settings, getLogger);

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
        public TransmogBagRecipe GetTransmogBagRecipe(Bag bagSource)
        {
            if (bagSource == null)
            {
                return null;
            }

            return ConfigureTransmogRecipe(bagSource, TransmogSettings.BackpackTag, ScriptableObject.CreateInstance<TransmogBagRecipe>(), _bagResultService);
        }
        public TransmogLanternRecipe GetTransmogLanternRecipe(Equipment equipSource)
        {
            if (equipSource == null)
            {
                return null;
            }

            return ConfigureTransmogRecipe(equipSource, TransmogSettings.LanternTag, ScriptableObject.CreateInstance<TransmogLanternRecipe>(), _lanternResultService);
        }
        public TransmogLexiconRecipe GetTransmogLexiconRecipe(Equipment equipSource)
        {
            if (equipSource == null)
            {
                return null;
            }

            return ConfigureTransmogRecipe(equipSource, TransmogSettings.LexiconTag, ScriptableObject.CreateInstance<TransmogLexiconRecipe>(), _lexiconResultService);
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
    }
}
