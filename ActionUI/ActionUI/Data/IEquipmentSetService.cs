using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public delegate void OnRenamedSetDelegate<T>(T equipmentSet, string oldName, string newName) where T : IEquipmentSet;
    public interface IEquipmentSetService<T> where T : IEquipmentSet
    {
        event Action<T> OnNewSet;
        event OnRenamedSetDelegate<T> OnRenamedSet;
        event Action<string> OnDeletedSet;

        EquipmentSetsProfile<T> GetEquipmentSetsProfile();
        //T GetActiveEquipmentSet();
        //T SetActiveEquipmentSet(string name);
        T GetEquipmentSet(string name);
        void RenameEquipmentSet(string setName, string newName);
        void SaveEquipmentSet(T set);
        void DeleteEquipmentSet(string setName);
        //void LearnEquipmentSetSkill(IEquipmentSet equipmentSet);
        //void ForgetEquipmentSetSkill(int SetID);
        ISlotAction GetSlotActionPreview(IEquipmentSet set);
        T CreateNewEquipmentSet(string name, EquipSlots iconSlot);
        T GetEquippedAsSet(string name);
        bool TryEquipSet(T equipmentSet);
        bool IsSetEquipped(string name);
        bool IsSetEquipped(T equipmentSet);
        bool IsContainedInSet(string itemUID);
    }
}
