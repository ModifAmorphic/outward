using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.ActionUI.Services.Injectors;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    public class EquipSetPrefabService
    {
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly InventoryServicesInjector _inventoryServicesInjector;
        private readonly ModifCoroutine _coroutines;

        public EquipSetPrefabService(InventoryServicesInjector inventoryServicesInjector, ModifCoroutine coroutines, Func<IModifLogger> getLogger)
        {
            (_inventoryServicesInjector, _coroutines, _getLogger) = (inventoryServicesInjector, coroutines, getLogger);
            _inventoryServicesInjector.EquipmentSetProfilesLoaded += TryAddEquipmentSets;
        }

        private void TryAddEquipmentSets(int playerID, string characterUID, ArmorSetsJsonService armorService, WeaponSetsJsonService weaponService)
        {
            try
            {
                AddEquipmentSets<ArmorSetSkill>(armorService.GetEquipmentSetsProfile().EquipmentSets, characterUID);
                AddEquipmentSets<WeaponSetSkill>(weaponService.GetEquipmentSetsProfile().EquipmentSets, characterUID);
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to add equipmentSets for player {playerID}, character {characterUID}", ex);
            }
        }

        private void AddEquipmentSets<T>(IEnumerable<IEquipmentSet> sets, string characterUID) where T : EquipmentSetSkill
        {
            Logger.LogDebug($"Adding {sets?.Count()} {typeof(T).Name} prefabs for character {characterUID}.");
            foreach (IEquipmentSet set in sets)
            {
                var skill = AddOrGetEquipmentSetSkillPrefab<T>(set);
                if (skill.ItemDisplay != null)
                    AddEquipmentSetIcon(skill.ItemDisplay, skill);
            }
        }

        public T AddOrGetEquipmentSetSkillPrefab<T>(IEquipmentSet equipmentSet) where T : EquipmentSetSkill
        {
            if (equipmentSet.SetID == 0)
                throw new ArgumentException($"{nameof(IEquipmentSet.SetID)} must not be zero.", nameof(equipmentSet));

            if (ResourcesPrefabManager.Instance.ContainsItemPrefab(equipmentSet.SetID.ToString()))
                return (T)ResourcesPrefabManager.Instance.GetItemPrefab(equipmentSet.SetID.ToString());

            var skillGo = new GameObject(equipmentSet.Name.Replace(" ", string.Empty));

            skillGo.SetActive(false);
            T skill = skillGo.AddComponent<T>();
            skill.SetEquipmentSet(equipmentSet);
            //skill.Character = _character;
            skill.IsPrefab = true;
            //skillGo.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(skillGo);
            var itemPrefabs = InventoryService.GetItemPrefabs();
            itemPrefabs.Add(skill.ItemIDString, skill);
            //skillGo.SetActive(true);

            Logger.LogDebug($"AddOrGetEquipmentSetSkillPrefab: Created skill prefab {skill.name}.");
            return skill;
        }

        public void AddEquipmentSetIcon(ItemDisplay itemDisplay, Item item)
        {
            var existingIcons = itemDisplay.GetComponentsInChildren<Image>().Where(i => i.name == "imgEquipmentSet").ToArray();

            if (!(item is EquipmentSetSkill setSkill))
            {
                for (int i = 0; i < existingIcons.Length; i++)
                    UnityEngine.Object.Destroy(existingIcons[i].gameObject);
                return;
            }
            else if (existingIcons.Any())
                return;

            var enchantedGo = itemDisplay.transform.Find("imgEnchanted").gameObject;
            var equipmentSetIconGo = UnityEngine.Object.Instantiate(enchantedGo, itemDisplay.transform);
            var existingImage = equipmentSetIconGo.GetComponent<Image>();
            UnityEngine.Object.DestroyImmediate(existingImage);

            equipmentSetIconGo.name = "imgEquipmentSet";
            var newImage = equipmentSetIconGo.AddComponent<Image>();
            newImage.sprite = ActionMenuResources.Instance.SpriteResources["EquipmentSetIcon"];
            equipmentSetIconGo.SetActive(true);

            Logger.LogDebug($"Added EquipmentSetIcon {equipmentSetIconGo.name} to ItemDisplay {itemDisplay.gameObject.name}.");
        }

        public void RemoveEquipmentSetPrefab(int setID)
        {

            if (ResourcesPrefabManager.Instance.ContainsItemPrefab(setID.ToString()))
            {
                var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(setID);
                InventoryService.GetItemPrefabs().Remove(setID.ToString());
                prefab.gameObject.Destroy();

                Logger.LogDebug($"{nameof(InventoryService)}::{nameof(RemoveEquipmentSetPrefab)}: Destroyed set prefab with SetID == {setID}.");
            }
        }
    }
}
