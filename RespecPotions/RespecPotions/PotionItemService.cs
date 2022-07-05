using BepInEx;
using Localizer;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Models;
using ModifAmorphic.Outward.Modules.CharacterMods;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Modules.Merchants;
using ModifAmorphic.Outward.RespecPotions.Effects;
using ModifAmorphic.Outward.RespecPotions.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModifAmorphic.Outward.RespecPotions
{
    internal class PotionItemService
    {
        //private readonly ServicesProvider _services;
        private readonly CharacterInstances _characterInstances;
        private readonly ResourcesPrefabManager _resourcesPrefabManager;
        private readonly PreFabricator _preFabricator;
        private readonly MerchantModule _merchantModule;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly RespecConfigSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public event Action<IEnumerable<Item>> PotionPrefabsAdded;

        private bool coroutineStarted = false;
        private bool prefabsLoaded = false;

        public PotionItemService(CharacterInstances characterInstances,
                                 ResourcesPrefabManager resourcesPrefabManager,
                                 PreFabricator preFabricator,
                                 MerchantModule merchantModule,
                                 BaseUnityPlugin baseUnityPlugin,
                                 RespecConfigSettings settings,
                                 Func<IModifLogger> getLogger)
        {
            (_characterInstances, _resourcesPrefabManager, _preFabricator, _merchantModule, _baseUnityPlugin, _settings, _getLogger) = (characterInstances, resourcesPrefabManager, preFabricator, merchantModule, baseUnityPlugin, settings, getLogger);
            PotionPrefabsAdded += (prefabs) =>
            {
                prefabsLoaded = true;
                coroutineStarted = false;
                AddPotionsToShops(prefabs);
            };
        }
        private void AddPotionsToShops(IEnumerable<Item> forgetPotions)
        {
            Logger.LogDebug($"{nameof(PotionItemService)}::{nameof(AddPotionsToShops)}: Adding '{forgetPotions.Count()}' forget potions to shop inventories.");

            var forgetPotionDrops = forgetPotions.Select(p => new CustomGuaranteedDrop()
            {
                ItemID = p.ItemID,
                MinAmount = _settings.ShopMinAmount.Value,
                MaxAmount = _settings.ShopMaxAmount.Value
            });
            foreach (var mpath in RespecConstants.VendorPaths)
            {
                _merchantModule.AddGuaranteedDrops(mpath.SceneName, mpath.VendorPath,
                    forgetPotionDrops,
                    "ForgetPotions");
            }
        }
        public void StartCreatePotionItemsCoroutine()
        {
            if (coroutineStarted || prefabsLoaded)
                return;
            
            coroutineStarted = true;

            const int timeoutSecs = 800;
            Func<bool> isPrefabAndSkillsLoaded = () =>
                    _characterInstances.TryGetSkillSchools(out _) 
                            //&& _services.TryGetService<ResourcesPrefabManager>(out var prefabManager) 
                            && _resourcesPrefabManager.Loaded;
            Action addForgetPotions = () => AddForgetPotionPrefabs(_characterInstances);
            var coroutine = new ModifCoroutine(_baseUnityPlugin, _getLogger);
            Func<IEnumerator> loadPotionsAfter = () => coroutine.InvokeAfter(isPrefabAndSkillsLoaded, addForgetPotions, timeoutSecs, .5f);

            _baseUnityPlugin.StartCoroutine(loadPotionsAfter.Invoke());
        }
        private void AddForgetPotionPrefabs(CharacterInstances characterInstances)
        {
            var iconDir = Path.Combine(
                                        Path.GetDirectoryName(_baseUnityPlugin.Info.Location),
                                        RespecConstants.IconPath);

            try
            {
                characterInstances.TryGetSkillSchools(out var skillSchools);

                var prefabItems = new List<Item>();

                var basePrefab = _resourcesPrefabManager.GetItemPrefab(4300220);

                foreach (int schoolIndex in skillSchools.Keys)
                {
                    var potionName = RespecConstants.PotionNameFormat.Replace("{SchoolName}", skillSchools[schoolIndex].Name);
                    var potionDesc = RespecConstants.PotionDescFormat.Replace("{SchoolName}", skillSchools[schoolIndex].Name);

                    var potionPrefab = _preFabricator.CreatePrefab(basePrefab, RespecConstants.ItemStartID - schoolIndex, potionName, potionDesc);
                    Logger.LogTrace($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: potionPrefab.gameObject.activeSelf={potionPrefab.gameObject.activeSelf}.");
                    //potionPrefab.gameObject.SetActive(true);

                    var iconFileName = GetIconFilePath(skillSchools[schoolIndex].name, iconDir);

                    Logger.LogDebug($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: Created '{potionPrefab.Name}' prefab with ItemID {potionPrefab.ItemID}.");
                    potionPrefab.ClearEffects()
                        .ConfigureItemIcon(Path.Combine(iconDir, iconFileName))
                        .AddEffect<ForgetSchoolEffect>()
                        .SchoolIndex = schoolIndex;
                    potionPrefab.AddEffect<AutoKnock>();
                    //potionPrefab.AddEffect<BurnHealthEffect>()
                    //    .AffectQuantity = .25f;
                    potionPrefab.AddEffect<BurnStaminaEffect>()
                        .AffectQuantity = .25f;

                    var itemStats = potionPrefab.gameObject.GetOrAddComponent<ItemStats>();
                    itemStats.SetBaseValue(_settings.PotionValue.Value);

                    Logger.LogTrace($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: Added effect {nameof(ForgetSchoolEffect)} with SchoolIndex of {schoolIndex} to prefab {potionPrefab.ItemID}.");

                    prefabItems.Add(potionPrefab);
                }
                Logger.LogInfo($"Created Respec Potions for {skillSchools.Count} schools.");
                PotionPrefabsAdded?.Invoke(prefabItems);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: Exception adding Potion Prefabs.", ex);
                coroutineStarted = false;
            }
        }
        private string GetIconFilePath(string schoolname, string iconDirectory)
        {
            if (RespecConstants.CustomSchoolIcons.TryGetValue(schoolname, out var iFile) && File.Exists(Path.Combine(iconDirectory, iFile)))
                return Path.Combine(iconDirectory, iFile);

            string otherIcon = Path.Combine(iconDirectory, schoolname + ".png");
            
            return File.Exists(otherIcon) ? otherIcon : RespecConstants.CustomSchoolIcons["Default"];
        }
    }
}
