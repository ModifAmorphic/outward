using BepInEx.Logging;
using System;

namespace ModifAmorphic.Outward.GreaterRunicBladePotion
{
    public class GreatRunicBladeEffect : RunicBlade
    {
        public ManualLogSource Logger => GreatRunicBladePotion.ModLogger;
        public override void AwakeInit()
        {
            base.AwakeInit();
            RunicBladePrefab = (Weapon)ResourcesPrefabManager.Instance.GetItemPrefab(2000100);
            RunicGreatBladePrefab = (Weapon)ResourcesPrefabManager.Instance.GetItemPrefab(2100999);
            ImbueAmplifierRunicBlade = (ImbueEffectPreset)ResourcesPrefabManager.Instance.GetEffectPreset(219);
            ImbueAmplifierGreatRunicBlade = (ImbueEffectPreset)ResourcesPrefabManager.Instance.GetEffectPreset(211);

            Logger.LogDebug($"Set {nameof(GreatRunicBladeEffect)} RunicBladePrefab to prefab '{RunicBladePrefab.name}', RunicGreatBladePrefab to prefab '{RunicGreatBladePrefab.name}', " +
                $"ImbueAmplifierRunicBlade to {ImbueAmplifierRunicBlade.name}, ImbueAmplifierGreatRunicBlade to {ImbueAmplifierGreatRunicBlade.name}.");
        }
        public override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            Logger.LogDebug($"{nameof(GreatRunicBladeEffect)}.ActivateLocally triggered for character {_affectedCharacter?.name}.");

            if (_affectedCharacter == null)
                return;

            Logger.LogDebug($"RunicBladePrefab is {(this.RunicBladePrefab != null ? "not" : "")} null. " +
                $"Character does {(!_affectedCharacter.Inventory.HasEquipped(this.RunicBladePrefab.ItemID) ? "not" : "")} have a RunicBlade equipped.");
            if (this.RunicBladePrefab != null && !_affectedCharacter.Inventory.HasEquipped(this.RunicBladePrefab.ItemID))
            {
                Weapon weapon = ItemManager.Instance.GenerateItem(this.RunicBladePrefab.ItemID) as Weapon;
                weapon.SetHolderUID((_affectedCharacter.UID + "_" + this.RunicBladePrefab.name));
                weapon.ClientGenerated = PhotonNetwork.isNonMasterClientInRoom;
                weapon.SetKeepAlive();
                Item equippedItem = _affectedCharacter.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.RightHand);
                weapon.GetComponent<SummonedEquipment>().Activate(EnvironmentConditions.ConvertToGameTime(this.SummonLifeSpan), equippedItem != null ? equippedItem.UID : null, null);
                if (this.ImbueAmplifierRunicBlade != null && _affectedCharacter.Inventory.SkillKnowledge.IsItemLearned(8205200))
                {
                    weapon.AddImbueEffect(this.ImbueAmplifierRunicBlade, this.SummonLifeSpan);
                    Logger.LogDebug($"Added imbue effect {ImbueAmplifierRunicBlade.name}.");
                }
                if (equippedItem != null)
                {
                    _affectedCharacter.Inventory.UnequipItem((Equipment)equippedItem);
                    equippedItem.ForceUpdateParentChange();
                    Logger.LogDebug($"Unequipped item {equippedItem.name}.");
                }
                weapon.transform.SetParent(_affectedCharacter.Inventory.GetMatchingEquipmentSlot(EquipmentSlot.EquipmentSlotIDs.RightHand).transform);
                weapon.ForceStartInit();
                Logger.LogDebug($"Equipped runic blade {weapon.name}.");
            }
            else
            {
                Logger.LogDebug($"RunicGreatBladePrefab is {(this.RunicGreatBladePrefab == null ? "" : "not")} null. " +
                    $"RunicBladePrefab is {(this.RunicBladePrefab == null ? "" : "not")} null. " +
                            $"Character does {(!_affectedCharacter.Inventory.HasEquipped(this.RunicBladePrefab.ItemID) ? "not" : "")} have a RunicBlade equipped.");

                if (this.RunicGreatBladePrefab == null || this.RunicBladePrefab == null || !_affectedCharacter.Inventory.HasEquipped(this.RunicBladePrefab.ItemID))
                    return;
                Item equippedItem1 = _affectedCharacter.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.RightHand);
                SummonedEquipment component = equippedItem1.GetComponent<SummonedEquipment>();
                Weapon itemNetwork = ItemManager.Instance.GenerateItemNetwork(this.RunicGreatBladePrefab.ItemID) as Weapon;
                itemNetwork.SetHolderUID(_affectedCharacter.UID + "_" + this.RunicGreatBladePrefab.name);
                itemNetwork.ClientGenerated = PhotonNetwork.isNonMasterClientInRoom;
                itemNetwork.SetKeepAlive();
                Item equippedItem2 = _affectedCharacter.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.LeftHand);
                var previousLeftHandUID = equippedItem2 == null || equippedItem2.UID == component.PreviousRightHand ? null : equippedItem2.UID;
                itemNetwork.GetComponent<SummonedEquipment>().Activate(EnvironmentConditions.ConvertToGameTime(this.SummonLifeSpan), component.PreviousRightHand, previousLeftHandUID);

                if (_affectedCharacter.Inventory.SkillKnowledge.IsItemLearned(8205200))
                {
                    itemNetwork.AddImbueEffect(this.ImbueAmplifierGreatRunicBlade, this.SummonLifeSpan);
                    Logger.LogDebug($"Added imbue effect {ImbueAmplifierGreatRunicBlade.name}.");
                }
                if (equippedItem1 != null)
                {
                    _affectedCharacter.Inventory.UnequipItem((Equipment)equippedItem1);
                    equippedItem1.ForceUpdateParentChange();
                    Logger.LogDebug($"Unequipped item {equippedItem1.name}.");
                }
                if (equippedItem2 != null)
                {
                    _affectedCharacter.Inventory.UnequipItem((Equipment)equippedItem2);
                    equippedItem2.ForceUpdateParentChange();
                    Logger.LogDebug($"Unequipped item {equippedItem2.name}.");
                }
                itemNetwork.transform.SetParent(_affectedCharacter.Inventory.GetMatchingEquipmentSlot(EquipmentSlot.EquipmentSlotIDs.RightHand).transform);
                itemNetwork.ForceStartInit();
            }
        }
    }
}
