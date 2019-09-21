using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2.UI
{
    [RequireComponent(typeof(UIWindow))]
    public sealed class UIWindowSync : MonoBehaviour
    {
        public UIWindow[] otherWindows = new UIWindow[0];

        public bool showWhenShown = true;
        public bool hideWhenHidden = true;

        private void Awake()
        {
            var window = GetComponent<UIWindow>();
            window.OnHide += () =>
            {
                foreach (var w in otherWindows)
                {
                    if (w == null)
                    {
                        continue;
                    }

                    if (hideWhenHidden)
                    {
                        w.Hide();
                    }
                }
            };

            window.OnShow += () =>
            {
                foreach (var w in otherWindows)
                {
                    if (w == null)
                    {
                        continue;
                    }

                    if (showWhenShown)
                    {
                        w.Show();
                    }
                }
            };
        }
    }
}
