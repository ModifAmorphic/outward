using ModifAmorphic.Outward.ActionUI.Models;
using ModifAmorphic.Outward.Events;
using ModifAmorphic.Outward.Logging;
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
        private EquipService _equipService => _getEquipService.Invoke();
        private Func<EquipService> _getEquipService;

        public ArmorSetsJsonService(GlobalProfileService globalProfileService,
                                     ProfileService profileService,
                                     Func<EquipService> getEquipService,
                                     string characterUID,
                                     Func<IModifLogger> getLogger) : base(globalProfileService, profileService, characterUID, getLogger)
        {
            _getEquipService = getEquipService;
            TransmorphicEventsEx.TryHookOnTransmogrified(this, OnTransmogrified);
        }

        public event Action<ArmorSet> OnNewSet;
        public event OnRenamedSetDelegate<ArmorSet> OnRenamedSet;
        public event Action<string> OnDeletedSet;

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
            Save(equipSet);
            OnProfileChanged?.TryInvoke(GetProfile());
            try { OnRenamedSet?.Invoke(equipSet, setName, newName); }
            catch (Exception ex) { Logger.LogException(ex); }
        }

        public bool TryEquipSet(ArmorSet armorSet) => _equipService.TryEquipArmorSet(armorSet);

        public bool IsSetEquipped(string name)
        {
            var set = GetEquipmentSet(name);
            if (set == null)
                return false;

            return IsSetEquipped(set);
        }

        public bool IsSetEquipped(ArmorSet armorSet) => _equipService.IsArmorSetEquipped(armorSet);

        public bool IsContainedInSet(string itemUID) =>
            GetEquipmentSetsProfile().EquipmentSets.Any(set => set.GetEquipSlots().Any(slot => slot != null && slot.UID == itemUID));


        public ArmorSet GetEquippedAsSet(string name)
        {
            var set = _equipService.GetEquippedAsArmorSet(name);
            var existingSet = GetEquipmentSet(name);
            if (existingSet != null)
            {
                set.SetID = existingSet.SetID;
                set.IconSlot = existingSet.IconSlot;
            }

            return set;
        }

        public void SaveEquipmentSet(ArmorSet armorSet)
        {
            Logger.LogInfo($"Saving equipment set '{armorSet.Name}'.");
            var sets = GetProfile().EquipmentSets;

            var removed = sets.RemoveAll(s => s.Name.Equals(armorSet.Name, StringComparison.InvariantCultureIgnoreCase));

            sets.Add(armorSet);
            Save(armorSet);

            if (removed < 1)
                OnNewSet?.TryInvoke(armorSet);
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

        public ArmorSet CreateNewEquipmentSet(string name, EquipSlots iconSlot)
        {
            if (GetSetExists(name))
                throw new ArgumentException($"Set with name '{name}' already exists.", nameof(name));

            var set = GetEquippedAsSet(name);
            set.IconSlot = iconSlot;
            set.SetID = GlobalProfileService.GetNextEquipmentSetID();

            GetProfile().EquipmentSets.Add(set);
            Save(set);
            var newSet = GetEquipmentSet(name);
            OnNewSet?.TryInvoke(newSet);

            return newSet;
        }


        private void LearnEquipmentSetSkill(IEquipmentSet equipmentSet)
        {
            Logger.LogDebug($"Learning Armor Set Skill for set '{equipmentSet.Name}'");
            _equipService.AddOrUpdateEquipmentSetSkill<ArmorSetSkill>(equipmentSet);
        }

        private void ForgetEquipmentSetSkill(int SetID) => _equipService.RemoveEquipmentSet(SetID);

        private bool GetSetExists(string name)
            => GetEquipmentSet(name) != null;

        private void Save(ArmorSet armorSet)
        {
            GlobalProfileService.AddOrUpdateEquipmentSet(armorSet, CharacterUID);
            Save();
            LearnEquipmentSetSkill(armorSet);
        }

        private void OnTransmogrified(int consumedItemID, string consumedItemUID, int transmogItemID, string transmogItemUID)
        {
            Logger.LogDebug($"ArmorSetsJsonService::OnTransmogrified" +
                    $"(consumedItemID: {consumedItemID}, consumedItemUID: '{consumedItemUID}', transmogItemID: {transmogItemID}, transmogItemUID: '{transmogItemUID}')");
            var sets = GetProfile().EquipmentSets.ToList();
            foreach (var set in sets)
            {
                bool saveSet = false;
                foreach (var equipSlot in set.GetEquipSlots())
                {

                    if (equipSlot != null && equipSlot.ItemID != 0 && !string.IsNullOrEmpty(equipSlot.UID))
                    {
                        if (equipSlot.ItemID == consumedItemID && equipSlot.UID == consumedItemUID)
                        {
                            equipSlot.UID = transmogItemUID;
                            saveSet = true;
                            Logger.LogDebug($"Found and updated set {set.Name} ItemID {equipSlot.ItemID}'s UID from '{consumedItemUID}' to '{transmogItemUID}'");
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

        public ISlotAction GetSlotActionPreview(IEquipmentSet set) => _equipService.GetEquipSkillPreview(set);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
