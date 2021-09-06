using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using System;
using System.IO;
using System.Linq;

namespace ModifAmorphic.Outward.StashPacks.Sync
{
    internal class StashSaveExecuter
    {
        private readonly object _syncLock = new object();
        private readonly StashSaveData _stashSaveData;
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;


        public StashSaveExecuter(StashSaveData stashSaveData, Func<IModifLogger> getLogger)
        {
            (_stashSaveData, _getLogger) = (stashSaveData, getLogger);

            Logger.LogTrace($"{nameof(StashSaveExecuter)}:: New instance created.");
        }
        public void ExecutePlan(ContainerSyncPlan syncPlan, SaveInstance saveInstance)
        {
            var stashSave = _stashSaveData.GetStashSave(syncPlan.Area);
            var envSave = _stashSaveData.GetEnvironmentSave(stashSave.SceneName);

            if (syncPlan.HasDifferentSilverAmount())
            {
                var stashIndex = envSave.ItemList.FindIndex(i => i.Identifier.ToString() == syncPlan.UID);
                envSave.ItemList[stashIndex] = syncPlan.SaveDataAfter;
            }

            envSave.ItemList.RemoveAll(i => syncPlan.RemovedItems.ContainsKey(i.Identifier.ToString()));

            foreach (var modItem in syncPlan.ModifiedItems)
            {
                var modIndex = envSave.ItemList.FindIndex(i => i.Identifier.ToString() == modItem.Key);
                if (modIndex > -1)
                {
                    envSave.ItemList[modIndex] = modItem.Value;
                }
                else
                {
                    modItem.Value.TryGetItemID(out var itemId);
                    Logger.LogError($"{nameof(StashSaveExecuter)}::{nameof(ExecutePlan)}: Could not modify stash for item with UID '{modItem.Key}', ItemID '{itemId}'. " +
                        $"Item in stash not found. Bad sync plan or EnvironmentSave changed between plan and execute.");
                }
            }
            envSave.ItemList.AddRange(syncPlan.AddedItems.Values.ToList());

            var (OriginalFileName, BackupFileName) = BackupCurrentSave(envSave, saveInstance.SavePath);
            try
            {
                Logger.LogInfo($"{nameof(StashSaveExecuter)}::{nameof(ExecutePlan)}: Saving area '{stashSave.SceneName}'.");
                envSave.ProcessSave();
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(StashSaveExecuter)}::{nameof(ExecutePlan)}: Failed to save area '{stashSave.SceneName}'.", ex);
                RestoreSave(BackupFileName, OriginalFileName);
            }

        }
        private (string OriginalFileName, string BackupFileName) BackupCurrentSave(EnvironmentSave envSave, string savePath)
        {
            envSave.SetSavePath(savePath);

            var saveDirectory = Path.GetDirectoryName(envSave.GetFullFilePath()) + Path.DirectorySeparatorChar;
            var bakSaveFileName = saveDirectory + Path.GetFileNameWithoutExtension(envSave.GetFullFilePath()) + ".bak";

            if (File.Exists(bakSaveFileName))
            {
                Logger.LogError($"{nameof(StashSaveExecuter)}::{nameof(BackupCurrentSave)}: Error: " +
                    $"Backup save for area '{envSave.AreaName}' already exists but should not. Aborting StashPack save." +
                    $"Changes made to the StashPack since last save will not be captured in this save.\n" +
                    $"\tGetFullFilePath(): {envSave.GetFullFilePath()}\n" +
                    $"\tBackup Save File: {bakSaveFileName}");
                return default;
            }
            Logger.LogInfo($"Backing up save for area {envSave.AreaName} to file {bakSaveFileName}.");

            File.Move(envSave.GetFullFilePath(), bakSaveFileName);

            return (envSave.GetFullFilePath(), bakSaveFileName);
        }
        private void RestoreSave(string backupFileName, string restoreFileName)
        {
            if (!File.Exists(backupFileName))
            {
                Logger.LogError($"{nameof(StashSaveExecuter)}::{nameof(RestoreSave)}: Error: " +
                    $"Backup save file '{backupFileName}' not found. Backup cannot be restored for '{restoreFileName}'.");
                return;
            }

            Logger.LogInfo($"Restoring save '{restoreFileName}' from backup '{backupFileName}'.");
            File.Move(backupFileName, restoreFileName);
        }
    }
}
