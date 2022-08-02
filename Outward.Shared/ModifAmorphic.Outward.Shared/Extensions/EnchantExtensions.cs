using UnityEngine;

namespace ModifAmorphic.Outward.Extensions
{
    public static class EnchantExtensions
    {
        public static bool GetHasMatchingEquipment(this EnchantmentRecipe recipe, Equipment equipment) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingEquipment", equipment);
        public static bool GetHasMatchingTime(this EnchantmentRecipe recipe) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingTime");
        public static bool GetHasMatchingWindAltarState(this EnchantmentRecipe recipe) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingWindAltarState");
        public static bool GetHasMatchingRegion(this EnchantmentRecipe recipe) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingRegion");
        public static bool GetHasMatchingTemperature(this EnchantmentRecipe recipe, EnchantmentTable table) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingTemperature", table);
        public static bool GetHasMatchingTemperature(this EnchantmentRecipe recipe, Transform transform)
        {
            if (recipe.Temperature != null && recipe.Temperature.Length != 0)
            {
                if ((bool)EnvironmentConditions.Instance)
                {
                    int temperature = EnvironmentConditions.Instance.GetTemperature(transform);
                    return recipe.Temperature.Contains((TemperatureSteps)temperature);
                }
                return false;
            }
            return true;
        }
        public static bool GetHasMatchingWeather(this EnchantmentRecipe recipe) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingWeather");
        public static bool GetHasMatchingQuestEvent(this EnchantmentRecipe recipe) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingQuestEvent");
        public static bool GetHasMatchingTablePlacement(this EnchantmentRecipe recipe, EnchantmentTable table) => recipe.InvokePrivateMethod<EnchantmentRecipe, bool>("GetHasMatchingTablePlacement", table);
    }
}
