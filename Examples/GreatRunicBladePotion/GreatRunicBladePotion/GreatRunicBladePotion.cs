using BepInEx;
using BepInEx.Logging;
using SideLoader;
using System;

namespace ModifAmorphic.Outward.GreaterRunicBladePotion
{
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(GreatRunicBladePotion.ModId, GreatRunicBladePotion.ModName, GreatRunicBladePotion.ModVersion)]
    public class GreatRunicBladePotion : BaseUnityPlugin
    {
        public const string ModId = "ModifAmorphic.Outward.GreaterRunicBladePotion";
        public const string ModName = "Greater Runic Blade Potion";
        public const string ModVersion = "1.0.0";

        public static BaseUnityPlugin Instance;
        public static ManualLogSource ModLogger;

        internal void Awake()
        {
            Instance = this;
            ModLogger = Logger;
            SL.OnPacksLoaded += AddEffect;
        }

        private void AddEffect()
        {
            const int targetItemID = -13300001;
            var potionPrefab = ResourcesPrefabManager.Instance.GetItemPrefab(targetItemID);
            var potionEffects = potionPrefab.transform.Find("Effects");
            potionEffects.gameObject.AddComponent<GreatRunicBladeEffect>();
            //Added twice is intentional.
            potionEffects.gameObject.AddComponent<GreatRunicBladeEffect>();
            Logger.LogDebug($"Added {nameof(GreatRunicBladeEffect)} to item prefab {potionPrefab.name}.");
        }
    }
}
