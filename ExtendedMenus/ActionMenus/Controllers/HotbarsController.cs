using ModifAmorphic.Outward.ActionMenus.Extensions;
using ModifAmorphic.Outward.Unity.ActionMenus.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Controllers
{
    public class HotbarsController : IHotbarController
    {
        private readonly HotbarsContainer _hbc;
        public HotbarsContainer HotbarsContainer => _hbc;

        private bool _resizeNeeded = false;

        public HotbarsController(HotbarsContainer hotbarsContainer)
        {
            _hbc = hotbarsContainer ?? throw new ArgumentNullException(nameof(hotbarsContainer));

            _hbc.ActionsViewer.OnSlotActionSelected += AssignSlotAction;
            //_hbc.HotkeyCapture.OnKeyPressed += AssignSlotHotkey;
            //public void AssignSlotHotkey(int slotId, KeyCode keyCode)
            //{
            //    if (_hbc.ActionSlots.TryGetValue(slotId, out var slot))
            //    {

            //    }
            //}
        }

        private void AssignSlotAction(int slotId, ISlotAction slotAction)
        {
            if (_hbc.ActionSlots.TryGetValue(slotId, out var slot))
            {
                slot.Controller.AssignSlotAction(slotAction);
                //_hbc.HasChanges = true;
            }
        }
        
        public void ConfigureHotbars(IHotbarProfile profile)
        {
            //var slotConfigs = new IActionSlotConfig[profile.Hotbars.Count, profile.SlotsPerRow * profile.Rows];
            //for (int h = 0; h < profile.Hotbars.Count; h++)
            //{
            //    var bar = profile.Hotbars[h];
            //    for (int s = 0; s < bar.Slots.Count; s++)
            //    {
            //        slotConfigs[h, s] = bar.Slots[s].Config;
            //        Debug.Log($"[Debug  :ActionMenus] Setting Slot[{h},{s}].Config to '{(bar.Slots[s].Config == null ? "null" : "an ActionSlotConfig instance.")}'.");
            //    }
            //}
            //ConfigureHotbars(profile.Hotbars.Count, profile.Rows, profile.SlotsPerRow, slotConfigs);
            int selectedIndex = _hbc.SelectedHotbar;

            Reset();
            _hbc.BaseGrid.constraintCount = profile.SlotsPerRow;
            _hbc.ConfigureHotbars(profile.Hotbars.Count);
            Debug.Log("[Debug  :ActionMenus] Configuring ActionSlots.");
            _hbc.LeftHotbarNav.SetNextHotkeyText(profile.NextHotkey);
            _hbc.LeftHotbarNav.SetPreviousHotkeyText(profile.PrevHotkey);
            _hbc.LeftHotbarNav.SetHotkeys(profile.Hotbars.Select(b => b.HotbarHotkey));
            _hbc.LeftHotbarNav.SetBarText((_hbc.SelectedHotbar + 1).ToString());

            for (int h = 0; h < profile.Hotbars.Count; h++)
            {
                var barCanvas = UnityEngine.Object.Instantiate(_hbc.BaseHotbarCanvas);
                barCanvas.transform.SetParent(_hbc.BaseHotbarCanvas.transform.parent, false);
                //barCanvas.transform.localScale = Vector3.one;
                barCanvas.name = "HotbarCanvas" + h;
                barCanvas.gameObject.SetActive(true);
                _hbc.HotbarGrid[h] = UnityEngine.Object.Instantiate(_hbc.BaseGrid, barCanvas.transform);
                _hbc.HotbarGrid[h].name = "HotbarsGrid" + h;
                _hbc.HotbarGrid[h].gameObject.SetActive(true);
                
                _hbc.ConfigureActionSlots(h, profile.SlotsPerRow * profile.Rows);
                for (int s = 0; s < profile.SlotsPerRow * profile.Rows; s++)
                {
                    var newSlot = UnityEngine.Object.Instantiate(_hbc.BaseActionSlot);
                    newSlot.transform.SetParent(_hbc.HotbarGrid[h].transform, false);
                    //newSlot.transform.localScale = Vector3.one;
                    var actionSlot = newSlot.GetComponent<ActionSlot>();
                    actionSlot.SlotIndex = s;
                    actionSlot.HotbarIndex = h;
                    actionSlot.HotbarsContainer = _hbc;
                    actionSlot.Config = profile.Hotbars[h].Slots[s].Config;
                    newSlot.SetActive(true);
                    _hbc.Hotbars[h][s] = actionSlot;
                    _hbc.ActionSlots.Add(actionSlot.SlotId, actionSlot);
                }
                barCanvas.enabled = h == 0;
            }
            Debug.Log($"[Debug  :ActionMenus] Added {_hbc.Hotbars.Length} Hotbars with {_hbc.Hotbars.FirstOrDefault()?.Length} Action Slots each. Calling StartCoroutine ResizeLayoutGroup().");
            _resizeNeeded = true;
            //_hbc.HasChanges = true;

            if (selectedIndex != _hbc.SelectedHotbar && selectedIndex < GetHotbarCount())
                SelectHotbar(selectedIndex);
            else if (selectedIndex != _hbc.SelectedHotbar)
                SelectHotbar(GetHotbarCount() - 1);
            else
                SelectHotbar(_hbc.SelectedHotbar);

        }

        public void EnableHotbars()
        {
            _hbc.ActionBarsCanvas.gameObject.SetActive(true);
            var positionable = _hbc.GetComponent<PositionableUI>();
            if (positionable != null)
            {
                positionable.enabled = true;
                if (positionable.BackgroundImage != null)
                    positionable.BackgroundImage.gameObject.SetActive(true);
            }
        }

        public void DisableHotbars()
        {
            _hbc.ActionBarsCanvas.gameObject.SetActive(false);

            var positionable = _hbc.GetComponent<PositionableUI>();
            if (positionable != null)
            {
                positionable.enabled = false;
                if (positionable.BackgroundImage != null)
                    positionable.BackgroundImage.gameObject.SetActive(false);
            }
        }

        public void SelectHotbar(int barIndex)
        {
            if (barIndex < 0 || barIndex >= _hbc.HotbarGrid.Length )
                throw new ArgumentOutOfRangeException(nameof(barIndex));
            
            for (int h = 0; h < _hbc.HotbarGrid.Length; h++)
            {
                _hbc.HotbarGrid[h].transform.parent.GetComponent<Canvas>().enabled = h == barIndex;
            }
            _hbc.SelectedHotbar = barIndex;
            _hbc.LeftHotbarNav.SelectHotbar(barIndex);
        }

        public void SelectNext()
        {
            int nextBar = _hbc.SelectedHotbar + 1 >= GetHotbarCount() ? 0 : _hbc.SelectedHotbar + 1;
            SelectHotbar(nextBar);
        }

        public void SelectPrevious()
        {
            int previousBar = _hbc.SelectedHotbar - 1 < 0 ? GetHotbarCount() - 1 : _hbc.SelectedHotbar - 1;
            SelectHotbar(previousBar);
        }

        public int GetHotbarCount()
        { 
            if (_hbc.HotbarGrid == null)
                return 0;
            return _hbc.HotbarGrid.Length;
        }
        public int GetActionSlotsPerRow()
        {
            if (_hbc.HotbarGrid == null || _hbc.HotbarGrid.Length == 0)
                return _hbc.BaseGrid.constraintCount;
            return _hbc.HotbarGrid[0].constraintCount;
        }
        public int GetActionSlotsPerBar()
        {
            if (_hbc.Hotbars == null || _hbc.Hotbars.Length == 0)
                return 0;
            return _hbc.Hotbars[0].Length;
        }

        public GridLayoutGroup[] GetHotbarGrids() => _hbc.HotbarGrid;
        public ActionSlot[][] GetActionSlots() => _hbc.Hotbars;
        public int GetRowCount()
        {
            if (_hbc.Hotbars == null || _hbc.Hotbars.Length == 0 || _hbc.HotbarGrid == null || _hbc.HotbarGrid.Length == 0)
                return 1;

            return _hbc.Hotbars[0].Length / _hbc.HotbarGrid[0].constraintCount;
        }

        public void HotbarsContainerUpdate()
        {
            if (_resizeNeeded)
            {
                ResizeLayoutGroup();
                //_hbc.StartCoroutine(ResizeLayoutGroupNextFrame());
            }
        }
        private void Reset()
        {
            var allCanvases = _hbc.GetComponentsInChildren<Canvas>(true);

            for (int c = 0; c < allCanvases.Length; c++)
            {
                var grid = allCanvases[c].GetComponentInChildren<GridLayoutGroup>(true);
                if (grid != null && grid != _hbc.BaseGrid)
                {
                    var actionSlots = grid.GetComponentsInChildren<ActionSlot>(true);
                    for (int s = 0; s < actionSlots.Length; s++)
                    {
                        actionSlots[s].gameObject.Destroy();
                    }
                    grid.gameObject.Destroy();
                    allCanvases[c].gameObject.Destroy();
                }
            }
            _hbc.ResetCollections();
            _hbc.SelectedHotbar = 0;
        }
        void ResizeLayoutGroup()
        {
            //yield return new WaitForEndOfFrame();
            //float btnWidth = _hbc.Hotbars[0][0].GetComponent<RectTransform>().rect.width;
            //float btnHeight = _hbc.Hotbars[0][0].GetComponent<RectTransform>().rect.height;
            if (_hbc.HotbarGrid[0].cellSize.x == 0)
                return;

            //var glgRect = _hbc.Hotbars[0].GetComponent<RectTransform>().rect;
            //float width = glgRect.width;
            float hotbarWidth = (_hbc.HotbarGrid[0].cellSize.x + _hbc.HotbarGrid[0].spacing.x) * (_hbc.HotbarGrid[0].constraintCount) + _hbc.HotbarGrid[0].padding.horizontal * 2 - _hbc.HotbarGrid[0].spacing.x;
            float hotbarHeight = (_hbc.HotbarGrid[0].cellSize.y + _hbc.HotbarGrid[0].spacing.y) * GetRowCount() + _hbc.HotbarGrid[0].padding.vertical * 2 - _hbc.HotbarGrid[0].spacing.y;
            Debug.Log("[Debug  :ActionMenus] ResizeLayoutGroup() called. Calculated width is " + hotbarWidth);

            _hbc.Resize(hotbarWidth, hotbarHeight);
            _resizeNeeded = false;
        }
        
        public void ToggleActionSlotEdits(bool editMode)
        {
            _hbc.IsInActionSlotEditMode = editMode;
            _hbc.LeftHotbarNav.ToggleActionSlotEditMode(editMode);
            //foreach (var slot in _hbc.ActionSlots.Values)
            //{
            //    slot.Controller.ToggleEditMode(editMode);
            //}
        }

        public void ToggleHotkeyEdits(bool editMode)
        {
            _hbc.IsInHotkeyEditMode = editMode;
            _hbc.LeftHotbarNav.ToggleHotkeyEditMode(editMode);
            foreach (var slot in _hbc.ActionSlots.Values)
            {
                slot.Controller.ToggleHotkeyEditMode(editMode);
            }
        }
    }
}