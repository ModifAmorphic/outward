using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting;
using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Data;
using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Linq;

namespace ModifAmorphic.Outward.ActionUI.Services
{

    public class WeaponSetsJsonService : JsonProfileService<EquipmentSetsProfile<WeaponSet>>, IEquipmentSetService<WeaponSet>
    {
        protected override string FileName => "WeaponSets.json";
        private InventoryService _inventoryService;
        private CraftingMenuEvents _craftingEvents;

        public WeaponSetsJsonService(GlobalProfileService globalProfileService, 
                                     ProfileService profileService, 
                                     InventoryService inventoryService, 
                                     CraftingMenuEvents craftingEvents, 
                                     string characterUID, 
                                     Func<IModifLogger> getLogger) : base(globalProfileService, profileService, characterUID, getLogger)
        {
            (_inventoryService, _craftingEvents) = (inventoryService, craftingEvents);
            _craftingEvents.DynamicCraftComplete += UpdateSetsCraftResults;
        }

        public event Action<WeaponSet> OnNewSet;
        public event OnRenamedSetDelegate<WeaponSet> OnRenamedSet;
        public event Action<string> OnDeletedSet;

        public event Action<EquipmentSetsProfile<WeaponSet>> OnProfileChanged;

        protected override void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            base.RefreshCachedProfile(actionMenusProfile, suppressChangeEvent);
            OnProfileChanged?.TryInvoke(GetProfile());
        }

        public EquipmentSetsProfile<WeaponSet> GetEquipmentSetsProfile()
        {
            var profiles = GetProfile();
            return profiles;
        }

        public WeaponSet GetEquipmentSet(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var profiles = GetProfile();
            var equipSet = profiles.EquipmentSets.FirstOrDefault(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return equipSet;
        }

        public void RenameEquipmentSet(string setName, string newName)
        {
            var equipSet = GetEquipmentSet(setName);
            if (equipSet == null)
                throw new ArgumentException($"Equipment set '{setName}' not found.", nameof(setName));

            equipSet.Name = newName;
            Save(equipSet);
            OnProfileChanged?.TryInvoke(GetProfile());

            try { OnRenamedSet?.Invoke(equipSet, setName, newName); }
            catch (Exception ex) { Logger.LogException(ex); }
        }

        public bool TryEquipSet(WeaponSet weaponSet) => _inventoryService.TryEquipWeaponSet(weaponSet);

        public bool IsSetEquipped(string name)
        {
            var set = GetEquipmentSet(name);
            if (set == null)
                return false;

            return IsSetEquipped(set);
        }
        public bool IsSetEquipped(WeaponSet weaponSet) => _inventoryService.IsWeaponSetEquipped(weaponSet);

        public bool IsContainedInSet(string itemUID) =>
            GetEquipmentSetsProfile().EquipmentSets.Any(set => set.GetEquipSlots().Any(slot => slot.UID == itemUID));

        public WeaponSet GetEquippedAsSet(string name)
        {
            var set = _inventoryService.GetEquippedAsWeaponSet(name);
            var existingSet = GetEquipmentSet(name);
            if (existingSet != null)
            {
                set.SetID = existingSet.SetID;
                set.IconSlot = existingSet.IconSlot;
            }

            return set;
        }

        public void SaveEquipmentSet(WeaponSet weaponSet)
        {
            var sets = GetProfile().EquipmentSets;

            var removed = sets.RemoveAll(s => s.Name.Equals(weaponSet.Name, StringComparison.InvariantCultureIgnoreCase));

            sets.Add(weaponSet);
            Save(weaponSet);

            if (removed < 1)
                OnNewSet?.TryInvoke(weaponSet);
        }

        public void DeleteEquipmentSet(string setName)
        {
            var sets = GetProfile().EquipmentSets;
            var removedSet = GetEquipmentSet(setName);
            var removed = sets.RemoveAll(s => s.Name.Equals(setName, StringComparison.InvariantCultureIgnoreCase));

            if (removed > 0)
            {
                _ = GlobalProfileService.RemoveEquipmentSet(removedSet.SetID);
                Save();
                ForgetEquipmentSetSkill(removedSet.SetID);
                OnDeletedSet?.Invoke(setName);
            }
        }

        public WeaponSet CreateNewEquipmentSet(string name, EquipSlots iconSlot)
        {
            if (GetSetExists(name))
                throw new ArgumentException($"Set with name '{name}' already exists.", nameof(name));

            var set = GetEquippedAsSet(name);
            set.IconSlot = iconSlot;
            set.SetID = GlobalProfileService.GetMinEquipmentSetID() - 1;

            GetProfile().EquipmentSets.Add(set);
            Save(set);

            var newSet = GetEquipmentSet(name);

            OnNewSet?.TryInvoke(newSet);

            return newSet;
        }

        private void Save(WeaponSet weaponSet)
        {
            GlobalProfileService.AddOrUpdateEquipmentSet(weaponSet, CharacterUID);
            Save();
            LearnEquipmentSetSkill(weaponSet);
        }

        private bool GetSetExists(string name)
            => GetEquipmentSet(name) != null;


        private void LearnEquipmentSetSkill(IEquipmentSet equipmentSet)
        {
            var prefab = _inventoryService.AddOrGetEquipmentSetSkillPrefab<WeaponSetSkill>(equipmentSet);
            _inventoryService.AddOrUpdateEquipmentSetSkill(prefab, equipmentSet);
        }

        private void ForgetEquipmentSetSkill(int SetID) => _inventoryService.RemoveEquipmentSetSkill(SetID);

        private void UpdateSetsCraftResults(DynamicCraftingResult result, int resultMultiplier, CustomCraftingMenu menu)
        {
            var sets = GetProfile().EquipmentSets.ToList();
            foreach (var set in sets)
            {
                bool saveSet = false;
                foreach (var equipSlot in set.GetEquipSlots())
                {

                    if (equipSlot != null && equipSlot.ItemID != 0 && !string.IsNullOrEmpty(equipSlot.UID))
                    {
                        if (result.IngredientCraftData.ConsumedItems.TryGetValue(equipSlot.ItemID, out var itemResults)
                            && itemResults.TryGetValue(equipSlot.UID, out var oldUID))
                        {
                            equipSlot.UID = result.ResultItem.UID;
                            saveSet = true;
                        }
                    }
                }
                if (saveSet)
                {
                    SaveEquipmentSet(set);
                    LearnEquipmentSetSkill(set);
                }
            }
        }
        public ISlotAction GetSlotActionPreview(IEquipmentSet set) => _inventoryService.GetEquipSkillPreview(set);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _craftingEvents.DynamicCraftComplete -= UpdateSetsCraftResults;
            _inventoryService = null;
            _craftingEvents = null;
        }
    }
}
