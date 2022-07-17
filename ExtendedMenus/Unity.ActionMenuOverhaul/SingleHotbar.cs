using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class SingleHotbar : MonoBehaviour, IHotbar
    {
        public GameObject GameObject { get => this.gameObject; }

        public event Action<float> OnResizeWidthRequest;

        private GridLayoutGroup _baseGrid;
        private GridLayoutGroup[] _hotbars;
        private GameObject _baseActionSlot;
        private GameObject[][] _hotbarSlots;
        /// <summary>
        /// [Hotbar Index][Slot Index]
        /// </summary>
        public GameObject[][] HotbarSlots => _hotbarSlots;

        public int ActionSlotsPerBar => _baseGrid.constraintCount;
        public int RowCount
        {
            get
            {
                if (_hotbarSlots == null || _hotbarSlots.Length == 0 || _hotbars == null || _hotbars.Length == 0)
                        return 1;

                return _hotbarSlots[0].Length / _hotbars[0].constraintCount;
            }
        }
        public int HotbarCount => _hotbars.Length;

        private int _selectedHotbar;
        public int SelectedHotbar => _selectedHotbar;

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
            _baseGrid = GetComponentsInChildren<GridLayoutGroup>().First(g => g.name == "BaseHotbarGrid");
            _baseActionSlot = _baseGrid.GetComponentInChildren<Button>().gameObject;
            _baseGrid.gameObject.SetActive(false);
            _baseActionSlot.SetActive(false);
        }

        public void ConfigureHotbar(int hotbars, int actionSlots)
        {
            ConfigureHotbar(hotbars, 1, actionSlots);
        }
        public void ConfigureHotbar(int hotbars, int rows, int slotsPerRow)
        {
            Reset();
            _baseGrid.constraintCount = slotsPerRow;
            _hotbars = new GridLayoutGroup[hotbars];
            _hotbarSlots = new GameObject[hotbars][];
            for (int h = 0; h < hotbars; h++)
            {
                _hotbars[h] = Instantiate(_baseGrid, _baseGrid.transform.parent);
                _hotbars[h].name = "HotbarsGrid" + h;
                _hotbars[h].gameObject.SetActive(h == 0);
                _hotbarSlots[h] = new GameObject[slotsPerRow * rows];
                for (int s = 0; s < slotsPerRow * rows; s++)
                {
                    var newSlot = Instantiate(_baseActionSlot, _hotbars[h].transform);
                    var slotBtn = newSlot.GetComponent<ActionSlot>();
                    slotBtn.SlotNo = s + 1;
                    slotBtn.HotbarId = h;
                    newSlot.SetActive(true);
                    _hotbarSlots[h][s] = newSlot;
                }
            }
            StartCoroutine(ResizeLayoutGroup());
        }

        public void SelectHotbar(int hotbar)
        {
            for (int h =0; h < _hotbars.Length; h++)
            {
                _hotbars[h].gameObject.SetActive(h == hotbar);
            }
            _selectedHotbar = hotbar;
        }

        public void SelectNext()
        {
            int nextBar = _selectedHotbar + 1 >= _hotbars.Length ? 0 : _selectedHotbar + 1;
            SelectHotbar(nextBar);
        }

        public void SelectPrevious()
        {
            int previousBar = _selectedHotbar - 1 < 0 ? _hotbars.Length - 1 : _selectedHotbar - 1;
            SelectHotbar(previousBar);
        }

        private void Reset()
        {
            var allHotbars = GetComponentsInChildren<GridLayoutGroup>(true);

            for (int h = 0; h < allHotbars.Length; h++)
            {
                if (allHotbars[h] != _baseGrid)
                {
                    var actionSlots = allHotbars[h].GetComponentsInChildren<ActionSlot>(true);
                    for (int s = 0; s < actionSlots.Length; s++)
                        Destroy(actionSlots[s].gameObject);

                    Destroy(allHotbars[h]);
                }
            }
            ResetCollections();
            _selectedHotbar = 0;
        }
        private void ResetCollections()
        {
            _hotbars = new GridLayoutGroup[0];
            _hotbarSlots = new GameObject[0][];
        }
        IEnumerator ResizeLayoutGroup()
        {
            yield return new WaitForEndOfFrame();

            float btnWidth = HotbarSlots[0][0].GetComponent<RectTransform>().rect.width;
            Debug.Log($"Slot Button RectTransform has a width of {btnWidth}");

            var glgRect = _hotbars[0].GetComponent<RectTransform>().rect;
            float width = glgRect.width;
            float hotbarWidth = (btnWidth + _hotbars[0].spacing.x) * ((float)_hotbars[0].constraintCount) + _hotbars[0].padding.horizontal * 2 - _hotbars[0].spacing.x;

            OnResizeWidthRequest?.Invoke(hotbarWidth);
        }
    }
}