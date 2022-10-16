using System;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public delegate void OnRenamedSkillChainDelegate(SkillChain skillChain, string oldName, string newName);
    public interface ISkillChainService
    {
        event Action<SkillChain> OnNewChain;
        event OnRenamedSkillChainDelegate OnRenamedChain;
        event Action<SkillChain> OnDeletedChain;

        SkillChainProfile GetSkillChainProfile();
        SkillChain GetSkillChain(string name);
        SkillChain GetSkillChain(int itemID);
        void RenameSkillChain(string existingName, string newName);
        void SaveSkillChain(SkillChain skillChain);
        void SaveNew(SkillChainProfile profile);
        void DeleteSkillChain(int itemID);
    }
}
