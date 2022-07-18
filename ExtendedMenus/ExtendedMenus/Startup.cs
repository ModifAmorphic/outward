
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.ActionMenus.Settings;
using BepInEx;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Unity.ActionMenus;
using System.Linq;
using ModifAmorphic.Outward.GameObjectResources;
using ModifAmorphic.Outward.ActionMenus.Services;

namespace ModifAmorphic.Outward.ActionMenus
{
    internal class Startup
    {
        private ServicesProvider _services;
        private Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        internal void Start(ServicesProvider services)
        {
            _services = services;
            var settingsService = new SettingsService(services.GetService<BaseUnityPlugin>(), ModInfo.MinimumConfigVersion);
            var confSettings = settingsService.ConfigureSettings();

            services
                .AddSingleton(confSettings)
                .AddSingleton(settingsService.ConfigureHotbarSettings(confSettings))
                .AddFactory(() => LoggerFactory.GetLogger(ModInfo.ModId))
                .AddSingleton(new ModifCoroutine(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new LevelCoroutines(services.GetService<BaseUnityPlugin>(),
                                                  services.GetService<IModifLogger>))
                .AddSingleton(new ModifGoService(services.GetService<IModifLogger>));

            _loggerFactory = services.GetServiceFactory<IModifLogger>();

            var menuOverhaul = ConfigureAssetBundle();
            ////var hotbars = menuOverhaul.GetComponentInChildren<HotbarsMain>();

            services.AddSingleton(new PlayerMenuService(services.GetService<BaseUnityPlugin>(),
                                                  menuOverhaul,
                                                  (HotbarsContainer hotbars, CharacterUI characterUI) => new HotbarService(hotbars,
                                                                            characterUI,
                                                                            services.GetService<HotbarSettings>(),
                                                                            services.GetService<IModifLogger>),
                                                  services.GetService<ModifCoroutine>(),
                                                  services.GetService<ModifGoService>(),
                                                  services.GetService<HotbarSettings>(),
                                                  services.GetService<IModifLogger>));

            //services.AddSingleton(new HotbarsService(services.GetService<BaseUnityPlugin>(),
            //                                      menuOverhaul,
            //                                      services.GetService<ModifCoroutine>(),
            //                                      _services.GetService<ModifGoService>(),
            //                                      services.GetService<HotbarSettings>(),
            //                                      services.GetService<IModifLogger>));

            //var menuAssets = LoadAssetBundle(services.GetService<BaseUnityPlugin>().GetPluginDirectory(), "asset-bundles", "extended-menus");
            //var utilityMenu = menuAssets.LoadAsset<GameObject>("UtilityMenu");
            //Logger.LogDebug($"Loaded asset UtilityMenu. {utilityMenu?.GetPath()}");
            //var umgo = utilityMenu.transform.Find("Canvas/ExMenuButtons").gameObject;
            //Logger.LogDebug($"Found ExMenuButtons. {umgo.GetPath()}");
            //umgo.AddComponent<ExMenuEvents>();
            //GameObject.Instantiate(utilityMenu);
            //menuAssets.Unload(false);
            ////TODO: ExMenuEvents.Start() doesn't get called yet. Need a better way to hook into these events.
            ////SubscribeToMenuButtonClicks();

            //AttachScripts(utilityMenu, typeof(ActionSlot).Assembly);
        }
        public GameObject ConfigureAssetBundle()
        {
            AttachScripts(typeof(ActionSlot).Assembly);
            //var menuAssets = LoadAssetMenu(services.GetService<BaseUnityPlugin>().GetPluginDirectory());
            Logger.LogDebug($"Loading asset bundle action-menus-overhaul.");
            var menuBundle = LoadAssetBundle(_services.GetService<BaseUnityPlugin>().GetPluginDirectory(), "asset-bundles", "action-menus-overhaul");
            var allAssetNames = menuBundle.GetAllAssetNames();
            var logAssets = "";
            foreach (string name in allAssetNames)
            {
                logAssets += name + ", ";
            }
            Logger.LogDebug($"Assets in Bundle: " + logAssets);
            var menuOverhaulAsset = menuBundle.LoadAsset<GameObject>("assets/prefabs/menuoverhaul.prefab");
            menuOverhaulAsset.SetActive(false);
            Logger.LogDebug($"Loaded asset assets/prefabs/menuoverhaul.prefab.");
            UnityEngine.Object.DontDestroyOnLoad(menuOverhaulAsset);
            menuBundle.Unload(false);

            var modGo = _services.GetService<ModifGoService>()
                                 .GetModResources(ModInfo.ModId, false);
            menuOverhaulAsset.transform.SetParent(modGo.transform);
            return menuOverhaulAsset;
            
            //AttachScripts(menuOverhaulAsset, typeof(ActionSlot).Assembly);


            //var parent = GameObject.Find("PlayerUI/Canvas/GameplayPanels/HUD").transform;
            //menuOverhaulAsset.transform.SetParent(parent);
            return GameObject.Instantiate(menuOverhaulAsset);

            //TODO: ExMenuEvents.Start() doesn't get called yet. Need a better way to hook into these events.
            //SubscribeToMenuButtonClicks();
        }
        public void AttachScripts(Assembly sourceAssembly)
        {
            var scriptComponentTypes = sourceAssembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(UnityScriptComponentAttribute), true).Length > 0);
            var goSvc = _services.GetService<ModifGoService>();
            var modGo = goSvc.GetModResources(ModInfo.ModId, false);
            var scriptGo = new GameObject("scripts");
            scriptGo.transform.SetParent(modGo.transform);

            foreach (var t in scriptComponentTypes)
            {
                scriptGo.AddComponent(t);
            }
        }
        public void AttachScripts(GameObject target, Assembly sourceAssembly)
        {
            foreach (Type type in sourceAssembly.GetTypes())
            {
                var scriptAttribute = type.GetCustomAttribute<UnityScriptComponentAttribute>();
                if (scriptAttribute != null && type.IsClass)
                {
                    var component = target.transform.Find(scriptAttribute.ComponentPath)?.gameObject;
                    if (component != null)
                    {
                        component.AddComponent(type);
                    }
                }
            }
        }
        private AssetBundle LoadAssetBundle(string pluginPath, string bundleDirectory, string bundleFileName)
        {
            using (var assetStream = new FileStream
                    (
                        Path.Combine(pluginPath, Path.Combine(bundleDirectory, bundleFileName)
                    ), FileMode.Open, FileAccess.Read)
                  )
            {
                return AssetBundle.LoadFromStream(assetStream);
            }
        }
        public void SubscribeToMenuButtonClicks()
        {
            ExMenuEvents.Instance.OnClick += this.Instance_OnClick;
        }

        private void Instance_OnClick(UnityEngine.UI.Button menuButton)
        {
            Logger.LogInfo("Got a menu button click! Button name is " + menuButton.name);
        }

    }
}
