using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    [DisallowMultipleComponent]
    public partial class Player : Character
    {
        [SerializeField]
        private bool _initPlayerOnStart = true;

        public ITrigger bestTriggerInRange { get; set; }
        public BestTriggerSelectorBase triggerSelector;
        
        private readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(0.1f);
        private readonly List<IPlayerInputCallbacks> _inputCallbacksCache = new List<IPlayerInputCallbacks>(); 
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            if (_initPlayerOnStart)
            {
                RegisterPlayerAsCurrentPlayer();
            }

            if (triggerSelector != null)
            {
                StartCoroutine(SelectBestTrigger());
            }
            
            InputManager.OnPlayerInputChanged += OnPlayerInputChanged;
        }

        private IEnumerator SelectBestTrigger()
        {
            while (true)
            {
                yield return _waitForSeconds;
                bestTriggerInRange = triggerSelector.GetBestTrigger(this, triggersInRange);
            }
        }

        private void Update()
        {
            if (bestTriggerInRange != null)
            {
                var c = bestTriggerInRange as UnityEngine.Component;
                if(c != null)
                {
                    var inputHandler = c.GetComponent<ITriggerInputHandler>();
                    if (inputHandler != null)
                    {
                        if (inputHandler.AreKeysDown())
                        {
                            inputHandler.Use(this);
                        }
                    }    
                }
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InputManager.OnPlayerInputChanged -= OnPlayerInputChanged;
        }

        public void RegisterPlayerAsCurrentPlayer()
        {
            PlayerManager.currentPlayer = this;
        }
        
        protected virtual void OnPlayerInputChanged(bool isInputLimited)
        {
            GetComponentsInChildren<IPlayerInputCallbacks>(true, _inputCallbacksCache);
            foreach (var c in _inputCallbacksCache)
            {
                c.SetInputActive(!isInputLimited);
            }
        }
    }
}