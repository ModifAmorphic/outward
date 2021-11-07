using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Enchanting.Recipes
{
    internal class RecipeVisibleService : IRecipeVisibiltyController
    {
        private readonly EnchantingSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public RecipeVisibleService(EnchantingSettings settings, Func<IModifLogger> getLogger) => (_settings, _getLogger) = (settings, getLogger);

        public bool GetRecipeIsVisible(CustomCraftingMenu menu, Recipe recipe)
        {
            if (!_settings.ConditionalEnchantingEnabled)
                return true;

            if (!(recipe is EnchantRecipe enchantRecipe) || enchantRecipe.BaseEnchantmentRecipe == null)
                return false;

            var r = enchantRecipe.BaseEnchantmentRecipe;

            if (_settings.EnchantingConditionQuest && !r.GetHasMatchingQuestEvent())
                return false;

            if (_settings.EnchantingConditionRegion && !r.GetHasMatchingRegion())
                return false;

            if (_settings.EnchantingConditionTemperature && menu.LocalCharacter != null && !r.GetHasMatchingTemperature(menu.LocalCharacter.transform))
                return false;

            if (_settings.EnchantingConditionTime && !r.GetHasMatchingTime())
                return false;

            if (_settings.EnchantingConditionWeather && !r.GetHasMatchingWeather())
                return false;

            if (_settings.EnchantingConditionWindAltarState && !r.GetHasMatchingWindAltarState())
                return false;

            return true;
        }
    }
}
