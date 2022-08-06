using ModifAmorphic.Outward.ActionMenus.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class ViewerLeftNav : MonoBehaviour
    {
        public GameObject BaseActionGo;
        private readonly Dictionary<int, GameObject> _viewTabs = new Dictionary<int, GameObject>();
        private int _tabCounter = 0;
        private int _lastSelectedId = 0;

        public Button AddViewTab(string displayText)
        {
            var newViewGo = Instantiate(BaseActionGo, this.transform);
            var tabId = _tabCounter;
            _viewTabs.Add(tabId, newViewGo);

            var button = newViewGo.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => ToggleViewTabs(tabId));
            button.GetComponentInChildren<Text>().text = displayText;

            newViewGo.SetActive(true);

            if (!_viewTabs.Any())
                ToggleViewTabs(tabId);
            else
                ToggleViewTab(newViewGo, false);

            _tabCounter++;
            return newViewGo.GetComponent<Button>();
        }

        public void ClearViewTabs()
        {
            for (int i = 0; i < _viewTabs.Count; i++)
            {
                _viewTabs[i].Destroy();
            }
            _viewTabs.Clear();
            _tabCounter = 0;
            _lastSelectedId = 0;
        }
        public void ClickSelectedTab()
        {
            if (_viewTabs.Any())
                ExecuteEvents.Execute(_viewTabs[_lastSelectedId], new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
        private void ToggleViewTabs(int selectedTabId)
        {
            if (!_viewTabs.ContainsKey(selectedTabId))
                throw new ArgumentOutOfRangeException(nameof(selectedTabId));

            foreach(var kvp in _viewTabs)
                ToggleViewTab(kvp.Value, kvp.Key == selectedTabId);
            _lastSelectedId = selectedTabId;

        }
        private void ToggleViewTab(GameObject viewTab, bool selected)
        {
            var images = viewTab.GetComponentsInChildren<Image>(true);
            var activeImage = images.First(i => i.name.Equals("ActiveImage"));
            var inactiveImage = images.First(i => i.name.Equals("InactiveImage"));
            var text = viewTab.GetComponentInChildren<Text>();

            activeImage.gameObject.SetActive(selected);
            inactiveImage.gameObject.SetActive(!selected);

            if (selected)
            {
                EventSystem.current.SetSelectedGameObject(viewTab, null);
                text.color = Color.white;
            }
            else
            {
                text.color = Color.grey;
            }
        }
    }
}
