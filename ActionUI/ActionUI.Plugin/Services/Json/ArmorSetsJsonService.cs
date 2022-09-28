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

    public class ArmorSetsJsonService : JsonProfileService<EquipmentSetsProfile<ArmorSet>>, IEquipmentSetService<ArmorSet>
    {
        protected override string FileName => "ArmorSets.json";
        private InventoryService _inventoryService;
        private CraftingMenuEvents _craftingEvents;

        public ArmorSetsJsonService(ProfileService profileService, InventoryService inventoryService, CraftingMenuEvents craftingEvents, Func<IModifLogger> getLogger) : base(profileService, getLogger)
        {
            (_inventoryService, _craftingEvents) = (inventoryService, craftingEvents);
            _craftingEvents.DynamicCraftComplete += UpdateSetsCraftResults;
        }

        public event Action<ArmorSet> OnNewSet;
        public event OnRenamedSetDelegate<ArmorSet> OnRenamedSet;

        public event Action<EquipmentSetsProfile<ArmorSet>> OnProfileChanged;

        protected override void RefreshCachedProfile(IActionUIProfile actionMenusProfile, bool suppressChangeEvent = false)
        {
            base.RefreshCachedProfile(actionMenusProfile, suppressChangeEvent);
            OnProfileChanged?.TryInvoke(GetProfile());
        }

        public EquipmentSetsProfile<ArmorSet> GetEquipmentSetsProfile()
        {
            var profiles = GetProfile();
            return profiles;
        }

        //public ArmorSet GetActiveEquipmentSet()
        //{
        //    if (_cachedSet != null)
        //        return _cachedSet;

        //    var profiles = GetProfile();
        //    _cachedSet = GetEquipmentSet(profiles.ActiveSetName);
        //    return _cachedSet;
        //}

        //public ArmorSet SetActiveEquipmentSet(string name)
        //{
        //    var activeSet = GetActiveEquipmentSet();
        //    if (activeSet != null && activeSet.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
        //        return activeSet;

        //    var profiles = GetProfile();
        //    profiles.ActiveSetName = name;
        //    base.Save();
        //    _cachedSet = null;
        //    Logger.LogDebug($"Set Active Armor Set to '{name}'.");
        //    OnSetActiveSet?.TryInvoke(GetActiveEquipmentSet());
        //    return GetActiveEquipmentSet();
        //}

        public int GetLastSetID() => GetProfile().EquipmentSets.Min(e => e.SetID);

        public ArmorSet GetEquipmentSet(string name)
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
            Save();
            OnProfileChanged?.TryInvoke(GetProfile());
            try { OnRenamedSet?.Invoke(equipSet, setName, newName); }
            catch (Exception ex) { Logger.LogException(ex); }
        }

        public bool TryEquipSet(ArmorSet armorSet) => _inventoryService.TryEquipArmorSet(armorSet);

        public bool IsSetEquipped(string name)
        {
            var set = GetEquipmentSet(name);
            if (set == null)
                return false;

            return IsSetEquipped(set);
        }

        public bool IsSetEquipped(ArmorSet armorSet) => _inventoryService.IsArmorSetEquipped(armorSet);

        public bool IsContainedInSet(string itemUID) =>
            GetEquipmentSetsProfile().EquipmentSets.Any(set => set.GetEquipSlots().Any(slot => slot != null && slot.UID == itemUID));


        public ArmorSet GetEquippedAsSet(string name)
        {
            var set = _inventoryService.GetEquippedAsArmorSet(name);
            var existingSet = GetEquipmentSet(name);
            if (existingSet != null)
            {
                set.SetID = existingSet.SetID;
                set.SlotIcon = existingSet.SlotIcon;
            }

            return set;
        }

        public void SaveEquipmentSet(ArmorSet armorSet)
        {
            bool isUpdate = false;
            var sets = GetProfile().EquipmentSets;

            if (sets.Any(s => s.Name.Equals(armorSet.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                sets.RemoveAll(s => s.Name.Equals(armorSet.Name, StringComparison.InvariantCultureIgnoreCase));
                isUpdate = true;
            }

            sets.Add(armorSet);
            Save();

            if (!isUpdate)
                OnNewSet(armorSet);
        }

        public ArmorSet CreateEmptyEquipmentSet(string name)
        {
            if (GetSetExists(name))
                throw new ArgumentException($"Set with name '{name}' already exists.", nameof(name));

            GetProfile().EquipmentSets.Add(new ArmorSet() { Name = name });
            Save();
            var newSet = GetEquipmentSet(name);
            OnNewSet?.TryInvoke(newSet);

            return newSet;
        }

        public void LearnEquipmentSetSkill(IEquipmentSet equipmentSet) => _inventoryService.LearnEquipmentSetSkill<ArmorSetSkill>(equipmentSet);

        private bool GetSetExists(string name)
            => GetEquipmentSet(name) != null;

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
