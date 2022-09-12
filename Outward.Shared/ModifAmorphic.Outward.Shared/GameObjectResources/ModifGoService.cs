using ModifAmorphic.Outward.Logging;
using System;
using UnityEngine;

namespace ModifAmorphic.Outward.GameObjectResources
{
    public class ModifGoService
    {
        private readonly Func<IModifLogger> _loggerFactory;
#pragma warning disable IDE0051 // Remove unused private members
        private IModifLogger Logger => _loggerFactory.Invoke();
#pragma warning restore IDE0051 // Remove unused private members

        const string RootPath = "ModifShared";
        const string ActivablePath = "Activable";
        const string InactivablePath = "Inactivable";

        private readonly GameObject _modifRoot;
        public GameObject ModifRoot => _modifRoot;

        private readonly GameObject _activable;
        public GameObject Activable => _activable;

        private readonly GameObject _inactivable;
        public GameObject Inactivable => _inactivable;

        public ModifGoService(Func<IModifLogger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _modifRoot = GetOrAddModifRoot();
            _activable = GetOrAddActivable();
            _inactivable = GetOrAddInactivable();
        }

        public GameObject GetModResources(string modId, bool activable)
        {
            var modName = modId.Replace(".", "_");
            return activable ? GetOrAddActivableModGo(modName) : GetOrAddInactivableModGo(modName);
        }

        public ModifItemPrefabs GetItemPrefabs(string modId)
        {
            var modGo = GetModResources(modId, true);

            return modGo.GetOrAddComponent<ModifItemPrefabs>();
        }

        private GameObject GetOrAddModifRoot()
        {
            var root = GameObject.Find(RootPath)?.gameObject;
            if (!root)
            {
                root = new GameObject(RootPath);
                root.SetActive(true);
                root.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(root);
            }
            return root;
        }
        private GameObject GetOrAddActivable()
        {
            var activable = ModifRoot.transform.Find("Activable")?.gameObject;
            if (!activable)
            {
                activable = new GameObject(ActivablePath);
                activable.transform.SetParent(ModifRoot.transform);
                activable.SetActive(true);
                activable.hideFlags = HideFlags.HideAndDontSave;
                //UnityEngine.Object.DontDestroyOnLoad(activable);
            }
            return activable;
        }
        private GameObject GetOrAddInactivable()
        {
            var inactivable = ModifRoot.transform.Find(InactivablePath)?.gameObject;
            if (!inactivable)
            {
                inactivable = new GameObject(InactivablePath);
                inactivable.transform.SetParent(ModifRoot.transform);
                inactivable.SetActive(false);
                inactivable.hideFlags = HideFlags.HideAndDontSave;
                //UnityEngine.Object.DontDestroyOnLoad(inactivable);
            }
            return inactivable;
        }
        private GameObject GetOrAddActivableModGo(string modName)
        {
            var activable = Activable.transform.Find(modName)?.gameObject;
            if (!activable)
            {
                activable = new GameObject(modName);
                activable.transform.SetParent(Activable.transform);
                activable.SetActive(true);
                activable.hideFlags = HideFlags.HideAndDontSave;
                //UnityEngine.Object.DontDestroyOnLoad(activable);
            }
            return activable;
        }
        private GameObject GetOrAddInactivableModGo(string modName)
        {
            var inactivable = Inactivable.transform.Find(modName)?.gameObject;
            if (!inactivable)
            {
                inactivable = new GameObject(modName);
                inactivable.transform.SetParent(Inactivable.transform);
                inactivable.SetActive(false);
                inactivable.hideFlags = HideFlags.HideAndDontSave;
                //UnityEngine.Object.DontDestroyOnLoad(inactivable);
            }
            return inactivable;
        }
    }
}
