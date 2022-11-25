using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class ModifGoService
    {
        private readonly Func<Logging.Logger> _loggerFactory;
#pragma warning disable IDE0051 // Remove unused private members
        private Logging.Logger Logger => _loggerFactory.Invoke();
#pragma warning restore IDE0051 // Remove unused private members

        const string RootPath = "ModifUnityScripts";
        const string ActivablePath = "Activable";
        const string InactivablePath = "Inactivable";

        private readonly GameObject _modifRoot;
        public GameObject ModifRoot => _modifRoot;

        private readonly GameObject _activable;
        public GameObject Activable => _activable;

        private readonly GameObject _inactivable;
        public GameObject Inactivable => _inactivable;

        public ModifGoService(Func<Logging.Logger> loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _modifRoot = GetOrAddModifRoot();
            _activable = GetOrAddActivable();
            _inactivable = GetOrAddInactivable();
        }
        /// <summary>
        /// Gets the parent gameobject for stored mod resources.
        /// </summary>
        /// <param name="modId">Unique ID of the Mod</param>
        /// <param name="activable">Whether or not the Gameobjects will ever be activated while under this parent.</param>
        /// <returns></returns>
        public GameObject GetModResources(string modId, bool activable)
        {
            var modName = modId.Replace(".", "_");
            return activable ? GetOrAddActivableModGo(modName) : GetOrAddInactivableModGo(modName);
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
            var activable = ModifRoot.transform.Find(ActivablePath)?.gameObject;
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
