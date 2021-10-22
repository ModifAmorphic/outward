//using ModifAmorphic.Outward.Extensions;
//using ModifAmorphic.Outward.Logging;
//using ModifAmorphic.Outward.Modules.Crafting.CompatibleIngredients;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ModifAmorphic.Outward.Modules.Crafting
//{
//    internal class IngredientSlotSorter
//    {
//        private readonly IngredientSelector[] _sorter = new IngredientSelector[6];
//        private readonly IngredientSelector[] _emptySelectors = new IngredientSelector[6];
//        public void SortExtraIngredients(IngredientSelector[] selectors, (RecipeIngredient Ingredient, CompatibleIngredient BestIngredient)[] extraIngredients) 
//        {
//            if (extraIngredients.Length + 4 > selectors.Length)
//                throw new ArgumentOutOfRangeException("Extra Ingredients can not be sorted into the IngredientSelector displays because it would exceed the amount" +
//                    "of slots available.", "extraIngredients");

//            int index = 0;
//            for (int i = 0; i < selectors.Length; i++)
//            {
//                index = i;
//                if (selectors[i].AssignedIngredient == null)
//                    break;
//            }
//            var extraSlot = 4;
//            for (int i = 0; i < extraIngredients.Length; i++)
//            {
//                extraSlot += i;
//                //Stash the selector about to be replaced
//                _sorter[index] = selectors[index];
//                //replace this "next" selector with one of the extra slots
//                selectors[index] = selectors[extraSlot];
//                //set the extra slot to the selector it just replaced so it can be resorted later
//                selectors[extraSlot] = _sorter[index];
//                //Set up the new selector
//                selectors[index].Set(extraIngredients[i].Ingredient, extraIngredients[i].BestIngredient);
//                //swap complete - clear the original
//                selectors[extraSlot].Clear();
//                index++;
//            }
//        }

//        private class FoundIngredient
//        {
//            public Guid IngredientID;
//            public int FoundAtSlotIndex;
//            public int DesiredDisplayPosition;
//            public int CurrentSlotIndex;
//            public int InitializedSlotIndex;
//        }
//        private void SwapSelectorIngredients(IngredientSelector selector1, IngredientSelector selector2, IModifLogger logger)
//        {
//            //Get the current selector with the static ingredients.
//            var ingredient1 = selector1.GetRecipeIngredient();
//            var assigned1 = selector1.AssignedIngredient;

//            //Get the static selector that wants the ingredients
//            var ingredient2 = selector2.GetRecipeIngredient();
//            var assigned2 = selector2.AssignedIngredient;

//            //Set the selector being swapped out, to the ingredients of the selector being swapped in
//            selector1.Free(true);
//            selector1.Clear();

//            if (!(ingredient2 is CustomRecipeIngredient ing2) || ing2.CustomRecipeIngredientID != (ingredient1 as CustomRecipeIngredient)?.CustomRecipeIngredientID)
//                selector1.Set(ingredient2, assigned2);

//            //if (swappedIngredient != null)
//            //    logger.LogDebug($"SortSlots: Swapped Ingredients at slot index: [{swapIndex}][m_selectorIndex={swappedInternal}] = ActionType: {swappedIngredient.ActionType}" +
//            //         $"{(swappedIngredient.AddedIngredient != null ? $", AddedIngredient: {swappedIngredient.AddedIngredient}" : "")}" +
//            //         $"{(swappedIngredient.AddedIngredientType != null ? $", AddedIngredientType: {swappedIngredient.AddedIngredientType.Tag}" : "")}");

//            //Do the reverse now, completing the swap.
//            selector2.Free(true);
//            selector2.Clear();

//            logger.LogDebug($"SortSlots: Swapping ingredient found to selector with [m_selectorIndex={selector2.GetPrivateField<IngredientSelector, int>("m_selectorIndex")}]" +
//                                $" ActionType: {ingredient1?.ActionType}" +
//                                $"{(ingredient1?.AddedIngredient != null ? $", AddedIngredient: {ingredient1?.AddedIngredient}" : "")}" +
//                                $"{(ingredient1?.AddedIngredientType != null ? $", AddedIngredientType: {ingredient1?.AddedIngredientType.Tag}" : "")}" +
//                                $", AssignedIngredient: {assigned1?.ItemPrefab} - {assigned1?.GetOwnedItems()?[0]}");
//            selector2.Set(ingredient1, assigned1);

//            //var staticsNewIngredient = staticSelector.GetRecipeIngredient();
//            //int staticInternal = staticSelector.GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //if (staticsNewIngredient != null)
//            //    logger.LogDebug($"SortSlots: Set Static Slots Ingredients at index: [{staticIndex}][m_selectorIndex={staticInternal}] = ActionType: {staticsNewIngredient.ActionType}" +
//            //         $"{(staticsNewIngredient.AddedIngredient != null ? $", AddedIngredient: {staticsNewIngredient.AddedIngredient}" : "")}" +
//            //         $"{(staticsNewIngredient.AddedIngredientType != null ? $", AddedIngredientType: {staticsNewIngredient.AddedIngredientType.Tag}" : "")}");
//        }
//        public void SortSlots(Recipe selectedRecipe, IngredientSelector[] selectors, List<StaticIngredient> staticIngredients, IModifLogger logger)
//        {
//            logger.LogDebug($"SortSlots: selectors: {selectors?.Count()}, staticIngredients: {staticIngredients.Count()}.");
//            //Reset(selectors, logger);
//            var staticIngreds = staticIngredients?.Count ?? 0;
//            if (staticIngreds < 1)
//                return;


//            int staticsFound = 0;
//            int activeSlots = 0;
//            for (int i = 0; i < selectors.Length; i++)
//            {

//                //if (selectors[i] == null)
//                //    break;
//                activeSlots++;
//                if (selectors[i].GetRecipeIngredient() is CustomRecipeIngredient customIngredient)
//                {
//                    foreach (var s in staticIngredients)
//                    {
//                        if (s.IngredientID == customIngredient.CustomRecipeIngredientID)
//                        {
//                            var staticIndex = (int)s.IngredientSlotPosition;
//                            var internalIndex = selectors[i].GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//                            logger.LogDebug($"SortSlots: Found static ingredient at slot index [{i}][m_selectorIndex={internalIndex}][IngredientSlotPosition={(int)s.IngredientSlotPosition}]. ActionType: {s.ActionType}" +
//                                $"{(customIngredient.AddedIngredient != null ? $", AddedIngredient: {customIngredient.AddedIngredient}" : "")}" +
//                                $"{(customIngredient.AddedIngredientType != null ? $", AddedIngredientType: {customIngredient.AddedIngredientType.Tag}" : "")}");

//                            if (i != staticIndex)
//                            {
//                                SwapSelectorIngredients(selectors[i], selectors[staticIndex], logger);
//                                var swappedIn = selectors[i].GetRecipeIngredient() as CustomRecipeIngredient;
//                                logger.LogDebug($"SortSlots: Swap happened at slot index [{i}][m_selectorIndex={selectors[i].GetPrivateField<IngredientSelector, int>("m_selectorIndex")}][IngredientSlotPosition={(int)s.IngredientSlotPosition}]. ActionType: {s.ActionType}" +
//                                $"{(swappedIn?.AddedIngredient != null ? $", AddedIngredient: {swappedIn?.AddedIngredient}" : "")}" +
//                                $"{(swappedIn?.AddedIngredientType != null ? $", AddedIngredientType: {swappedIn?.AddedIngredientType.Tag}" : "")}");
//                            }
//                            //staticSlots[staticsFound] = (s, i, internalIndex, (int)s.IngredientSlotPosition);
//                            //ingredients.Add(s.IngredientID, (s, i, i, internalIndex, (int)s.IngredientSlotPosition));
//                            staticsFound++;
//                        }
//                    }
//                }
//            }

//            //for (int i = 0; i < selectors.Length; i++)
//            //{
//            //    if (selectors[i].GetRecipeIngredient() == null)
//            //    {
//            //        selectors[i].Free();
//            //        selectors[i].Clear();
//            //    }
                
//            //}

//            //if (!swapHappened)
//            //    return;

//            //int fillSlotIndex = 0;
//            //int emptySlotIndex = 0;
//            //for (int i = 0; i < selectors.Length; i++)
//            //{
//            //    if (selectors[i].GetRecipeIngredient() != null)
//            //    {
//            //        _sorter[fillSlotIndex] = selectors[i];
//            //        fillSlotIndex++;
//            //    }
//            //    else
//            //    {
//            //        _emptySelectors[emptySlotIndex] = selectors[i];
//            //        emptySlotIndex++;
//            //    }
//            //}

//            //for (int i = 0; i < fillSlotIndex; i++)
//            //{
//            //    selectors[i] = _sorter[i];
//            //}
//            //emptySlotIndex = 0;
//            //for (int i = fillSlotIndex; i < selectors.Length; i ++)
//            //{
//            //    selectors[i] = _emptySelectors[emptySlotIndex];
//            //    emptySlotIndex++;
//            //}
//            return;

//            // if (staticsFound != staticIngreds)
//            //{
//            //    if (staticsFound == 0)
//            //    {
//            //        logger.LogDebug($"SortSlots: No ingredients matching static ingredients found in displays. Tried to find {staticIngreds} static ingredients.");
//            //        return;
//            //    }
//            //}
//            //for (int i = 0; i < selectors.Length; i++)
//            //{

//            //}
//            //foreach(var id in ingredients.Keys)
//            //{
//            //    //If the internal index doesn't match the desired display position, that means
//            //    //the ingredient is in the wrong spot
//            //    if (ingredients[id].InternalIndex != (int)ingredients[id].StaticIngredient.IngredientSlotPosition)
//            //    {
//            //        var swapIndex = ingredients[id].CurrentIndex;
//            //        var staticIndex = (int)ingredients[id].StaticIngredient.IngredientSlotPosition;

//            //        //Get the current selector with the static ingredients.
//            //        var swapSelector = selectors[swapIndex];
//            //        var staticIngredient = swapSelector.GetRecipeIngredient();
//            //        var staticAssigned = swapSelector.AssignedIngredient;

//            //        //Get the static selector that wants the ingredients
//            //        var staticSelector = selectors[staticIndex];
//            //        var swapIngredient = staticSelector.GetRecipeIngredient();
//            //        var swapAssigned = staticSelector.AssignedIngredient;

//            //        //Set the selector being swapped out, to the ingredients of the selector being swapped in
//            //        swapSelector.Free(true);
//            //        swapSelector.Set(swapIngredient, swapAssigned);

//            //        var swappedIngredient = swapSelector.GetRecipeIngredient();
//            //        int swappedInternal = swapSelector.GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //        if (swappedIngredient != null)
//            //            logger.LogDebug($"SortSlots: Swapped Ingredients at slot index: [{swapIndex}][m_selectorIndex={swappedInternal}] = ActionType: {swappedIngredient.ActionType}" +
//            //                 $"{(swappedIngredient.AddedIngredient != null ? $", AddedIngredient: {swappedIngredient.AddedIngredient}" : "")}" +
//            //                 $"{(swappedIngredient.AddedIngredientType != null ? $", AddedIngredientType: {swappedIngredient.AddedIngredientType.Tag}" : "")}");

//            //        //Do the reverse now, completing the swap.
//            //        staticSelector.Free(true);
//            //        staticSelector.Set(staticIngredient, staticAssigned);

//            //        var staticsNewIngredient = staticSelector.GetRecipeIngredient();
//            //        int staticInternal = staticSelector.GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //        if (staticsNewIngredient != null)
//            //            logger.LogDebug($"SortSlots: Set Static Slots Ingredients at index: [{staticIndex}][m_selectorIndex={staticInternal}] = ActionType: {staticsNewIngredient.ActionType}" +
//            //                 $"{(staticsNewIngredient.AddedIngredient != null ? $", AddedIngredient: {staticsNewIngredient.AddedIngredient}" : "")}" +
//            //                 $"{(staticsNewIngredient.AddedIngredientType != null ? $", AddedIngredientType: {staticsNewIngredient.AddedIngredientType.Tag}" : "")}");

//            //        //do the actual swap
//            //        selectors[swapIndex] = staticSelector;
//            //        selectors[staticIndex] = swapSelector;

//            //        //record the swap
//            //        if (swapIngredient is CustomRecipeIngredient cswapIngredient)
//            //        {
//            //            if (ingredients.TryGetValue(cswapIngredient.CustomRecipeIngredientID, out var otherStaticIng))
//            //            {
//            //                otherStaticIng.CurrentIndex = staticIndex;
//            //            }
//            //        }

//            //    }

//            ////check if selector has the static ingredient assigned, and if not swap with whatever selector does.
//            //(int low, int high) lastSwap = (-1, -1);
//            //for (int i = 0; i < staticsFound; i++)
//            //{
//            //    logger.LogDebug($"SortSlots: Swap i={i}; FoundAtIndex={ingredients[i].FoundAtIndex}, FoundAtInternalIndex={ingredients[i].FoundAtInternalIndex}, PositionNo={ingredients[i].PositionNo} -------------------------------------");
//            //    //internal index is set to the initial array index the slot was assigned to and doesn't change.
//            //    //it can be used to reset orders, or in the case check if the slot we found with the ingredient is the "static" slot.
//            //    //If not, then we need to swap it
//            //    if (ingredients[i].FoundAtInternalIndex != ingredients[i].PositionNo
//            //        && !(lastSwap.low == ingredients[i].PositionNo && lastSwap.high == ingredients[i].FoundAtIndex))
//            //    {
//            //        var swapIndex = ingredients[i].FoundAtIndex;
//            //        var staticIndex = ingredients[i].PositionNo;
                    
//            //        //Get the current selector with the static ingredients.
//            //        var swapSelector = selectors[swapIndex];
//            //        var staticIngredient = swapSelector.GetRecipeIngredient();
//            //        var staticAssigned = swapSelector.AssignedIngredient;

//            //        //Get the static selector that wants the ingredients
//            //        var staticSelector = selectors[staticIndex];
//            //        var swapIngredient = staticSelector.GetRecipeIngredient();
//            //        var swapAssigned = staticSelector.AssignedIngredient;


//            //        //Set the selector being swapped out, to the ingredients of the selector being swapped in
//            //        swapSelector.Free(true);
//            //        swapSelector.Set(swapIngredient, swapAssigned);

//            //        var swappedIngredient = swapSelector.GetRecipeIngredient();
//            //        int swappedInternal = swapSelector.GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //        if (swappedIngredient != null)
//            //            logger.LogDebug($"SortSlots: Swapped Ingredients at slot index: [{swapIndex}][m_selectorIndex={swappedInternal}] = ActionType: {swappedIngredient.ActionType}" +
//            //                 $"{(swappedIngredient.AddedIngredient != null ? $", AddedIngredient: {swappedIngredient.AddedIngredient}" : "")}" +
//            //                 $"{(swappedIngredient.AddedIngredientType != null ? $", AddedIngredientType: {swappedIngredient.AddedIngredientType.Tag}" : "")}");

//            //        //Do the reverse now, completing the swap.
//            //        staticSelector.Free(true);
//            //        staticSelector.Set(staticIngredient, staticAssigned);

//            //        var staticsNewIngredient = staticSelector.GetRecipeIngredient();
//            //        int staticInternal = staticSelector.GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //        if (staticsNewIngredient != null)
//            //            logger.LogDebug($"SortSlots: Set Static Slots Ingredients at index: [{staticIndex}][m_selectorIndex={staticInternal}] = ActionType: {staticsNewIngredient.ActionType}" +
//            //                 $"{(staticsNewIngredient.AddedIngredient != null ? $", AddedIngredient: {staticsNewIngredient.AddedIngredient}" : "")}" +
//            //                 $"{(staticsNewIngredient.AddedIngredientType != null ? $", AddedIngredientType: {staticsNewIngredient.AddedIngredientType.Tag}" : "")}");


//            //        //now do the actual slot swapping unless the static slot is already in it's desired position
//            //        if (staticIndex != i)
//            //        {
//            //            logger.LogDebug($"SortSlots: Swapping Slots {swapIndex} and {staticIndex}.");
//            //            selectors[swapIndex] = staticSelector;
//            //            selectors[staticIndex] = swapSelector;
//            //            lastSwap = (swapIndex, staticIndex);
//            //        }
//            //    }
//            //}
            
            
//            //for (int i = 0; i < staticsFound; i++)
//            //{
//            //    logger.LogDebug($"SortSlots: Swap i={i} -------------------------------------");
//            //    //logger.LogDebug($"SortSlots: Plan: Move static ingredient found at Slot Index {staticSlots[i].FoundAtIndex} .");
//            //    var swapToIndex = staticSlots[i].FoundAtIndex;
//            //    var swapFromIndex = (int)staticSlots[i].StaticIngredient.IngredientSlotPosition;
//            //    //if its already in the right spot, just continue on
//            //    if (staticSlots[i].FoundAtIndex == (int)staticSlots[i].StaticIngredient.IngredientSlotPosition)
//            //        continue;
//            //    var swapOut = selectors[swapToIndex];
//            //    var swapOutIngredient = swapOut.GetRecipeIngredient();
//            //    var swapOutAssigned = swapOut.AssignedIngredient;

//            //    var swapIn = selectors[swapFromIndex];
//            //    var swapInIngredient = swapIn.GetRecipeIngredient();
//            //    var swapInAssigned = swapIn.AssignedIngredient;

//            //    swapOut.Free(true);
//            //    swapOut.Set(swapInIngredient, swapInAssigned);

//            //    swapIn.Free(true);
//            //    swapIn.Set(swapOutIngredient, swapOutAssigned);
                
//            //    selectors[swapToIndex] = swapIn;
//            //    var swapped1 = selectors[swapToIndex].GetRecipeIngredient();
//            //    int swapped1Index = selectors[swapToIndex].GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //    if (swapped1 != null)
//            //        logger.LogDebug($"SortSlots: 1: [{swapToIndex}][m_selectorIndex={swapped1Index}] = ActionType: {swapped1.ActionType}" +
//            //             $"{(swapped1.AddedIngredient != null ? $", AddedIngredient: {swapped1.AddedIngredient}" : "")}" +
//            //             $"{(swapped1.AddedIngredientType != null ? $", AddedIngredientType: {swapped1.AddedIngredientType.Tag}" : "")}");
//            //    else
//            //        logger.LogDebug($"SortSlots: 1: [{swapToIndex}] = null");

//            //    selectors[swapFromIndex] = swapOut;
//            //    var swapped2 = selectors[swapFromIndex].GetRecipeIngredient();
//            //    int swapped2Index = selectors[swapFromIndex].GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//            //    if (swapped2 != null)
//            //        logger.LogDebug($"SortSlots: 2: [{swapFromIndex}][m_selectorIndex={swapped2Index}] = ActionType: {swapped2?.ActionType}" +
//            //             $"{(swapped2.AddedIngredient != null ? $", AddedIngredient: {swapped2.AddedIngredient}" : "")}" +
//            //             $"{(swapped2.AddedIngredientType != null ? $", AddedIngredientType: {swapped2.AddedIngredientType.Tag}" : "")}");
//            //    else
//            //        logger.LogDebug($"SortSlots: 2: [{swapFromIndex}] = null");

//            //    //_sorter[(int)staticSlots[i].StaticIngredient.IngredientSlotPosition] = swapOut;
//            //    ////Stash the selector about to be replaced
//            //    //_sorter[swapSlot] = selectors[desiredIndex];
//            //    ////check if the slot about to be swapped in has any ingredients and if so, set the slot about to be replaced to them.
//            //    //if (selectors[swapSlot].Button.interactable)
//            //    //    _sorter[swapSlot].Set(selectors[swapSlot].GetRecipeIngredient(), selectors[swapSlot].AssignedIngredient);
//            //    //else
//            //    //    _sorter[swapSlot].Clear();
//            //    //logger.LogDebug($"SortSlots: Stashed 1st IngredientSlot from index {desiredIndex} to swap array index {swapSlot}. Set ingredients to {selectors[swapSlot].GetRecipeIngredient()}, {selectors[swapSlot].AssignedIngredient}.");
//            //    ////Now swap the ingredients from the earlier slot to the one replacing it with values references we grabbed earlier
//            //    //selectors[swapSlot].Set(staticSlots[i].Ingredient, staticSlots[i].BestIngredient);
//            //    //logger.LogDebug($"SortSlots: Set about to be swapped ahead slot {swapSlot} to ingredients: {staticSlots[i].Ingredient}, {staticSlots[i].BestIngredient}.");


//            //    ////Do the actual swap now, replace the first slot with the later one
//            //    //selectors[desiredIndex] = selectors[swapSlot];
//            //    //logger.LogDebug($"SortSlots: Swapped {swapSlot} slot index into {desiredIndex} slot.");
//            //    ////Finish the swap, move the replaced slot back in to the other slots slot
//            //    //selectors[swapSlot] = _sorter[swapSlot];
//            //    //logger.LogDebug($"SortSlots: Swapped stashed slot {swapSlot} slot index into swapped out of slot {swapSlot}. Ingredients: {selectors[swapSlot].GetRecipeIngredient()}, {selectors[swapSlot].AssignedIngredient}.");

//            //}
//            //logger.LogDebug($"SortSlots: 04");
//        }
//        public void Reset(IngredientSelector[] selectors, IModifLogger logger, bool clearSelectors = false) 
//        {
//            logger.LogDebug("IngredientSlotSorter::Reset: Sorting...");
//            for (int i = 0; i < selectors.Length; i++)
//            {
//                int index = selectors[i].GetPrivateField<IngredientSelector, int>("m_selectorIndex");
//                _sorter[index] = selectors[i];
//                logger.LogDebug($"IngredientSlotSorter::Reset: i={i};  m_selectorIndex={index}");
//            }
//            logger.LogDebug("IngredientSlotSorter::Reset: Applying Sort...");
//            for (int i = 0; i < selectors.Length; i++)
//            {
//                selectors[i] = _sorter[i];
//                var compatible = _sorter[i]?.GetRecipeIngredient();
//                logger.LogDebug($"IngredientSlotSorter::Reset: selectors[{i}] = _sorter[{i}] = " +
//                    $"[ActionType: {compatible?.ActionType}, AddedIngredient: {compatible?.AddedIngredient}, AddedIngredientType: {compatible?.AddedIngredientType?.Tag}]");
//                if (clearSelectors)
//                {
//                    selectors[i].Set(null, null);
//                    selectors[i].Free(true);
//                    selectors[i].Clear();
//                }
//            }
//        }
//    }
//}
