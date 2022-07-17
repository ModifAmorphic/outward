using ModifAmorphic.Outward.Unity.ActionMenus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    [UnityScriptComponent]
    public class PlayerMenu : MonoBehaviour
    {
        private int _rewiredID;
        public int RewiredID { get => _rewiredID; }

        private string _playerUID;
        public string PlayerUID { get => _playerUID; }

        //private GameObject _characterGo;
        //public GameObject CharacterGo { get => _characterGo; }
        
        public void SetIDs(int rewiredID, string playerUID) => (_rewiredID, _playerUID) = (rewiredID, playerUID);

        public void Awake()
        {
        }
        
    }
}
