using System;
using System.Collections;
using UnityEngine;

namespace Devdog.General2.UI
{
    [RequireComponent(typeof(UIWindow))]
    public sealed class UIWindowInputHandler : MonoBehaviour, IUIWindowInputHandler
    {
        public KeyCode keyCode = KeyCode.None;

        private UIWindow _window;
        private void Awake()
        {
            _window = GetComponent<UIWindow>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                _window.Toggle();
            }
        }
    }
}
