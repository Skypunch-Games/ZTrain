#if REWIRED

using System;
using System.Linq;
using UnityEngine;

namespace Devdog.General.Integration.RewiredIntegration
{
    [RequireComponent(typeof(UIWindow))]
    public sealed class RewiredTriggerUI : MonoBehaviour
    {
        [System.Serializable]
        public class Binder
        {
            public string actionName;
            public Sprite icon;
        }

        public UnityEngine.UI.Image imageIcon;
        public UnityEngine.UI.Text shortcutText;
        public bool moveToTriggerLocation = true;


        [SerializeField]
        private Binder[] _binders = new Binder[0];
        private UIWindow _window;

        private ITrigger _currentTrigger;
        
        private void Awake()
        {
            _window = GetComponent<UIWindow>();
        }

        private void Start()
        {
            PlayerManager.instance.OnPlayerChanged += OnPlayerChanged;
            if (PlayerManager.instance.currentPlayer != null)
            {
                OnPlayerChanged(null, PlayerManager.instance.currentPlayer);
            }
        }

        private void LateUpdate()
        {
            if (PlayerManager.instance.currentPlayer != null)
            {
                var player = PlayerManager.instance.currentPlayer;
                var best = player.triggerSelector.GetBestTrigger(player, player.GetInRangeTriggers());
                if (best != _currentTrigger)
                {
                    BestTriggerChanged(best);
                }
                
                if (moveToTriggerLocation)
                {
                    UpdatePosition(_currentTrigger);
                }
            }
        }
        
        private void OnPlayerChanged(Player oldPlayer, Player newPlayer)
        {
            var best = newPlayer.triggerSelector.GetBestTrigger(newPlayer, newPlayer.GetInRangeTriggers());
            BestTriggerChanged(best);
        }

        private void BestTriggerChanged(ITrigger newTrigger)
        {
            if (newTrigger != null)
            {
                _window.OnShow();
                Repaint(newTrigger);
            }
            else
            {
                _window.OnHide();
            }
        }

        private void UpdatePosition(ITrigger trigger)
        {
            if(trigger != null)
                transform.position = Camera.main.WorldToScreenPoint(trigger.transform.position);
        }

        private void Repaint(ITrigger trigger)
        {
            _window.OnShow();

            Sprite icon = null;
            string actionName = "";
            if (trigger != null)
            {
                var input = trigger.transform.GetComponent<ITriggerInputHandler>();
                if (input != null)
                {
                    var binder = _binders.FirstOrDefault(o => o.actionName == input.actionInfo.actionName);
                    if(binder != null)
                    {
                        icon = binder.icon;
                        actionName = binder.actionName;
                    }
                    else
                    {
                        icon = input.actionInfo.icon;
                        actionName = input.actionInfo.actionName;
                    }
                }
            }

            if (imageIcon != null && imageIcon.sprite != icon)
            {
                imageIcon.sprite = icon;
            }

            if (shortcutText != null && shortcutText.text != actionName)
            {
                shortcutText.text = actionName;
            }
        }
    }    
}

#endif