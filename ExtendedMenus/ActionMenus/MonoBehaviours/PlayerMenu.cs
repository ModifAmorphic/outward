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
        private int _playerID;
        public int PlayerID { get => _playerID; }

        //public Text posText;
        //public RectTransform rectTransform;

        //public Text canvasPosText;
        //public RectTransform canvasRectTransform;

        public void SetIDs(int playerID) => (_playerID) = (playerID);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            //posText = GetComponentsInChildren<Text>().First(t => t.name == "PlayerPosText");
            //rectTransform = GetComponent<RectTransform>();

            //canvasPosText = GetComponentsInChildren<Text>().First(t => t.name == "ActionCanvasPosText");
            //canvasRectTransform = GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            //posText.text = $"Player Pos: {rectTransform.position.x}, {rectTransform.position.y}. Size {rectTransform.sizeDelta.x}, {rectTransform.sizeDelta.y}";

            //canvasPosText.text = $"ActionCanvas Pos: {canvasRectTransform.position.x}, {canvasRectTransform.position.y}. Size {canvasRectTransform.sizeDelta.x}, {canvasRectTransform.sizeDelta.y}";
        }
        
    }
}
