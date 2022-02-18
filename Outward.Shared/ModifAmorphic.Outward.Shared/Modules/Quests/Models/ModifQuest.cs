using ModifAmorphic.Outward.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Quests.Models
{
	public class ModifQuest : Quest
	{
		protected QuestProgress m_questProgress
		{
			get => this.GetPrivateField<Quest, QuestProgress>("m_questProgress");
			set => this.SetPrivateField<Quest, QuestProgress>("m_questProgress", value);
		}

		protected float m_completionGameTime
		{
			get => this.GetPrivateField<Quest, float>("m_completionGameTime");
			set => this.SetPrivateField<Quest, float>("m_completionGameTime", value);
		}

		protected bool m_registered
		{
			get => this.GetPrivateField<Quest, bool>("m_registered");
			set => this.SetPrivateField<Quest, bool>("m_registered", value);
		}

		protected Character m_host
		{
			get => this.GetPrivateField<Quest, Character>("m_host");
			set => this.SetPrivateField<Quest, Character>("m_host", value);
		}

		//private string _name;
		//public override string Name
		//{
		//	get => _name;
		//	protected set => _name = value;
		//}
		//public void SetQuestName(string name) => _name = name;

		//public override string DisplayName => _name;

		//public override string TypeDisplay => GetType().ToString();

		//private string _description;
		//public override string Description => _description;
		//public void SetDescription(string description) => _description = description;


		protected override void OnAwake()
        {
            base.OnAwake();
            //m_questProgress = GetComponent<QuestProgress>();
        }
    }
}
