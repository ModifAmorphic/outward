using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class SkillChainPrefabricator
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly InventoryServicesInjector _inventoryServicesInjector;
        private readonly ModifCoroutine _coroutines;

        public static SkillChainPrefabricator Instance;

        public SkillChainPrefabricator(InventoryServicesInjector inventoryServicesInjector, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            (_inventoryServicesInjector, _coroutines, _getLogger) = (inventoryServicesInjector, coroutines, getLogger);
            _inventoryServicesInjector.SkillChainsProfilesLoaded += TryAddSkillChains;
            Instance = this;
        }

        private void TryAddSkillChains(int playerID, string characterUID, SkillChainsJsonService skillChainsService)
        {
            try
            {
                AddSkillChains(skillChainsService.GetSkillChainProfile().SkillChains, characterUID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to add {nameof(SkillChain)}s for player {playerID}, character {characterUID}", ex);
            }
        }

        private void AddSkillChains(IEnumerable<SkillChain> skillChains, string characterUID)
        {
            Logger.LogDebug($"Adding {skillChains?.Count()} {nameof(SkillChain)} prefabs for character {characterUID}.");
            foreach (var skillChain in skillChains)
            {
                var skill = AddOrGetSkillPrefab(skillChain);
                //if (skill.ItemDisplay != null)
                //    AddEquipmentSetIcon(skill.ItemDisplay, skill);
            }
        }

        public ChainedSkill AddOrGetSkillPrefab(SkillChain skillChain)
        {
            if (skillChain.ItemID == -1 || skillChain.ItemID == 0)
                throw new ArgumentException($"{nameof(SkillChain.ItemID)} must not be zero or -1.", nameof(skillChain));

            if (ResourcesPrefabManager.Instance.ContainsItemPrefab(skillChain.ItemID.ToString()))
                return (ChainedSkill)ResourcesPrefabManager.Instance.GetItemPrefab(skillChain.ItemID.ToString());

            var skillGo = new GameObject(skillChain.ItemID + "_" + skillChain.Name.Replace(" ", string.Empty));

            skillGo.SetActive(false);
            var skill = skillGo.AddComponent<ChainedSkill>();
            skill.SetChain(skillChain.Name, skillChain.ItemID, skillChain.ActionChain.Values);
            //skill.Character = _character;
            skill.IsPrefab = true;
            //skillGo.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(skillGo);
            var itemPrefabs = InventoryService.GetItemPrefabs();
            itemPrefabs.Add(skill.ItemIDString, skill);
            //skillGo.SetActive(true);

            Logger.LogDebug($"AddOrGetSkillPrefab: Created skill prefab {skill.name}.");
            return skill;
        }

        //public void AddEquipmentSetIcon(ItemDisplay itemDisplay, Item item)
        //{
        //    var existingIcons = itemDisplay.GetComponentsInChildren<Image>().Where(i => i.name == "imgEquipmentSet").ToArray();

        //    if (!(item is EquipmentSetSkill setSkill))
        //    {
        //        for (int i = 0; i < existingIcons.Length; i++)
        //            UnityEngine.Object.Destroy(existingIcons[i].gameObject);
        //        return;
        //    }
        //    else if (existingIcons.Any())
        //        return;

        //    var enchantedGo = itemDisplay.transform.Find("imgEnchanted").gameObject;
        //    var equipmentSetIconGo = UnityEngine.Object.Instantiate(enchantedGo, itemDisplay.transform);
        //    var existingImage = equipmentSetIconGo.GetComponent<Image>();
        //    UnityEngine.Object.DestroyImmediate(existingImage);

        //    equipmentSetIconGo.name = "imgEquipmentSet";
        //    var newImage = equipmentSetIconGo.AddComponent<Image>();
        //    newImage.sprite = ActionMenuResources.Instance.SpriteResources["EquipmentSetIcon"];
        //    equipmentSetIconGo.SetActive(true);

        //    Logger.LogDebug($"Added EquipmentSetIcon {equipmentSetIconGo.name} to ItemDisplay {itemDisplay.gameObject.name}.");
        //}

        public void RemoveSkillChainPrefab(int itemID)
        {

            if (ResourcesPrefabManager.Instance.ContainsItemPrefab(itemID.ToString()))
            {
                var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(itemID);
                InventoryService.GetItemPrefabs().Remove(itemID.ToString());
                prefab.gameObject.Destroy();

                Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveSkillChainPrefab)}: Destroyed set prefab with SetID == {itemID}.");
            }
        }
    }
}
