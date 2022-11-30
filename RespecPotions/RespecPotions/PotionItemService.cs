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
using System.Text.RegularExpressions;

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

                    if (prefabItems.Any(p => p.Name.Equals(potionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        potionName += $" [{schoolIndex}]";
                        potionDesc = RespecConstants.PotionDescFormat.Replace("{SchoolName}", potionName);
                        potionDesc += "\n" + RespecConstants.PotionDuplicateFormat.Replace("{SchoolName}", skillSchools[schoolIndex].Name);
                        Logger.LogDebug($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: Found duplicate potion with Name {skillSchools[schoolIndex].Name}. Renamed new potion to {potionName}.");
                    }
                    if (TryGetForgetPotion(skillSchools[schoolIndex], schoolIndex, potionName, potionDesc, basePrefab, iconDir, out var potionPrefab))
                    {
                        prefabItems.Add(potionPrefab);
                        Logger.LogTrace($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: Added effect {nameof(ForgetSchoolEffect)} with SchoolIndex of {schoolIndex} to prefab {potionPrefab.name}.");
                    }
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

        private bool TryGetForgetPotion(SkillSchool skillSchool, int schoolIndex, string potionName, string potionDesc, Item basePortionPrefab, string iconDir, out Item forgetPotion)
        {
            try
            {
                forgetPotion = _preFabricator.CreatePrefab(basePortionPrefab, RespecConstants.ItemStartID - schoolIndex, potionName, potionDesc, true);
                Logger.LogTrace($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: potionPrefab.gameObject.activeSelf={forgetPotion.gameObject.activeSelf}.");
                //potionPrefab.gameObject.SetActive(true);

                var iconFileName = GetIconFilePath(skillSchool.name, iconDir);

                Logger.LogDebug($"{nameof(PotionItemService)}::{nameof(AddForgetPotionPrefabs)}: Created '{forgetPotion.Name}' prefab with ItemID {forgetPotion.ItemID}.");
                forgetPotion.ClearEffects()
                    .ConfigureItemIcon(Path.Combine(iconDir, iconFileName))
                    .AddEffect<ForgetSchoolEffect>()
                    .SchoolIndex = schoolIndex;
                forgetPotion.AddEffect<AutoKnock>();
                //potionPrefab.AddEffect<BurnHealthEffect>()
                //    .AffectQuantity = .25f;
                forgetPotion.AddEffect<BurnStaminaEffect>()
                    .AffectQuantity = .25f;

                var itemStats = forgetPotion.gameObject.GetOrAddComponent<ItemStats>();
                itemStats.SetBaseValue(_settings.PotionValue.Value);
                
                return true;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(skillSchool?.name))
                    Logger.LogException($"Failed to create forgot potion for school '{skillSchool.name}'", ex);
                else
                    Logger.LogException($"Failed to create forgot potion for school index {schoolIndex}", ex);
            }

            forgetPotion = null;
            return false;
        }

        private bool TryAddForgetSchoolEffect(Item item, int schoolIndex)
        {
            try
            {
                item
                    .AddEffect<ForgetSchoolEffect>()
                    .SchoolIndex = schoolIndex;
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogException($"Failed to add {nameof(ForgetSchoolEffect)} to item {item?.name} for school index {schoolIndex}.", ex);
            }

            return false;
        }

        private string GetIconFilePath(string schoolname, string iconDirectory)
        {
            if (RespecConstants.CustomSchoolIcons.TryGetValue(schoolname, out var iFile) && File.Exists(Path.Combine(iconDirectory, iFile)))
                return Path.Combine(iconDirectory, iFile);

            try
            {
                var cleanFile = RemoveInvalidFilePathCharacters(schoolname + ".png", "_");
                var filePath = Path.Combine(iconDirectory, cleanFile);
                Logger.LogDebug($"Original Filename: '{schoolname + ".png"}'. Cleaned Filename: '{cleanFile}'.");
                if (File.Exists(filePath))
                    return filePath;
            }
            catch
            {
                Logger.LogWarning($"Encountered error trying to find school icon file for school {schoolname}. Return default file icon.");
            }
            
            return RespecConstants.CustomSchoolIcons["Default"];
        }
        public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            var replaced1 = r.Replace(filename, replaceChar);
            string replaced2 = string.Empty;
            foreach(char c in replaced1)
            {
                if ((int)c.GetTypeCode() > 126)
                    replaced2 += "_";
                else
                    replaced2 += c;
            }
            return replaced2;
        }
    }
}
