using ModifAmorphic.Outward.Extensions;
using NodeCanvas.StateMachines;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Quests.Models
{
    public class ModifQuestProgress : QuestProgress
    {
		protected List<QuestLogEntrySignature> m_logSignatures
		{
			get => this.GetPrivateField<QuestProgress, List<QuestLogEntrySignature>>("m_logSignatures");
			set => this.SetPrivateField<QuestProgress, List<QuestLogEntrySignature>>("m_logSignatures", value);
		}
		public List<QuestLogEntrySignature> LogSignatures => m_logSignatures;
		protected QuestTreeOwner m_questSequence
		{
			get => this.GetPrivateField<QuestProgress, QuestTreeOwner>("m_questSequence");
			set => this.SetPrivateField<QuestProgress, QuestTreeOwner>("m_questSequence", value);
		}

		protected List<QuestLogEntry> m_logEntries
		{
			get => this.GetPrivateField<QuestProgress, List<QuestLogEntry>>("m_logEntries");
			set => this.SetPrivateField<QuestProgress, List<QuestLogEntry>>("m_logEntries", value);
		}

		protected List<string> m_completedObjectivesToSave
		{
			get => this.GetPrivateField<QuestProgress, List<string>>("m_completedObjectivesToSave");
			set => this.SetPrivateField<QuestProgress, List<string>>("m_completedObjectivesToSave", value);
		}

		protected ProgressState m_progressState
		{
			get => this.GetPrivateField<QuestProgress, ProgressState>("m_progressState");
			set => this.SetPrivateField<QuestProgress, ProgressState>("m_progressState", value);
		}

		protected bool m_refreshDisplayRequired
		{
			get => this.GetPrivateField<QuestProgress, bool>("m_refreshDisplayRequired");
			set => this.SetPrivateField<QuestProgress, bool>("m_refreshDisplayRequired", value);
		}

		protected bool m_notificationRequired
		{
			get => this.GetPrivateField<QuestProgress, bool>("m_notificationRequired");
			set => this.SetPrivateField<QuestProgress, bool>("m_notificationRequired", value);
		}

		protected bool m_notificationPending
		{
			get => this.GetPrivateField<QuestProgress, bool>("m_notificationPending");
			set => this.SetPrivateField<QuestProgress, bool>("m_notificationPending", value);
		}

		protected AchievementOnQuestCompleted[] m_eventsOnComplete
		{
			get => this.GetPrivateField<QuestProgress, AchievementOnQuestCompleted[]>("m_eventsOnComplete");
			set => this.SetPrivateField<QuestProgress, AchievementOnQuestCompleted[]>("m_eventsOnComplete", value);
		}

		protected string[] m_nonConcurrentNPCs
		{
			get => this.GetPrivateField<QuestProgress, string[]>("m_nonConcurrentNPCs");
			set => this.SetPrivateField<QuestProgress, string[]>("m_nonConcurrentNPCs", value);
		}

		protected override void AwakeInit()
		{
			base.AwakeInit();
			
		}
	}
}
