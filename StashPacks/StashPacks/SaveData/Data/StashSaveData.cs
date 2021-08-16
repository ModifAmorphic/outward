using ModifAmorphic.Outward.StashPacks.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.SaveData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ModifAmorphic.Outward.Extensions;

namespace ModifAmorphic.Outward.StashPacks.SaveData.Data
{

    internal class StashSaveData
    {
        internal string CharacterUID => _characterSaveInstanceHolder.CharacterUID;

        private readonly CharacterSaveInstanceHolder _characterSaveInstanceHolder;
        private readonly AreaManager _areaManager;
        private readonly IReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)> _areaStashes;

        public string SaveFilePath => _characterSaveInstanceHolder.CurrentSaveInstance.SavePath;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        public StashSaveData(AreaManager areaManager
            , CharacterSaveInstanceHolder characterSaveInstanceHolder
            , IReadOnlyDictionary<AreaManager.AreaEnum, (string StashUID, int ItemId)> areaStashes
            , Func<IModifLogger> getLogger) =>
                (_areaManager, _areaStashes, _characterSaveInstanceHolder, _getLogger) = (areaManager, areaStashes, characterSaveInstanceHolder, getLogger);

        /// <summary>
        /// Loads the Scene <see cref="EnvironmentSave"/> from the <see cref="StashSaveData.SaveFilePath"/> of this instance.
        /// </summary>
        /// <param name="sceneName">The name of the scene <see cref="EnvironmentSave"/> to be loaded.</param>
        /// <returns>A new <see cref="EnvironmentSave"/> instance if a save is found, otherwise null</returns>
        public EnvironmentSave GetEnvironmentSave(string sceneName)
        {
            var envSave = new EnvironmentSave()
            {
                AreaName = sceneName
            };
            if (envSave.LoadFromFile(SaveFilePath))
                return envSave;

            return null;
        }
        /// <summary>
        /// Finds and returns save file paths for every area containing a stash that this <see cref="StashSaveData.CharacterUID" /> has a save for.
        /// </summary>
        /// <returns>A Dictionary collection of SceneNames and their SaveFilePaths, indexed by <see cref="AreaManager.AreaEnum"/>.</returns>
        public Dictionary<AreaManager.AreaEnum, (string SceneName, string SaveFilePath)> GetStashesSavePaths()
        {
            var savePaths = new Dictionary<AreaManager.AreaEnum, (string SceneName, string SaveFilePath)>();
            foreach (var area in _areaStashes.Keys)
            {
                Logger.LogDebug($"{nameof(StashSaveData)}::{nameof(GetStashesSavePaths)}: Getting Save Path for {area.GetName()} area, character {_characterSaveInstanceHolder?.CharacterUID}. " +
                    $"SavePath: {_characterSaveInstanceHolder.CurrentSaveInstance?.SavePath}");
                var sceneName = _areaManager.GetArea(area)?.SceneName;
                if (!string.IsNullOrWhiteSpace(sceneName))
                {
                    if (_characterSaveInstanceHolder.CurrentSaveInstance.PathToSceneSaves
                        .TryGetValue(sceneName, out var savePath))
                    {
                        savePaths.Add(area, (sceneName, savePath));
                    }
                }
            }
            return savePaths;
        }
        /// <summary>
        /// Searches for an area stash's save data, and if found returns a new <see cref="StashSave"/> save container.
        /// </summary>
        /// <param name="area">The <see cref="AreaManager.AreaEnum"/> of the desired stashes save data.</param>
        /// <returns>If stash save data is found an <see cref="StashSave"/> container, otherwise null.</returns>
        public StashSave GetStashSave(AreaManager.AreaEnum area)
        {
            var permenantSaves = GetStashesSavePaths();
            if (!permenantSaves.TryGetValue(area, out var saveDetails))
            {
                Logger.LogDebug($"Could not find a save file path for area {area.GetName()}.");
                return null;
            }
            if (!_areaStashes.TryGetValue(area, out var areaStash))
            {
                Logger.LogDebug($"Area '{area.GetName()}' does not contain a Stash.");
                return null;
            }

            var envSave = GetEnvironmentSave(saveDetails.SceneName);
            Logger.LogTrace($"{nameof(StashSaveData)}::{nameof(GetStashSave)}: Got EnvironmentSave for area {area.GetName()} and character '{CharacterUID}'. Save Path: {saveDetails.SaveFilePath}");
            if (envSave != null)
            {
                var stashItems = envSave.ItemList.GetContainerItems(areaStash.StashUID, areaStash.ItemId);
                return new StashSave()
                {
                    CharacterUID = _characterSaveInstanceHolder.CharacterUID,
                    Area = area,
                    SceneName = saveDetails.SceneName,
                    UID = areaStash.StashUID,
                    ItemID = areaStash.ItemId,
                    SaveFilePath = saveDetails.SaveFilePath,
                    BasicSaveData = envSave.ItemList.GetItemSaveData(areaStash.StashUID),
                    ItemsSaveData = stashItems.ToList()
                };
            }
            return null;
        }
        /// <summary>
        /// Retrieves stash data for all areas.
        /// </summary>
        /// <param name="areasToExclude">Area save data to exclude.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="StashSave"/> instances for each area stash containing save data.</returns>
        public List<StashSave> GetStashSaves(IEnumerable<AreaManager.AreaEnum> areasToExclude = null)
        {
            var stashSaves = new List<StashSave>();
            var stashAreas = _areaStashes.Keys.Where(a => areasToExclude == null || !areasToExclude.Contains(a));
            foreach (var area in stashAreas)
            {
                var stashSave = GetStashSave(area);
                if (stashSave != null)
                    stashSaves.Add(stashSave);
            }
            return stashSaves.Any() ? stashSaves : null;
        }
    }
}
