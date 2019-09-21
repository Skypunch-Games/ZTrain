using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Devdog.General2
{
    public abstract class TriggerInputHandlerBase : MonoBehaviour, ITriggerInputHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public abstract TriggerActionInfo actionInfo { get; }

        protected ITrigger trigger;

        
//        protected static CursorIcon cursorIconBefore;

        protected virtual void Awake()
        {
            trigger = GetComponent<ITrigger>();
            Assert.IsNotNull(trigger, "TriggerInputHandlerBase used but no ITrigger found on object.");
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            TriggerUtility.mouseOnTrigger = trigger;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (TriggerUtility.mouseOnTrigger == trigger)
            {
                TriggerUtility.mouseOnTrigger = null;
            }
            
            // Also called on game quit, which might destroy the manager first.
            if(GeneralSettingsManager.instance != null)
            {
                GeneralSettingsManager.instance.settings.defaultCursor.Enable();
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {

        }

        public abstract bool AreKeysDown();

        public virtual void Use(Character character)
        {
            trigger.Toggle(character);
        }
    }
}
