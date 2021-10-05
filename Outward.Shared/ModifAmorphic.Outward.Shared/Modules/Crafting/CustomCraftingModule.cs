using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Models;
using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
using ModifAmorphic.Outward.Modules.Crafting.Patches;
using ModifAmorphic.Outward.Modules.Crafting.Services;
using ModifAmorphic.Outward.Patches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Crafting
{
    public class CustomCraftingModule : IModifModule
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        private readonly CraftingMenuService _menuTabService;
        private readonly CustomRecipeService _customRecipeService;
        private readonly CustomCraftingService _craftingService;

        private readonly ConcurrentStack<CraftingMenuMetadata> _menusQueue = 
            new ConcurrentStack<CraftingMenuMetadata>();

        private readonly ConcurrentDictionary<Type, CraftingMenuMetadata> _craftingMenus =
           new ConcurrentDictionary<Type, CraftingMenuMetadata>();

        private readonly ConcurrentDictionary<Type, Dictionary<string, RecipeMetadata>> _customRecipes =
           new ConcurrentDictionary<Type, Dictionary<string, RecipeMetadata>>();

        private readonly ConcurrentDictionary<Type, Recipe.CraftingType> _craftingStationTypes = 
            new ConcurrentDictionary<Type, Recipe.CraftingType>();

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(CharacterUIPatches),
            typeof(CraftingMenuPatches),
            typeof(CompatibleIngredientPatches),
            typeof(LocalizationManagerPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(CharacterUIPatches),
            typeof(CraftingMenuPatches),
            typeof(LocalizationManagerPatches)
        };

        internal CustomCraftingModule(CraftingMenuService menuTabService, CustomRecipeService customRecipeService, CustomCraftingService craftingService, Func<IModifLogger> loggerFactory)
        {
            (_menuTabService, _customRecipeService, _craftingService, _loggerFactory) = (menuTabService, customRecipeService, craftingService, loggerFactory);
            CharacterUIPatches.AwakeBefore += CharacterUIPatches_AwakeBefore;
            CraftingMenuPatches.AwakeInitAfter += CraftingMenuPatches_AwakeInitAfter;
        }

        private void CraftingMenuPatches_AwakeInitAfter(CraftingMenu craftingMenu)
        {
            //Needed to avoid infinite recursion
            if (craftingMenu is CustomCraftingMenu)
                return;

            AddCustomCraftingMenus(craftingMenu);
        }

        private void CharacterUIPatches_AwakeBefore(CharacterUI characterUI)
        {
            AddCraftingTabAndFooter(characterUI);
        }

        /// <summary>
        /// Registers a new crafting menu to be added when the base CharacterUI menus are normally added. This should be called sometime during a plugin's Awake to insure it gets added.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="CustomCraftingMenu"/> to register.</typeparam>
        /// <param name="craftingMenu"></param>
        /// <param name="MenuDisplayName"></param>
        public Recipe.CraftingType RegisterCraftingMenu<T>(string MenuDisplayName) where T : CustomCraftingMenu
        {
            if (_craftingMenus.ContainsKey(typeof(T)))
                throw new ArgumentException($"{nameof(CustomCraftingMenu)} of type {typeof(T)} already exists. " +
                    $"Only one instance of a type derived from {nameof(CustomCraftingMenu)} can be added.", nameof(T));

            //var orderNo = _craftingMenus.Count;
            _craftingMenus.TryAdd(typeof(T),
                new CraftingMenuMetadata()
                {
                    MenuType = typeof(T),
                    TabButtonName = "btn" + typeof(T).Name,
                    TabName = "PlayerMenu_Tab_" + typeof(T).Name,
                    TabDisplayName = MenuDisplayName,
                    MenuName = typeof(T).Name,
                    FooterName = typeof(T).Name + "Footer",
                    //TabOrderNo = orderNo
                }) ;

            _menusQueue.Push(_craftingMenus[typeof(T)]);

            _craftingStationTypes.TryAdd(typeof(T), 
                (Recipe.CraftingType)_menuTabService.AddIngredientTag());

            TryAddRecipes();

            return _craftingStationTypes[typeof(T)];

        }
        /// <summary>
        /// Registers a new recipe to be added once the <see cref="Recipe.CraftingType" /> is known for the 
        /// <see cref="CustomCraftingMenu"/> type of <typeparamref name="T"/>. If the <see cref="Recipe.CraftingType" />
        /// is already known, then the recipe will be added immedately.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="CustomCraftingMenu"/> to register the new <paramref name="recipe"/> for.</typeparam>
        /// <param name="recipe">The new <see cref="Recipe"/></param>
        public void RegisterRecipe<T>(Recipe recipe) where T : CustomCraftingMenu
        {
            var recipes = _customRecipes.GetOrAdd(typeof(T), new Dictionary<string, RecipeMetadata>());
            if (recipes.ContainsKey(recipe.UID.ToString()))
                throw new ArgumentException($"A Recipe with UID '{recipe.UID}' is already registered for crafting station type {typeof(T).Name}."
                    , nameof(recipe));

            recipes.Add(recipe.UID.ToString(),
                new RecipeMetadata()
                {
                   CraftingStationType = typeof(T),
                   Recipe = recipe
                });
            TryAddRecipes();
        }
        public void RegisterCustomCrafter<T>(ICustomCrafter crafter)  where T : CustomCraftingMenu => _craftingService.AddOrUpdateCrafter<T>(crafter);
        public void RegisterCompatibleIngredientMatcher<T>(ICompatibleIngredientMatcher matcher) where T : CustomCraftingMenu 
            => _craftingService.AddOrUpdateCompatibleIngredientMatcher<T>(matcher);
        private void TryAddRecipes()
        {
            //get only recipes for crafting stations where the Custom CraftingType is known - basically, the addition of the custom crafting menu is
            //far enough along to where the CraftingType has been determined.
            var stations = _customRecipes.Where(rt => _craftingStationTypes.ContainsKey(rt.Key));
            foreach (var stationRecipes in stations)
            {
                var recipes = stationRecipes.Value
                    .Where(kvp => kvp.Value.CustomCraftingType == default)
                    .Select(kvp =>
                    {
                        //Set this value to the type to mark it as added.
                        kvp.Value.CustomCraftingType = _craftingStationTypes[stationRecipes.Key];
                        //set the actual recipe value as well.
                        kvp.Value.Recipe.SetCraftingType(_craftingStationTypes[stationRecipes.Key]);
                        return kvp.Value.Recipe;
                    });
                _customRecipeService.AddRecipes(recipes);
            }
        }
        private void AddCraftingTabAndFooter(CharacterUI characterUI)
        {
            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");

            var screenNo = menuTypes.Length;
            Array.Resize(ref menuTypes, menuTypes.Length + _craftingMenus.Count);

            //Crafting menus get inserted right after the base crafting menu, so add the last added menu first.
            //this way the order the menus were registered in is preserved.
            while (_menusQueue.TryPop(out var menu))
            {
                menu.MenuScreenNo = screenNo++;

                menuTypes[menu.MenuScreenNo] = menu.MenuType;
                menu.MenuTab = _menuTabService.AddMenuTab(characterUI, menu.TabName, menu.TabDisplayName, menu.TabButtonName, menu.MenuScreenNo, menu.OrderAfterTab);
                menu.MenuFooter = _menuTabService.AddFooter(characterUI, menu.MenuScreenNo, menu.FooterName);
            }
            characterUI.SetPrivateField("MenuTypes", menuTypes);

            var m_menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            if (m_menus != null && m_menus.Length < menuTypes.Length)
                Array.Resize(ref m_menus, menuTypes.Length);
            else
                m_menus = new MenuPanel[menuTypes.Length];

            characterUI.SetPrivateField("m_menus", m_menus);
        }
        private void AddCraftingTabFooter(CharacterUI characterUI)
        {
            var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");
            
            var screenNo = menuTypes.Length;
            Array.Resize(ref menuTypes, menuTypes.Length + _craftingMenus.Count);

            //Crafting menus get inserted right after the base crafting menu, so add the last added menu first.
            //this way the order the menus were registered in is preserved.
            foreach (var kvp in _craftingMenus)
            {
                (Type menuType, CraftingMenuMetadata menu) = (kvp.Key, kvp.Value);
                
                menu.MenuScreenNo = screenNo++;

                menuTypes[menu.MenuScreenNo] = menuType;
                menu.MenuTab =  _menuTabService.AddMenuTab(characterUI, menu.TabName, menu.TabDisplayName, menu.TabButtonName, menu.MenuScreenNo, menu.OrderAfterTab);
                menu.MenuFooter = _menuTabService.AddFooter(characterUI, menu.MenuScreenNo, menu.FooterName);
            }
            characterUI.SetPrivateField("MenuTypes", menuTypes);

            var m_menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
            if (m_menus != null && m_menus.Length < menuTypes.Length)
                Array.Resize(ref m_menus, menuTypes.Length);
            else
                m_menus = new MenuPanel[menuTypes.Length];

            characterUI.SetPrivateField("m_menus", m_menus);
        }
        private void AddCustomCraftingMenus(CraftingMenu baseCraftingMenu)
        {
            foreach (var kvp in _craftingMenus)
            {
                (Type menuType, CraftingMenuMetadata meta) = (kvp.Key, kvp.Value);
                meta.MenuDisplay = _menuTabService.AddCustomMenu(baseCraftingMenu, meta);
                
                //backfill the Crafting Station type on the newly created CustomCraftingMenu instance if it's not a permanent crafting station
                var customMenu = (CustomCraftingMenu)meta.MenuDisplay.GetComponent(menuType);
                if (customMenu.PermanentCraftingStationType == null)
                {
                    customMenu.CustomCraftingType = _craftingStationTypes[menuType];
                    Logger.LogDebug($"{nameof(CustomCraftingModule)}::{nameof(AddCustomCraftingMenus)}(): " +
                        $"Set {nameof(CustomCraftingMenu.CustomCraftingType)} to {_craftingStationTypes[menuType]} for" +
                        $"type {menuType}");
                }
                // Avoids key not found exceptions if there is a custom crafting station with no actual recipes.
                if (customMenu.IsCustomCraftingStation())
                    _customRecipeService.AddOrGetCraftingStationRecipes(customMenu.GetRecipeCraftingType());
            }
        }
        /// <summary>
        /// Unregisters a crafting from the CharacterUI menu.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="CustomCraftingMenu"/> to register.</typeparam>
        [Obsolete]
        public void UnregisterCraftingMenu<T>() where T : CustomCraftingMenu
        {
            //TODO: This fails spectacularily. Revist or remove.
            throw new NotImplementedException();

            if (!_craftingMenus.ContainsKey(typeof(T)))
                throw new ArgumentException($"{nameof(CustomCraftingMenu)} of type {typeof(T)} does not exist. " +
                    $"{nameof(CustomCraftingMenu)} can not be removed.", nameof(T));

            if (!_craftingMenus.TryRemove(typeof(T), out var menuMetaData))
                return;

            foreach (var player in SplitScreenManager.Instance.LocalPlayers)
            {
                var characterUI = player.AssignedCharacter.CharacterUI;
                var menuTabs = characterUI.GetPrivateField<CharacterUI, MenuTab[]>("m_menuTabs");
                var menuTypes = characterUI.GetPrivateField<CharacterUI, Type[]>("MenuTypes");
                var m_menus = characterUI.GetPrivateField<CharacterUI, MenuPanel[]>("m_menus");
                
                var removeMenu = Array.FindIndex(m_menus, m => m is T);
                m_menus[removeMenu] = null;
                menuTypes[menuMetaData.MenuScreenNo] = null;

                menuTabs = menuTabs.Where(t => t.TabName != menuMetaData.TabName).ToArray();

                characterUI.SetPrivateField<CharacterUI, MenuTab[]>("m_menuTabs", menuTabs);

                UnityEngine.Object.Destroy(menuMetaData.MenuFooter);
                UnityEngine.Object.Destroy(menuMetaData.MenuDisplay);
                UnityEngine.Object.Destroy(menuMetaData.MenuTab);
            }

        }
    }
}
