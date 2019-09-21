using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Devdog.General2
{
    public class TriggerInputHandler : TriggerInputHandlerBase
    {
        public override TriggerActionInfo actionInfo
        {
            get
            {
                return new TriggerActionInfo()
                {
                    actionName = triggerKeyCode.ToString()
                };
            }
        }


        [SerializeField]
        private bool _triggerMouseClick = true;
        public virtual bool triggerMouseClick
        {
            get { return _triggerMouseClick; }
            set { _triggerMouseClick = value; }
        }

        [SerializeField]
        private KeyCode _triggerKeyCode = KeyCode.None;
        public virtual KeyCode triggerKeyCode
        {
            get { return _triggerKeyCode; }
            set { _triggerKeyCode = value; }
        }


        public bool useCursorIcon = true;

        [SerializeField]
        private CursorIcon _cursorIcon;
        public virtual CursorIcon cursorIcon
        {
            get { return _cursorIcon; }
            set { _cursorIcon = value; }
        }

//        protected override void Update()
//        {
//            base.Update();
//
//            if (useCursorIcon && trigger.inRange && TriggerUtility.mouseOnTrigger && UIUtility.isHoveringUIElement == false)
//            {
//                cursorIcon.Enable();
//            }
//        }

        public override bool AreKeysDown()
        {
            if (_triggerKeyCode == KeyCode.None)
            {
                return false;
            }

            return Input.GetKeyDown(_triggerKeyCode);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (useCursorIcon)
            {
                cursorIcon.Enable();
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (_triggerMouseClick)
            {
                Use(PlayerManager.currentPlayer);
            }
        }

        public override string ToString()
        {
            return triggerKeyCode.ToString();
        }
    }
}
