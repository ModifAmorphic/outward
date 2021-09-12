using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.SkillSchools.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ModifAmorphic.Outward.Modules.Instances
{
    public class CharacterInstances : IModifModule
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(SkillTreeHolderPatches),
            typeof(CharacterManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(SkillTreeHolderPatches),
            typeof(CharacterManagerPatches)
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal CharacterInstances(Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            SkillTreeHolderPatches.AwakeAfter += SkillTreeHolder_Awake;
            CharacterManagerPatches.AwakeAfter += CharacterManagerPatches_AwakeAfter;
        }

        private CharacterManager _characterManager;
        private void CharacterManagerPatches_AwakeAfter(CharacterManager characterManager) => _characterManager = characterManager;
        public bool TryGetCharacterManager(out CharacterManager characterManager)
        {
            if (_characterManager != null)
            {
                characterManager = _characterManager;
                return true;
            }
            else if (CharacterManager.Instance != null)
            {
                _characterManager = CharacterManager.Instance;
                characterManager = _characterManager;
                return true;
            }
            characterManager = default;
            return false;
        }

        private SkillTreeHolder _skillTreeHolder;
        private void SkillTreeHolder_Awake(SkillTreeHolder skillTreeHolder) => this._skillTreeHolder = skillTreeHolder;

        public bool TryGetSkillTreeHolder(out SkillTreeHolder skillTreeHolder)
        {
            if (_skillTreeHolder != null)
            {
                skillTreeHolder = _skillTreeHolder;
                return true;
            }
            else if (SkillTreeHolder.Instance != null)
            {
                _skillTreeHolder = SkillTreeHolder.Instance;
                skillTreeHolder = _skillTreeHolder;
                return true;
            }
            skillTreeHolder = default;
            return false;
        }
        public bool TryGetSkillSchools(out IDictionary<int, SkillSchool> skillSchools)
        {
            skillSchools = new Dictionary<int, SkillSchool>();
            if (!TryGetSkillTreeHolder(out var skillTreeHolder))
                return false;

            var schools = skillTreeHolder.GetComponentsInChildren<SkillSchool>();
            if (schools == null || schools.Length < 1)
                return false;

            for (var i = 0; i < schools.Length; i++)
            {
                skillSchools.Add(i + 1, schools[i]);
            }

            return true;
        }
    }
}
