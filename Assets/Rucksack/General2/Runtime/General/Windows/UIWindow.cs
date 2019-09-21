using System;
using System.Collections;
using UnityEngine;

namespace Devdog.General2.UI
{
    public partial class UIWindow : MonoBehaviour
    {
        [System.Serializable]
        public class UIWindowActionEvent : UnityEngine.Events.UnityEvent
        { }


        /// <summary>
        /// Event is fired when the window is hidden.
        /// </summary>
        public event Action OnHide;

        /// <summary>
        /// Event is fired when the window becomes visible.
        /// </summary>
        public event Action OnShow;



        [Header("Behavior")]
        public string windowName = "MyWindow";

        /// <summary>
        /// Should the window be hidden when the game starts?
        /// </summary>
        public bool hideOnStart = true;

        /// <summary>
        /// Set the position to 0,0 when the game starts
        /// </summary>
        public bool resetPositionOnStart = true;


        public bool blockUIInput = false;
        public bool blockPlayerInput = false;


        [Header("Actions")]
        public UIWindowActionEvent onShowActions = new UIWindowActionEvent();
        public UIWindowActionEvent onHideActions = new UIWindowActionEvent();

        /// <summary>
        /// Is the window visible or not? Used for toggling.
        /// </summary>
        public bool isVisible { get; protected set; }


        private RectTransform _rectTransform;
        protected RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }


        private UIWindow _parentWindow;
        private IUIWindowVisuals _visuals;

        protected virtual void Awake()
        {
            _visuals = GetComponent<IUIWindowVisuals>();
            if (resetPositionOnStart)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
#endif
        }

#if UNITY_5_4_OR_NEWER
        private void SceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            LevelStart();
        }
#endif

        protected void OnDestroy()
        {
            if (_parentWindow != null)
            {
                _parentWindow.OnShow -= OnShow;
                _parentWindow.OnHide -= OnHide;
            }

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
#endif
        }

        protected virtual void Start()
        {
            LevelStart();
        }

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3

        public void OnLevelWasLoaded(int level)
        {
            LevelStart();
        }

#endif

        protected virtual void LevelStart()
        {
            if (hideOnStart)
            {
                HideFirst();
            }
            else
            {
                isVisible = true;
            }

            if (transform.parent != null)
            {
                _parentWindow = transform.parent.GetComponentInParent<UIWindow>();
                if (_parentWindow != null && _parentWindow != this)
                {
                    _parentWindow.OnShow += OnShow;
                    _parentWindow.OnHide += OnHide;
                }
            }
        }

        public void SetChildrenActive(bool active)
        {
            foreach (Transform t in transform) 
            { 
                t.gameObject.SetActive(active); 
            } 
 
            var img = gameObject.GetComponents<UnityEngine.UI.Graphic>(); 
            foreach (var graphic in img) 
            { 
                graphic.enabled = active; 
            }
            
//            gameObject.SetActive(active);
        }

        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            DoShow();
        }

        public void Show(float waitTime)
        {
            if (waitTime > 0f)
            {
                StartCoroutine(_Show(waitTime));
            }
            else
            {
                DoShow();
            }
        }

        protected IEnumerator _Show(float waitTime)
        {
            if (isVisible)
            {
                yield break;
            }

            yield return StartCoroutine(CoroutineUtility.WaitRealtime(waitTime));

            DoShow();
        }

        protected virtual void DoShow(bool resetCurrentPage = true)
        {
            if (isVisible)
            {
                return;
            }

            isVisible = true;           
            NotifyWindowShown();
        }
        
        protected void NotifyWindowShown()
        {
            if (blockUIInput)
            {
                InputManager.LimitUIInputTo(gameObject);
            }

            if (blockPlayerInput)
            {
                InputManager.LimitPlayerInputTo(gameObject);
            }

            onShowActions.Invoke();
            if (OnShow != null)
            {
                OnShow();
            }

            if (_visuals != null)
            {
                _visuals.OnShow();
            }
            else
            {
                SetChildrenActive(true);
            }
        }

        protected virtual void HideFirst()
        {
            isVisible = false;
            SetChildrenActive(false);
        }

        /// <summary>
        /// Convenience method for easy upgrading...
        /// </summary>
        public void Hide()
        {
            DoHide();
        }

        public void Hide(float waitTime)
        {
            if (waitTime > 0f)
            {
                StartCoroutine(_Hide(waitTime));
            }
            else
            {
                DoHide();
            }
        }

        protected IEnumerator _Hide(float waitTime)
        {
            if (isVisible == false)
                yield break;

            yield return StartCoroutine(CoroutineUtility.WaitRealtime(waitTime));
 
            DoHide();
        }

        protected virtual void DoHide()
        {
            if (isVisible == false)
            {
                return;
            }

            isVisible = false;
            NotifyWindowHidden();
        }
        
        protected void NotifyWindowHidden()
        {
            if (blockUIInput)
            {
                InputManager.RemoveUILimitInput(gameObject);
            }

            if (blockPlayerInput)
            {
                InputManager.RemovePlayerLimitInput(gameObject);
            }

            onHideActions.Invoke();
            if (OnHide != null)
            {
                OnHide();
            }

            if (_visuals != null)
            {
                _visuals.OnHide();
            }
            else
            {
                SetChildrenActive(false);
            }
        }
    }
}
