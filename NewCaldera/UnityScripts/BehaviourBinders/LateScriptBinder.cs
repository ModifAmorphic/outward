using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    public abstract class LateScriptBinder : MonoBehaviour
    {
        public abstract string ScriptName { get; }

        private Component _boundComponent;
        public Component BoundComponent 
        { 
            get
            {
                Bind();
                return _boundComponent;
            }
        }

        private Type _boundType;
        public Type BoundType
        {
            get
            {
                Bind();
                return _boundType;
            }
        }
        private bool _isPreInit = false;

        public bool IsBound { get; private set; }

        private void Awake()
        {
            Debug.Log($"{this.GetType().Name}::Awake");
            Bind();
        }

        public Component Bind()
        {
            PreInit();
            IsBound = true;
            return _boundComponent;
        }

        private void PreInit()
        {
            if (_isPreInit)
                return;

            _isPreInit = true;

            var behaviours = GetComponents<MonoBehaviour>();
            var behaviourType = OutwardAssembly.GetType(ScriptName);
            var existing = behaviours.FirstOrDefault(b => b.GetType() == behaviourType);
            if (existing != null)
            {
                _boundComponent = existing;
                _boundType = behaviourType;
                return;
            }

            var isActive = gameObject.activeSelf;
            if (isActive)
            {
                gameObject.SetActive(false);
                Debug.Log($"{this.GetType().Name}'s gameObject {gameObject.name} deactivated prior to binding {ScriptName} MonoBehaviour script.");
            }

            Init();

            if (isActive)
            {
                gameObject.SetActive(true);
                Debug.Log($"{this.GetType().Name}'s gameObject {gameObject.name} activated.");
            }

        }

        protected virtual void Init()
        {
            AttachScript(ScriptName);
        }

        private void AttachScript(string scriptName)
        {
            _boundType = OutwardAssembly.GetType(scriptName);

            _boundComponent = gameObject.AddComponent(BoundType);

            Debug.Log($"Binder {this.GetType().Name} attached MonoBehaviour script {BoundType.Name} to gameObject {gameObject.name}.");
        }
    }
}
