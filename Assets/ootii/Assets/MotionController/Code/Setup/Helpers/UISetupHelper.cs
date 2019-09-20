using UnityEngine;
using com.ootii.Geometry;
using UnityEngine.EventSystems;

namespace com.ootii.Setup
{
    /// <summary>
    /// Helper functions for setting up the UI
    /// </summary>
    public class UISetupHelper
    {
#if UNITY_EDITOR

        /// <summary>
        /// Find or create a GameObject named "UI" to use as a root-level container
        /// </summary>
        /// <param name="rName"></param>
        /// <returns></returns>
        public static GameObject GetUIContainer(string rName = "UI")
        {
            GameObject lUIObject = GameObject.Find(rName);
            if (lUIObject == null)
            {
                lUIObject = CreateUIContainer(rName);
            }
            else
            {
                Canvas lCanvas = lUIObject.GetComponentInChildren<Canvas>();
                if (lCanvas == null)
                {
                    lUIObject = CreateUIContainer(rName);
                }
            }

            return lUIObject;
        }

        /// <summary>
        /// Configure the GameObject container for the UI. 
        /// </summary>
        /// <param name="rName"></param>
        /// <returns></returns>
        public static GameObject CreateUIContainer(string rName = "UI")
        {
            GameObject lUIObject = GameObject.Find(rName);
            if (lUIObject == null)
            {
                lUIObject = new GameObject(rName);
            }

            // Get or add the Canvas
            Canvas lCanvas = lUIObject.GetComponentInChildren<Canvas>();
            if (lCanvas == null)
            {
                GameObject lCanvasObject = new GameObject("Canvas");
                lCanvasObject.transform.SetParent(lUIObject.transform);
                lCanvasObject.transform.ResetRect();
                lCanvas = lCanvasObject.AddComponent<Canvas>();
                lCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            // Add an Event System
            EventSystem lEventSystem = Object.FindObjectOfType<EventSystem>();
            if (lEventSystem == null)
            {
                GameObject lEventSystemObject = new GameObject("EventSystem");
                lEventSystemObject.transform.SetParent(lUIObject.transform);
                lEventSystemObject.transform.Reset();
                lEventSystemObject.AddComponent<EventSystem>();
                lEventSystemObject.AddComponent<StandaloneInputModule>();
            }

            //CanvasScaler lScaler = lCanvas.GetOrAddComponent<CanvasScaler>();
            //lScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //lScaler.referenceResolution = new Vector2(1920, 1080);
            //lScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            //lScaler.matchWidthOrHeight = 1.0f;
            //lScaler.referencePixelsPerUnit = 100;

            return lUIObject;
        }        
#endif
    }
}
