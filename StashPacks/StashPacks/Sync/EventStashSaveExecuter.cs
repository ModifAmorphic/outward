using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.SaveData.Data;
using ModifAmorphic.Outward.StashPacks.Sync.Models;
using ModifAmorphic.Outward.StashPacks.SaveData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModifAmorphic.Outward.StashPacks.Patch.Events;
using System.Collections.Concurrent;
using System.IO;
using ModifAmorphic.Outward.StashPacks.Sync.Extensions;
using ModifAmorphic.Outward.StashPacks.Extensions;
using UnityEngine.SceneManagement;

namespace ModifAmorphic.Outward.StashPacks.Sync
{
    internal class EventStashSaveExecuter : IDisposable
    {
        private readonly object _syncLock = new object();
        private readonly StashSaveData _stashSaveData;
        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly ConcurrentDictionary<AreaManager.AreaEnum, Action<SaveInstance>> _beforeSaveExecutions = 
            new ConcurrentDictionary<AreaManager.AreaEnum, Action<SaveInstance>>();

        private readonly ConcurrentDictionary<string, Action> _afterSaveRenames =
            new ConcurrentDictionary<string, Action>();

        public EventStashSaveExecuter(StashSaveData stashSaveData, Func<IModifLogger> getLogger)
        {
            (_stashSaveData, _getLogger) = (stashSaveData, getLogger);

            Logger.LogTrace($"{nameof(EventStashSaveExecuter)}:: New instance created.");
            //CharacterSaveInstanceHolderEvents.SaveBefore += ExecuteBeforeSaveActions;
            //CharacterSaveInstanceHolderEvents.SaveAfter += ExecuteAfterSaveActions;
        }
        public void SubscribeToSaves()
        {
            SaveInstanceEvents.SaveBefore += ExecuteBeforeSaveActions;
            SaveInstanceEvents.SaveAfter += ExecuteAfterSaveActions;
            Logger.LogTrace($"{nameof(EventStashSaveExecuter)}::{nameof(SubscribeToSaves)}: Subscribed to SaveEvents.");
        }
        public void ExecutePlan(AreaManager.AreaEnum area, Func<ContainerSyncPlan> planGenerator, Action<ContainerSyncPlan> planLogger = null)
        {
            var syncPlan = planGenerator.Invoke();
            if (!syncPlan.HasChanges())
            {
                Logger.LogInfo($"Sync Plan for '{syncPlan.Area.GetName()}' {syncPlan.ContainerType.GetName()} has no changes to execute.");
                return;
            }

            Action<SaveInstance> executeBefore = (SaveInstance saveInstance) => ExecuteBeforeSave(saveInstance, syncPlan, planLogger);
            Logger.LogDebug($"{nameof(EventStashSaveExecuter)}::{nameof(ExecutePlan)}: AddOrUpdate Stash Update for Area {area.GetName()}");
            _beforeSaveExecutions.AddOrUpdate(area, executeBefore, (k, v) => executeBefore);
        }

        private void ExecuteBeforeSaveActions(SaveInstance saveInstance)
        {
            try
            {
                Logger.LogTrace($"{nameof(EventStashSaveExecuter)}::{nameof(ExecuteBeforeSaveActions)}: Character '{saveInstance?.CharSave.CharacterUID}' " +
                    $"has {_beforeSaveExecutions.Values.Count} Save Actions to process");
                foreach (var executer in _beforeSaveExecutions.Values)
                {
                    executer.Invoke(saveInstance);
                }
                _beforeSaveExecutions.Clear();
            }
            catch (Exception ex) //Catch and log so the rest of save process isn't screwed up
            {
                Logger.LogException($"{nameof(EventStashSaveExecuter)}::{nameof(ExecuteBeforeSaveActions)}: Error processing BeforeSaveAction for Character '{saveInstance.CharSave?.CharacterUID}', Path: " +
                    $"{saveInstance.SavePath}", ex);
            }
        }

        private void ExecuteAfterSaveActions(SaveInstance saveInstance)
        {
            Logger.LogTrace($"{nameof(EventStashSaveExecuter)}::{nameof(ExecuteAfterSaveActions)}: Character '{saveInstance.CharSave?.CharacterUID}' " +
                $"has {_beforeSaveExecutions.Values.Count} Save Actions to process");
            foreach (var rename in _afterSaveRenames.Values)
            {
                rename.Invoke();
            }
            _afterSaveRenames.Clear();
        }
        
        private void ExecuteBeforeSave(SaveInstance saveInstance, ContainerSyncPlan syncPlan, Action<ContainerSyncPlan> planLogger)
        {
            lock (_syncLock)
            {
                //var syncPlan = planGenerator.Invoke();
                if (syncPlan == null)
                {
                    Logger.LogDebug($"{nameof(EventStashSaveExecuter)}::{nameof(ExecuteBeforeSave)}: Null plan generated. Aborting.");
                    return;
                }
                planLogger?.Invoke(syncPlan);
                var stashSave = _stashSaveData.GetStashSave(syncPlan.Area);
                var envSave = _stashSaveData.GetEnvironmentSave(stashSave.SceneName);


                envSave.ItemList.RemoveAll(i => syncPlan.RemovedItems.ContainsKey(i.Identifier.ToString()));

                foreach (var modItem in syncPlan.ModifiedItems)
                {
                    var modIndex = envSave.ItemList.FindIndex(i => i.Identifier.ToString() == modItem.Key);
                    if (modIndex > -1)
                        envSave.ItemList[modIndex] = modItem.Value;
                    else
                    {
                        modItem.Value.TryGetItemID(out var itemId);
                        Logger.LogError($"Could not modify stash for item with UID '{modItem.Key}', ItemID '{itemId}'. Item in stash not found. Bad sync plan or EnvironmentSave changed between plan and execute.");
                    }
                }
                var stashItems = syncPlan.AddedItems.Values.ToList();
                if (syncPlan.HasDifferentSilverAmount())
                    stashItems.Add(syncPlan.SaveDataAfter);
                envSave.ItemList.AddRange(stashItems);

                RenameToStashPackSave(envSave, saveInstance.SavePath);
            }
        }
        private void RenameToStashPackSave(EnvironmentSave envSave, string savePath)
        {
            envSave.SetSavePath(savePath);

            var saveDirectory = Path.GetDirectoryName(envSave.GetFullFilePath()) + Path.DirectorySeparatorChar;
            var tmpSaveFileName = saveDirectory + Path.GetFileNameWithoutExtension(envSave.GetFullFilePath()) + ".stashpack";

            if (File.Exists(tmpSaveFileName))
            {
                Logger.LogError($"{nameof(EventStashSaveExecuter)}::{nameof(RenameToStashPackSave)}: Error: " +
                    $"Temporary StashPack save for area '{envSave.AreaName}' already exists but should not. Aborting StashPack save." +
                    $"Changes made to the StashPack since last save will not be captured in this save.\n" +
                    $"\tGetFullFilePath(): {envSave.GetFullFilePath()}\n" +
                    $"\tTemporaryStashPackFile: {tmpSaveFileName}");
                return;
            }
            Logger.LogInfo($"Processing save for area {envSave.AreaName} to file {tmpSaveFileName}.");
            envSave.ProcessSave();

            File.Move(envSave.GetFullFilePath(), tmpSaveFileName);

            Action renameAfter = () => PostFileRename(tmpSaveFileName, envSave.GetFullFilePath());

            _afterSaveRenames.AddOrUpdate(envSave.AreaName, renameAfter, (k, v) => renameAfter);
        }
        private void PostFileRename(string preFileName, string postFileName)
        {
            if (!File.Exists(preFileName))
            {
                Logger.LogError($"{nameof(EventStashSaveExecuter)}::{nameof(PostFileRename)}: Error: " +
                        $"Could not find temporary StashPack file '{preFileName}' to rename. Not replacing '{postFileName}'. " +
                        "Aborting safe file replace. Changes made to the StashPack since last save will not be captured in this save.\n" +
                            $"\tPreSaveFileName: {preFileName}\n" +
                            $"\tTargetFileName: {postFileName}");
                return;
            }
            var saveDirectory = Path.GetDirectoryName(postFileName) + Path.DirectorySeparatorChar;
            var backupAreaFile = saveDirectory + Path.GetFileNameWithoutExtension(postFileName) + ".bak";

            if (File.Exists(backupAreaFile))
            {
                Logger.LogError($"{nameof(EventStashSaveExecuter)}::{nameof(PostFileRename)}: Error: " +
                        $"Backup file should not exists but does. Backup file '{backupAreaFile}' already exists and should not. " +
                        "Aborting area save file replace. Changes made to the StashPack since last save will not be captured in this save.\n" +
                            $"\tPreSaveFileName: {preFileName}\n" +
                            $"\tBackupFileName: {backupAreaFile}\n" +
                            $"\tTargetFileName: {postFileName}");
                return;
            }
            Logger.LogInfo($"Renaming area save file {postFileName} to {backupAreaFile}. Renaming temporary stashpack save to {preFileName} to area save {postFileName}.");
            //Rename the File created by the the builtin SaveInstance.Save() to a backup file.
            File.Move(postFileName, backupAreaFile);
            //Rename the temporary file stashpack save to the name the game expects to find for this area.
            File.Move(preFileName, postFileName);
        }

        public void Dispose()
        {
            Logger.LogDebug($"{nameof(EventStashSaveExecuter)} instance is disposing.");
            _beforeSaveExecutions.Clear();
            _afterSaveRenames.Clear();
            SaveInstanceEvents.SaveBefore -= ExecuteBeforeSaveActions;
            SaveInstanceEvents.SaveAfter -= ExecuteAfterSaveActions;
        }
    }
}
