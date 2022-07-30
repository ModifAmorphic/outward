using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class PlayerMenu : MonoBehaviour
    {
        private int _rewiredID;
        public int RewiredID { get => _rewiredID; }

        private string _playerUID;
        public string PlayerUID { get => _playerUID; }

        public Text posText;
        public RectTransform rectTransform;

        public Text canvasPosText;
        public RectTransform canvasRectTransform;

        //private GameObject _characterGo;
        //public GameObject CharacterGo { get => _characterGo; }

        public void SetIDs(int rewiredID, string playerUID) => (_rewiredID, _playerUID) = (rewiredID, playerUID);

        private void Awake()
        {
            posText = GetComponentsInChildren<Text>().First(t => t.name == "PlayerPosText");
            rectTransform = GetComponent<RectTransform>();

            canvasPosText = GetComponentsInChildren<Text>().First(t => t.name == "ActionCanvasPosText");
            canvasRectTransform = GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        }
        private void Update()
        {
            posText.text = $"Player Pos: {rectTransform.position.x}, {rectTransform.position.y}. Size {rectTransform.sizeDelta.x}, {rectTransform.sizeDelta.y}";

            canvasPosText.text = $"ActionCanvas Pos: {canvasRectTransform.position.x}, {canvasRectTransform.position.y}. Size {canvasRectTransform.sizeDelta.x}, {canvasRectTransform.sizeDelta.y}";
        }
        
    }
}
