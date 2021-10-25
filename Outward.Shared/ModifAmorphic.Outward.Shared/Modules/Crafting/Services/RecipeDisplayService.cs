using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Crafting.Patches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Modules.Crafting.Services
{
    internal class RecipeDisplayService
    {
        private readonly ConcurrentDictionary<Type, RecipeSelectorDisplayConfig> _displayConfigs = new ConcurrentDictionary<Type, RecipeSelectorDisplayConfig>();

        private readonly ConcurrentDictionary<Type, List<StaticIngredient>> _staticIngredients = new ConcurrentDictionary<Type, List<StaticIngredient>>();
        

        private readonly Func<IModifLogger> _loggerFactory;
        private int _baseSelectorLength = 4;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public RecipeDisplayService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            //CraftingMenuPatches.GenerateResultOverride += TryGenerateResultOverride;
            CraftingMenuPatches.RefreshAutoRecipeAfter += HideRecipes;
        }

        public void AddUpdateDisplayConfig<T>(RecipeSelectorDisplayConfig recipeDisplayConfig) where T : CustomCraftingMenu
        {
            var config = _displayConfigs.AddOrUpdate(typeof(T), recipeDisplayConfig, (k, v) => v = recipeDisplayConfig);
        }
        //public bool TryRemoveDisplayConfig<T>() where T : CustomCraftingMenu =>_displayConfigs.TryRemove(typeof(T), out _);

        public bool TryGetDisplayConfig<T>(out RecipeSelectorDisplayConfig config) where T : CustomCraftingMenu =>
           _displayConfigs.TryGetValue(typeof(T), out config);
        public bool TryGetDisplayConfig(Type menuType, out RecipeSelectorDisplayConfig config) =>
           _displayConfigs.TryGetValue(menuType, out config);
        public bool TryGetStaticIngredients(Type menuType, out List<StaticIngredient> ingredients) =>
           _staticIngredients.TryGetValue(menuType, out ingredients);

        public void AddOrUpdateStaticIngredients<T>(IEnumerable<StaticIngredient> ingredients) => _staticIngredients.AddOrUpdate(typeof(T), ingredients.ToList(), (k , v) => v = ingredients.ToList());

        public bool TryRemoveStaticIngredients<T>() =>
           _staticIngredients.TryRemove(typeof(T), out _);

        public void ExtendIngredientSelectors(CustomCraftingMenu menu)
        {

            if (!TryGetDisplayConfig(menu.GetType(), out var config) || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.None)
                return;

            var extras = config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both ? 2 : 1;

            Logger.LogDebug($"RecipeDisplayService::ExtendIngredientSelectors(): Extending menu {menu.GetType()} ingredient slots by {extras} selectors.");

            //Expand last free ingredients otherwise will throw out of range
            var freeIngredientIDs = menu._lastFreeRecipeIngredientIDs;
            Array.Resize(ref freeIngredientIDs, freeIngredientIDs.Length + extras);
            menu._lastFreeRecipeIngredientIDs = freeIngredientIDs;
            menu.resetFreeRecipeLastIngredients();

            var template = menu._ingredientSelectorTemplate;
            var slots = menu._ingredientSelectors;
            //get the craft button for nav reasons
            var craftButton = menu._recipeResultDisplay?.GetComponent<Button>();

            //get the "next" ingredient selector
            var index = slots.Length;
            //resize the ingredient selectors
            Array.Resize(ref slots, slots.Length + extras);

            //Create the rect and horizontal layout group for the new bottom selectors
            var extraIngredsRect = CreateBottomIngredientRect(menu);
            if (config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.BottomLeft || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both)
            {
                slots[index] = GetIngredientSelector(menu, template, extraIngredsRect, index);
                index++;
            }

            if (config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.BottomRight || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both)
            {
                slots[index] = GetIngredientSelector(menu, template, extraIngredsRect, index);
            }

            SetSelectorNavigation(config.ExtraIngredientSlotOption, craftButton, slots);

            menu._ingredientSelectors = slots;
        }
        internal void SetSelectorNavigation(ExtraIngredientSlotOptions extraIngredientSlotOptions, Button craftButton, IngredientSelector[] selectors)
        {
            if (craftButton != null)
            {
                var craftNav = craftButton.navigation;
                craftNav.selectOnLeft = null;
                craftNav.selectOnRight = null;
                craftNav.selectOnUp = selectors[0].Button;
                craftButton.navigation = craftNav;
            }

            for (int i = 0; i < _baseSelectorLength; i++)
            {
                //var leftSelector = i > 0 ? selectors[i - 1] : null;
                var selector = selectors[i];
                var leftSelector = i > 0 ? selectors[i - 1] : null;

                var nav = selector.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnLeft = selector.Button.interactable ? leftSelector?.Button : null;
                nav.selectOnDown = selector.Button.interactable ? craftButton : null;
                selector.navigation = nav;

                if (leftSelector != null)
                {
                    var leftNav = leftSelector.navigation;
                    leftNav.selectOnRight = selector.Button.interactable ? selector.Button : null;
                    leftSelector.navigation = leftNav;
                }
            }

            var bottomIndex = _baseSelectorLength;
            if (extraIngredientSlotOptions == ExtraIngredientSlotOptions.BottomLeft || extraIngredientSlotOptions == ExtraIngredientSlotOptions.Both)
            {
                SetBottomLeftNav(bottomIndex, craftButton, extraIngredientSlotOptions, selectors);
                bottomIndex++;
            }
            if (extraIngredientSlotOptions == ExtraIngredientSlotOptions.BottomRight || extraIngredientSlotOptions == ExtraIngredientSlotOptions.Both)
            {
                SetBottomRightNav(bottomIndex, craftButton, extraIngredientSlotOptions, selectors);
            }
        }
        private RectTransform CreateBottomIngredientRect(CustomCraftingMenu menu)
        {
            var content = menu.transform.Find("Content");
            var top = menu.transform.Find("Content/Ingredients").GetComponent<RectTransform>();
            var result = menu.transform.Find("Content/CraftingResult").GetComponent<RectTransform>();

            var bottomGo = new GameObject("ExtraIngredients");
            bottomGo.transform.SetParent(content);
            var bottom = bottomGo.AddComponent<RectTransform>();
            bottom.pivot = new Vector2(result.pivot.x, result.pivot.y);
            bottom.localScale = top.localScale;
            bottom.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, top.rect.width);
            bottom.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, top.rect.height);
            bottom.position = new Vector3(top.position.x, result.position.y);

            return bottom;
        }
        private IngredientSelector GetIngredientSelector(CustomCraftingMenu menu, IngredientSelector selectorTemplate, Transform parent, int index)
        {
            var selector = UnityEngine.Object.Instantiate(selectorTemplate);
            selector.transform.SetParent(parent);
            selector.transform.ResetLocal();
            selector.SetParentCraftingMenu(menu, index);

            ((UnityEvent)(object)selector.onClick).AddListener(delegate
            {
                menu.OnIngredientSelectorClicked(index);
            });
            return selector;
        }
        private void SetBottomLeftNav(int index, Button craftButton, ExtraIngredientSlotOptions slotOptions, IngredientSelector[] slots)
        {
            if (!slots[index].Button.interactable)
            {
                Logger.LogDebug($"SetBottomLeftNav: Index slot {index} button was not interactable.");
                return;
            }
            Logger.LogDebug($"SetBottomLeftNav: Index slot {index} button was interactable.");
            var navigation = slots[index].navigation;
            var bottomRightSel = slotOptions == ExtraIngredientSlotOptions.Both ? slots.Last() : null;
            
            navigation.mode = Navigation.Mode.Explicit;
            
            var leftIndex = Array.FindLastIndex(slots, _baseSelectorLength - 1, _baseSelectorLength, s => s.Button.interactable);
            
            navigation.selectOnLeft = null;
            navigation.selectOnRight = craftButton;
            navigation.selectOnDown = null;
            
            if (craftButton != null)
            {
                var craftNavigation = craftButton.navigation;
                craftNavigation.selectOnLeft = slots[index].Button.interactable ? slots[index].Button : null;
                craftButton.navigation = craftNavigation;
            }
            
            var navDownIndex = slotOptions == ExtraIngredientSlotOptions.BottomLeft ? 4 : 2;
            //change top slots down navigation to go to this slot. If there is a Bottom right slot enabled,
            //then only the first 2 should go to this slot. Otherwise, down takes all 4 here.
            bool upSet = false;
            for (var top = 0; top < navDownIndex; top++)
            {
                if (slots[top].Button.interactable)
                {
                    var topNavigation = slots[top].navigation;
                    topNavigation.selectOnDown = slots[index].Button.interactable ? slots[index].Button : null;
                    slots[top].navigation = topNavigation;
                    if (!upSet && slots[index].Button.interactable)
                    {
                        upSet = true;
                        navigation.selectOnUp = slots[top].Button;
                    }
                }
            }

            slots[index].navigation = navigation;
        }
        private void SetBottomRightNav(int index, Button craftButton, ExtraIngredientSlotOptions slotOptions, IngredientSelector[] slots)
        {
            if (!slots[index].Button.interactable)
            {
                Logger.LogDebug($"SetBottomRightNav: Index slot {index} button was not interactable.");
                return;
            }
            Logger.LogDebug($"SetBottomRightNav: Index slot {index} button was interactable.");
            var navigation = slots[index].navigation;
            navigation.mode = Navigation.Mode.Explicit;
            //Get the closest active button on the right
            var leftSelector = slotOptions == ExtraIngredientSlotOptions.Both ? slots[4] : null;

            navigation.selectOnDown = null;
            navigation.selectOnRight = null;

            if (craftButton != null)
            {
                navigation.selectOnLeft = craftButton;
                var craftNavigation = craftButton.navigation;
                craftNavigation.selectOnRight = slots[index].Button.interactable ? slots[index].Button : null;
                craftButton.navigation = craftNavigation;
            }
            else if (leftSelector != null)
            {
                navigation.selectOnLeft = leftSelector.Button;
                var leftNavigation = leftSelector.navigation;
                leftNavigation.selectOnRight = slots[index].Button.interactable ? slots[index].Button : null;
                leftSelector.navigation = leftNavigation;
            }
            
            //change top slots down navigation to go to this slot. If there is a Bottom left slot enabled,
            //then only the last 2 should go to this slot. Otherwise, down takes all 4 here.
            var navDownIndex = slotOptions == ExtraIngredientSlotOptions.BottomRight ? 0 : 2;
            for (var top = navDownIndex; top < _baseSelectorLength; top++)
            {
                if (slots[top].Button.interactable)
                {
                    var topNavigation = slots[top].navigation;
                    topNavigation.selectOnDown = slots[index].Button.interactable ? slots[index].Button : null;
                    slots[top].navigation = topNavigation;
                    
                }
            }
            //set the up to the last active on the top row
            for (var i = 0; i < _baseSelectorLength; i++)
                if (slots[i].Button.interactable)
                    navigation.selectOnUp = slots[i].Button;

            slots[index].navigation = navigation;
        }

        public void AddStaticIngredients(CustomCraftingMenu menu, ref List<Recipe> recipes)
        {
            if (TryGetStaticIngredients(menu.GetType(), out var staticIngredients))
            {
                if (staticIngredients != null && staticIngredients.Count > 0)
                {
                    var specificItems = staticIngredients.Where(i => i.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient);
                    var prefabs = new Item[specificItems.Count()];

                    int pid = 0;
                    foreach (var s in specificItems)
                    {
                        prefabs[pid] = ResourcesPrefabManager.Instance.GetItemPrefab(s.SpecificItemID);
                        pid++;
                    }
                    var modRecipes = new List<Recipe>();
                    foreach (var recipe in recipes)
                    {
                        var modRecipe = UnityEngine.Object.Instantiate(recipe);
                        int prefabIndex = 0;
                        foreach (var staticIngredient in staticIngredients)
                        {
                            if (staticIngredient.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient)
                            {
                                modRecipe.AddCustomIngredient(staticIngredient.IngredientID, prefabs[prefabIndex]);
                                prefabIndex++;
                            }
                            else
                            {
                                modRecipe.AddCustomIngredient(staticIngredient.IngredientID, staticIngredient.IngredientTypeSelector);
                            }
                        }
                        modRecipes.Add(modRecipe);
                    }
                    recipes = modRecipes;
                }
            }
        }
        internal void PositionSelectors(CraftingMenu menu, IngredientSelector[] ingredientSelectors)
        {
            if (!TryGetDisplayConfig(menu.GetType(), out var config) || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.None)
                return;
            
            var result = menu.transform.Find("Content/CraftingResult/ItemDisplayGrid").GetComponent<RectTransform>();
            var index = 4;
            if (config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.BottomLeft)
            {
                SetSlotPosition(ingredientSelectors[0].gameObject.transform, result, ingredientSelectors[index].gameObject.transform);
                index++;
            }
            if (config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.BottomRight)
            {
                SetSlotPosition(ingredientSelectors[3].gameObject.transform, result, ingredientSelectors[index].gameObject.transform);
            }
        }
        private void SetSlotPosition(Transform xRef, RectTransform yRef, Transform target)
        {
            var rectTarget = target.GetComponent<RectTransform>();
            var rectXref = xRef.GetComponent<RectTransform>();

            rectTarget.pivot = new Vector2(yRef.pivot.x, yRef.pivot.y);
            rectTarget.position = new Vector3(rectXref.position.x, yRef.position.y);
            Logger.LogDebug($"{nameof(RecipeDisplayService)}::{nameof(SetSlotPosition)}: Source (X, Y) ({rectXref}: {rectXref.position.x}, {yRef}: {yRef.position.y}). Target (X, Y) ({rectTarget.position.x}, {rectTarget.position.y})");
        }

        private void HideRecipes(CustomCraftingMenu menu)
        {
            var menuType = menu.GetType();
            if (!TryGetDisplayConfig(menuType, out var config) || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.None
                || !TryGetStaticIngredients(menuType, out var staticIngredients))
                return;

            var ignoredAmt = staticIngredients.Count(s => !s.CountAsIngredient);
            if (ignoredAmt < 1)
                return;

            var complexeRecipes = menu.GetPrivateField<CraftingMenu, List<KeyValuePair<int, Recipe>>>("m_complexeRecipes");
            if (complexeRecipes.Count < 1)
                return;

            //verify statics have actually been added
            if (!complexeRecipes[0].Value.Ingredients.Any(r => r is CustomRecipeIngredient cri && cri.CustomRecipeIngredientID != Guid.Empty))
                return;

            var recipeDisplays = menu.GetPrivateField<CraftingMenu, List<RecipeDisplay>>("m_recipeDisplays");

            var trimComplexRecipes = new List<KeyValuePair<int, Recipe>>();
            var trimRecipeDisplays = new List<RecipeDisplay>();

            int index = 0;
            for (int i = 0; i < complexeRecipes.Count; i++)
            {
                
                if (complexeRecipes[i].Value.IngredientCount - ignoredAmt > 1)
                {
                    
                    trimComplexRecipes.Add(new KeyValuePair<int, Recipe>(complexeRecipes[i].Key, complexeRecipes[i].Value));
                    var display = recipeDisplays[i];
                    display.onClick.RemoveAllListeners();
                    int selectIndex = index;
                    display.onClick.AddListener(delegate
                     {
                         menu.OnRecipeSelected(selectIndex);
                     });
                    trimRecipeDisplays.Add(display);
                    index++;
                }
                else
                {
                    Logger.LogDebug($"HideRecipes: Removed Recipe index:{i}, {complexeRecipes[i].Value.Name}. Destroying " +
                        $"display. Recipe had {complexeRecipes[i].Value.IngredientCount} ingredients, but {ignoredAmt} were ignored. ");
                    recipeDisplays[i].transform.SetParent(null);
                    recipeDisplays[i].Hide();
                    recipeDisplays[i].gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(recipeDisplays[i]);
                }
            }
            for (int i = complexeRecipes.Count; i < recipeDisplays.Count; i++)
            {
                trimRecipeDisplays.Add(recipeDisplays[i]);
            }

            menu.SetPrivateField<CraftingMenu, List<KeyValuePair<int, Recipe>>>("m_complexeRecipes", trimComplexRecipes);
            menu.SetPrivateField<CraftingMenu, List<RecipeDisplay>>("m_recipeDisplays", trimRecipeDisplays);

            Logger.LogDebug($"HideRecipes: m_complexeRecipes trimmed from {complexeRecipes.Count} to {menu.GetPrivateField<CraftingMenu, List<KeyValuePair<int, Recipe>>>("m_complexeRecipes").Count}." +
                $" m_recipeDisplays trimmed from {complexeRecipes.Count} to {menu.GetPrivateField<CraftingMenu, List<RecipeDisplay>>("m_recipeDisplays").Count}.");
        }
    }
}
