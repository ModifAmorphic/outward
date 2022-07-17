using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class GridHotbar : MonoBehaviour, IHotbar
    {
        public GameObject GameObject { get => this.gameObject; }

        public event Action<float> OnResizeWidthRequest;

        private GridLayoutGroup _hotbarsGrid;
        private GameObject _baseActionSlot;
        private GameObject[][] _actionSlots;
        public GameObject[][] HotbarSlots { get => _actionSlots; }

        public int ActionSlotsPerBar { get => _hotbarsGrid.constraintCount; }
        public int HotbarCount
        {
            get
            {
                var totalSlots = _hotbarsGrid.transform.childCount;
                return totalSlots > _hotbarsGrid.constraintCount ? totalSlots / _hotbarsGrid.constraintCount : 1;
            }
        }
        public int RowCount
        {
            get
            {
                if (_actionSlots == null || _actionSlots.Length == 0 || _hotbarsGrid == null )
                    return 1;

                return _actionSlots.Length / _hotbarsGrid.constraintCount;
            }
        }

        public int SelectedHotbar => throw new NotImplementedException();

        private void Awake()
        {
            SetComponents();
            //ConfigureHotbar(1, 8);
        }

        private void Start()
        {


        }

        private void SetComponents()
        {
            _hotbarsGrid = GetComponentsInChildren<GridLayoutGroup>(true).First();
            _baseActionSlot = _hotbarsGrid.GetComponentsInChildren<Button>(true).First().gameObject;
            _baseActionSlot.SetActive(false);
        }


        public void ConfigureHotbar(int hotbars, int rows, int actionSlots) => ConfigureHotbar(hotbars, actionSlots);
        public void ConfigureHotbar(int hotbars, int actionSlots)
        {
            Reset();
            _hotbarsGrid.constraintCount = actionSlots;

            _actionSlots = new GameObject[hotbars][];
            for (int h = 0; h < hotbars; h++)
            {
                _actionSlots[h] = new GameObject[actionSlots];
                for (int s = 0; s < actionSlots; s++)
                {
                    var newSlot = Instantiate(_baseActionSlot, _hotbarsGrid.transform);
                    var slotBtn = newSlot.GetComponent<ActionSlot>();
                    slotBtn.SlotNo = s + 1;
                    slotBtn.HotbarId = h;
                    newSlot.SetActive(true);
                    _actionSlots[h][s] = newSlot;
                }
            }
            StartCoroutine(ResizeLayoutGroup());
        }

        public void SelectHotbar(int hotbarIndex) 
        {
            if (hotbarIndex != 0)
                throw new ArgumentOutOfRangeException(nameof(hotbarIndex));
        }
        public void SelectNext() { }
        public void SelectPrevious() { }

        private void Reset()
        {
            var actionSlots = _hotbarsGrid.GetComponentsInChildren<ActionSlot>(false);
            if (actionSlots != null)
            {
                for (int i = 0; i < actionSlots.Length; i++)
                {
                    DestroyImmediate(actionSlots[i].gameObject);
                }
            }

        }
        IEnumerator ResizeLayoutGroup()
        {
            yield return new WaitForEndOfFrame();

            float btnWidth = HotbarSlots[0][0].GetComponent<RectTransform>().rect.width;
            Debug.Log($"Slot Button RectTransform has a width of {btnWidth}");

            var glgRect = _hotbarsGrid.GetComponent<RectTransform>().rect;
            float width = glgRect.width;
            float hotbarWidth = (btnWidth + _hotbarsGrid.spacing.x) * ((float)_hotbarsGrid.constraintCount) + _hotbarsGrid.padding.horizontal * 2 - _hotbarsGrid.spacing.x;

            OnResizeWidthRequest?.Invoke(hotbarWidth);

            var parentName = this.transform.parent.gameObject.name;
            Debug.Log($"Changing width of {parentName}'s RectTransform from {width} to {hotbarWidth}");


            //GetComponentInParent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hotbarWidth);
        }
    }
}