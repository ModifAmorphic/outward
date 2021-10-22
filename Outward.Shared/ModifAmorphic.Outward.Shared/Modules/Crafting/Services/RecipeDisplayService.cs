using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
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

        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        public RecipeDisplayService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            //CraftingMenuPatches.GenerateResultOverride += TryGenerateResultOverride;

        }
        public void AddUpdateDisplayConfig<T>(RecipeSelectorDisplayConfig recipeDisplayConfig) where T : CustomCraftingMenu
        {
            var config = _displayConfigs.AddOrUpdate(typeof(T), recipeDisplayConfig, (k, v) => v = recipeDisplayConfig);
        }
        public bool TryRemoveDisplayConfig<T>() where T : CustomCraftingMenu =>_displayConfigs.TryRemove(typeof(T), out _);

        public bool TryGetDisplayConfig<T>(out RecipeSelectorDisplayConfig config) where T : CustomCraftingMenu =>
           _displayConfigs.TryGetValue(typeof(T), out config);
        public bool TryGetDisplayConfig(Type menuType, out RecipeSelectorDisplayConfig config) =>
           _displayConfigs.TryGetValue(menuType, out config);


        public void ExtendIngredientSelectors(CustomCraftingMenu menu)
        {

            if (!TryGetDisplayConfig(menu.GetType(), out var config) || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.None)
                return;

            var extras = config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both ? 2 : 1;

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
                SetBottomLeftNav(slots, index, craftButton, config.ExtraIngredientSlotOption);
                index++;
            }

            if (config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.BottomRight || config.ExtraIngredientSlotOption == ExtraIngredientSlotOptions.Both)
            {
                slots[index] = GetIngredientSelector(menu, template, extraIngredsRect, index);
                SetBottomRightNav(slots, index, craftButton, config.ExtraIngredientSlotOption);
            }

            menu._ingredientSelectors = slots;
        }
        private RectTransform CreateBottomIngredientRect(CustomCraftingMenu menu)
        {
            var content = menu.transform.Find("Content");
            var top = menu.transform.Find("Content/Ingredients").GetComponent<RectTransform>();
            var result = menu.transform.Find("Content/CraftingResult").GetComponent<RectTransform>();



            var bottomGo = new GameObject("ExtraIngredients");
            bottomGo.transform.SetParent(content);
            var bottom = bottomGo.AddComponent<RectTransform>();
            //bottom.name = "ExtraIngredients";
            bottom.pivot = new Vector2(result.pivot.x, result.pivot.y);
            //bottom.anchoredPosition = new Vector2(result.anchoredPosition.x, result.anchoredPosition.y);
            //bottom.anchoredPosition3D = new Vector3(result.anchoredPosition3D.x, result.anchoredPosition3D.y, result.anchoredPosition3D.z);
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
        private void SetBottomLeftNav(IngredientSelector[] slots, int index, Button craftButton, ExtraIngredientSlotOptions slotOptions)
        {
            var navigation = slots[index].navigation;
            //left goes to previous slot (top right)
            navigation.selectOnLeft = slots[index - 1].Button;
            if (craftButton != null)
            {
                navigation.selectOnDown = craftButton;
            }
            //Up goes directly to first item (top left).
            navigation.selectOnUp = slots[0].Button;
            //Configure top right slot.  Right goes to next line, this bottom left slot.
            var prevNavigation = slots[index - 1].navigation;
            prevNavigation.selectOnRight = slots[index].Button;

            var navDownIndex = slotOptions == ExtraIngredientSlotOptions.BottomLeft ? 4 : 2;
            //change top slots down navigation to go to this slot. If there is a Bottom right slot enabled,
            //then only the first 2 should go to this slot. Otherwise, down takes all 4 here.
            for (var top = 0; top < navDownIndex; top++)
            {
                var topNavigation = slots[top].navigation;
                topNavigation.selectOnDown = slots[index].Button;
                //just to make it clearer what we're doing here.
                slots[top].navigation = topNavigation;
            }
        }
        private void SetBottomRightNav(IngredientSelector[] slots, int index, Button craftButton, ExtraIngredientSlotOptions slotOptions)
        {
            var navigation = slots[index].navigation;
            //left goes to previous slot (top right)
            navigation.selectOnLeft = slots[index - 1].Button;
            //if there's a craft button, both right and down go to it
            if (craftButton != null)
            {
                navigation.selectOnDown = craftButton;
                navigation.selectOnRight = craftButton;
            }
            //Up goes directly to last top item (top right).
            navigation.selectOnUp = slots[3].Button;

            //Configure right nav for previous slot (top right or bottom left if configured)
            //right goes to this slot.
            var prevNavigation = slots[index - 1].navigation;
            prevNavigation.selectOnRight = slots[index].Button;

            //change top slots down navigation to go to this slot. If there is a Bottom left slot enabled,
            //then only the last 2 should go to this slot. Otherwise, down takes all 4 here.
            var navDownIndex = slotOptions == ExtraIngredientSlotOptions.BottomRight ? 0 : 2;
            for (var top = navDownIndex; top < 4; top++)
            {
                var topNavigation = slots[top].navigation;
                topNavigation.selectOnDown = slots[index].Button;
                //just to make it clearer what we're doing here.
                slots[top].navigation = topNavigation;
            }
        }

        public void AddStaticIngredients(CustomCraftingMenu menu, ref List<Recipe> recipes)
        {
            if (TryGetDisplayConfig(menu.GetType(), out var recipeDisplayConfig))
            {
                if (recipeDisplayConfig.StaticIngredients != null && recipeDisplayConfig.StaticIngredients.Count > 0)
                {
                    var specificItems = recipeDisplayConfig.StaticIngredients.Where(i => i.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient);
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
                        foreach (var staticIngredient in recipeDisplayConfig.StaticIngredients)
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
    }
}
