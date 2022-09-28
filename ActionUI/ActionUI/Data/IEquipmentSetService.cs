using ModifAmorphic.Outward.Unity.ActionUI.EquipmentSets;
using ModifAmorphic.Outward.Unity.ActionUI.Models.EquipmentSets;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public delegate void OnRenamedSetDelegate<T>(T equipmentSet, string oldName, string newName) where T : IEquipmentSet;
    public interface IEquipmentSetService <T> where T : IEquipmentSet
    {
        event OnRenamedSetDelegate<T> OnRenamedSet;
        event Action<T> OnNewSet;

        EquipmentSetsProfile<T> GetEquipmentSetsProfile();
        //T GetActiveEquipmentSet();
        //T SetActiveEquipmentSet(string name);
        T GetEquipmentSet(string name);
        int GetLastSetID();
        void RenameEquipmentSet(string setName, string newName);
        void SaveEquipmentSet(T set);
        void LearnEquipmentSetSkill(IEquipmentSet equipmentSet);
        ISlotAction GetSlotActionPreview(IEquipmentSet set);
        T CreateEmptyEquipmentSet(string name);
        T GetEquippedAsSet(string name);
        bool TryEquipSet(T equipmentSet);
        bool IsSetEquipped(string name);
        bool IsSetEquipped(T equipmentSet);
        bool IsContainedInSet(string itemUID);
    }
}
