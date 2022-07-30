using BepInEx;
using ModifAmorphic.Outward.ActionMenus.Patches;
using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Plugin.Services
{
    internal class RewiredListener
    {
        private readonly ConfigSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly BaseUnityPlugin _baseUnityPlugin;

        private readonly LevelCoroutines _coroutine;

        private InputMapCategory menuMapCategory;
        private InputMapCategory slotsMapCategory;

        private readonly List<InputAction> _inputAction = new List<InputAction>();

        private InputMapCategory mapCategory;

        public RewiredListener(BaseUnityPlugin baseUnityPlugin,
                                LevelCoroutines coroutine,
                                ConfigSettings settings, Func<IModifLogger> getLogger)
        {
            (_baseUnityPlugin, _coroutine, _getLogger) = (baseUnityPlugin, coroutine, getLogger);
            //MapHelperConstructorPatch.OnNewMapHelper += AddDefaultMaps;
            //PlayerConstructorPatch.OnNewPlayer += PlayerConstructorPatch_OnNewPlayer;
            InputManager_BasePatches.BeforeInitialize += InitializeActonMenus;
            //ReInput.InitializedEvent += LoadDefaultControllerMaps;
            //NetworkLevelLoaderPatches.MidLoadLevelAfter += (NetworkLevelLoader levelLoader) => 
            //    coroutine.InvokeAfterLevelAndPlayersLoaded(levelLoader, LoadDefaultControllerMaps, 300);

            //InputManager_BasePatches.AfterInitialize += LoadDefaultControllerMaps;
        }

        private void InitializeActonMenus(InputManager_Base inputManager)
        {
            //RewiredConstants.WakeMe = true;
            var actionCategoryMap = inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap");

            inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Add(RewiredConstants.ActionMenus.CategoryMap);
            inputManager.userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(RewiredConstants.ActionMenus.ActionCategory);
            actionCategoryMap.AddCategory(RewiredConstants.ActionMenus.ActionCategory.id);

            inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Add(RewiredConstants.ActionSlots.CategoryMap);
            inputManager.userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(RewiredConstants.ActionSlots.ActionCategory);
            inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddCategory(RewiredConstants.ActionSlots.ActionCategory.id);

            var actions = inputManager.userData.GetPrivateField<UserData, List<InputAction>>("actions");

            foreach (var action in RewiredConstants.ActionSlots.Actions)
            {
                Logger.LogDebug($"Adding action {action.name} with id {action.id}");
                inputManager.userData.GetPrivateField<UserData, List<InputAction>>("actions").Add(action);
                inputManager.userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddAction(action.categoryId, action.id);
            }

        }

        //private InputMapCategory AddMapCategory(InputManager_Base inputManager, RewiredConfig rewiredConfig)
        //{
        //    inputManager.userData.AddMapCategory();
        //    var mapCategory = new InputMapCategory();
        //    //var mapCategory = inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Last();
        //    mapCategory.SetPrivateField("_name", rewiredConfig.Name);
        //    mapCategory.SetPrivateField("_descriptiveName", rewiredConfig.DescriptiveName);
        //    mapCategory.SetPrivateField("_id", rewiredConfig.Id);
        //    mapCategory.SetPrivateField("_userAssignable", rewiredConfig.Id);
        //    inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Add(mapCategory);

        //    return mapCategory;
        //}
        //private InputMapCategory AddActionCategory(InputManager_Base inputManager, RewiredConfig rewiredConfig)
        //{
        //    int categoryId = AddActionCategory(inputManager.userData, "ActionSlots");
        //    var mapCategory = inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Last();
        //    mapCategory.SetPrivateField("_name", rewiredConfig.Name);
        //    mapCategory.SetPrivateField("_descriptiveName", rewiredConfig.DescriptiveName);
        //    mapCategory.SetPrivateField("_id", rewiredConfig.Id);

        //    return mapCategory;
        //}
        //private void InputManager_BasePatches_AfterInitialize(InputManager_Base inputManager)
        //{
        //    var savepath = _baseUnityPlugin.GetPluginDirectory();
        //    savepath = Path.Combine(savepath, "xml");
        //    var allPlayers = Rewired.ReInput.players.AllPlayers;

        //    inputManager.userData.AddMapCategory();
        //    mapCategory = inputManager.userData.GetPrivateField<UserData, List<InputMapCategory>>("mapCategories").Last();
        //    mapCategory.SetPrivateField("_name", "ActionMenus");
        //    mapCategory.SetPrivateField("_descriptiveName", "ActionMenus");


        //    var keyboardMap = new KeyboardMap();


        //    for (int i = 0;i < _inputAction.Count;i++)
        //    {

        //    }

        //    foreach (var player in allPlayers)
        //    {
        //        var playerSaveData = player.GetSaveData(true);

        //        //Controller controller = parent.Controllers.First(c => c.type == ControllerType.Keyboard);
        //        //var map = new ControllerMap();
        //        Logger.LogDebug($"{nameof(RewiredListener)}::{nameof(AddDefaultMaps)}(): Dumping default maps to xml from PlayerSaveData. Found {playerSaveData.AllControllerMapSaveData?.Count()} maps.");
        //        foreach (ControllerMapSaveData saveData in playerSaveData.AllControllerMapSaveData)
        //        {
        //            var xml = saveData.map.ToXmlString();
        //            File.WriteAllText(Path.Combine(savepath, $"Player_{player.id}-Map_{saveData.map.id}.xml"), xml);
        //        }
        //    }
        //}

        //private void PlayerConstructorPatch_OnNewPlayer(Player player)
        //{
        //    var savepath = _baseUnityPlugin.GetPluginDirectory();
        //    savepath = Path.Combine(savepath, "xml");
        //    var playerSaveData = player.GetSaveData(true);

        //    //Controller controller = parent.Controllers.First(c => c.type == ControllerType.Keyboard);
        //    //var map = new ControllerMap();
        //    Logger.LogDebug($"{nameof(RewiredListener)}::{nameof(AddDefaultMaps)}(): Dumping default maps to xml from PlayerSaveData. Found {playerSaveData.AllControllerMapSaveData?.Count()} maps.");
        //    foreach (ControllerMapSaveData saveData in playerSaveData.AllControllerMapSaveData)
        //    {
        //        var xml = saveData.map.ToXmlString();
        //        File.WriteAllText(Path.Combine(savepath, saveData.map.name + ".xml"), xml);
        //    }
        //}

        //private void AddDefaultMaps(Rewired.Player.ControllerHelper.MapHelper mapHelper, Rewired.Player player, Rewired.Player.ControllerHelper parent)
        //{
        //    var savepath = _baseUnityPlugin.GetPluginDirectory();
        //    savepath = Path.Combine(savepath, "xml");
        //    var playerSaveData = player.GetSaveData(true);

        //    //Controller controller = parent.Controllers.First(c => c.type == ControllerType.Keyboard);
        //    //var map = new ControllerMap();
        //    Logger.LogDebug($"{nameof(RewiredListener)}::{nameof(AddDefaultMaps)}(): Dumping default maps to xml from PlayerSaveData. Found {playerSaveData.AllControllerMapSaveData?.Count()} maps.");
        //    foreach (ControllerMapSaveData saveData in playerSaveData.AllControllerMapSaveData)
        //    {
        //        var xml = saveData.map.ToXmlString();
        //        File.WriteAllText(Path.Combine(savepath, saveData.map.name + ".xml"), xml);
        //    }
        //        //mapHelper.AddMap(controller, ControllerMap.CreateFromXml();
        //}


        //private static int AddActionCategory(UserData userData, string categoryName)
        //{
        //    var newCategory = userData.InvokePrivateMethod<UserData, InputCategory>("ktrkfyIlcFgnRMWiroMCIOVWfQua");
        //    newCategory.SetPrivateField("_name", categoryName);
        //    newCategory.SetPrivateField("_descriptiveName", categoryName);

        //    userData.GetPrivateField<UserData, List<InputCategory>>("actionCategories").Add(newCategory);
        //    userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddCategory(newCategory.id);

        //    return newCategory.id;

        //}
        //private static InputAction GetAction(int slotId, int categoryId, UserData userData)
        //{
        //    var inputAction = userData.InvokePrivateMethod<UserData, InputAction>("PKUyIcoQeLkuCTdhUnVwqtkIVLm");

        //    inputAction.SetPrivateField("_name", "ActionSlot" + slotId);
        //    inputAction.SetPrivateField("_descriptiveName", "ActionSlot" + slotId);
        //    inputAction.SetPrivateField("_type", InputActionType.Button);
        //    inputAction.SetPrivateField("_userAssignable", true);
        //    inputAction.SetPrivateField("_categoryId", categoryId);

        //    userData.GetPrivateField<UserData, List<InputAction>>("actions").Add(inputAction);
        //    userData.GetPrivateField<UserData, ActionCategoryMap>("actionCategoryMap").AddAction(categoryId, inputAction.id);

        //    return inputAction;
        //}


    }
}
