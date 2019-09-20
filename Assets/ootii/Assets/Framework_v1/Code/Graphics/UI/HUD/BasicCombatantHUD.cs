using System;
using System.Collections;
using com.ootii.Actors.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace com.ootii.UI
{
    public class BasicCombatantHUD : MonoBehaviour
    {
        [Tooltip("The character's Attribute Source.")]
        public BasicAttributes _BasicAttributes;
        public BasicAttributes BasicAttributes
        {
            get { return _BasicAttributes; }
            set { _BasicAttributes = value; }
        }
        
        [Tooltip("Automatically assign this HUD to the player? Requires the player to have the Player tag set.")]
        public bool _UsePlayer = false;
        public bool UsePlayer
        {
            get { return _UsePlayer; }
            set { _UsePlayer = value; }
        }

        [Tooltip("Health attribute key.")]
        public string _HealthKey = "Health";
        public string HealthKey
        {
            get { return _HealthKey; }
            set { _HealthKey = value; }
        }
        
        [Tooltip("Enables easing when changing values?")]
        public bool _UseEasing = true;
        public bool UseEasing
        {
            get { return _UseEasing; }
            set { _UseEasing = value; }
        }
       
        [Tooltip("The easing curve function to use.")]
        public AnimationCurve _EasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve EasingCurve
        {
            get { return _EasingCurve; }
            set { _EasingCurve = value; }
        }
        
        [Tooltip("Speed at which the value is eased.")]
        public float _EasingSpeed = 1f;
        public float EasingSpeed
        {
            get { return _EasingSpeed; }
            set { _EasingSpeed = value; }
        }

        [Tooltip("Hides the HUD when the character is at full health")]
        public bool _HideWhenFull = true;
        public bool HideWhenFull
        {
            get { return _HideWhenFull; }
            set { _HideWhenFull = value; }
        }

        [Tooltip("Hide the HUD upon the character's death?")]
        public bool _HideOnDeath = false;
        public bool HideOnDeath
        {
            get { return _HideOnDeath; }
            set { _HideOnDeath = value; }
        }
        
        [Tooltip("The UGUI Slider control used for the heath bar.")]        
        public Slider _HealthBar;
        public Slider HealthBar
        {
            get { return _HealthBar; }
            set { _HealthBar = value; }
        }       

        [Tooltip("Show debug output in the Unity console?")]
        public bool _ShowDebugInfo = false;
        public bool ShowDebugInfo
        {
            get { return _ShowDebugInfo; }
            set { _ShowDebugInfo = value; }
        }

        // Set true when a Canvas with RenderMode = WorldSpace is a child object
        protected bool mDisplayInWorldSpace = false;
        public bool DisplayInWorldSpace
        {
            get
            {
                if (!mCheckedForCanvas)
                {
                    mCanvas = GetComponentInChildren<Canvas>();
                    mCheckedForCanvas = true;
                    if (mCanvas != null)
                    {
                        mDisplayInWorldSpace = mCanvas.renderMode == RenderMode.WorldSpace;
                        mCanvas.worldCamera = Camera.main;
                    }
                }
                
                return mDisplayInWorldSpace;
            }
        }

        // Set true once we have verified that all necessary UI components exit and the slider has been configured        
        protected bool mIsInitialized = false;                

        // The UGUI Canvas which contains the Slider (used when displaying in World Space)
        protected Canvas mCanvas;

        protected CanvasGroup mCanvasGroup;

        // Flag to determine if we have already checked for the presence of a Canvas child object
        protected bool mCheckedForCanvas = false;

        protected virtual void Awake()
        {
            // If no Slider was assigned in the inspector, attempt to find one
            if (_HealthBar == null) { _HealthBar = GetComponentInChildren<Slider>(); }

            // If we can't find a Slider, log a warning and do not 
            if (_HealthBar == null)
            {
                Debug.LogWarning(string.Format("[{0}] Could not find an attached Slider control.", GetType().Name));
                return;
            }

            mCanvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            // If no Slider was assigned or found, don't continue initializing
            if (_HealthBar == null) { return; }

            // If this HUD is for the player, attempt to find the player
            if (_UsePlayer)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _BasicAttributes = player.GetComponent<BasicAttributes>();
                }
            }
            if (_BasicAttributes == null) return;

            BasicAttributeFloat lHealthAttribute = _BasicAttributes.GetAttribute(_HealthKey) as BasicAttributeFloat;
            if (lHealthAttribute == null) { return; }

            // Set the slider min and max values
            SetupSlider(lHealthAttribute, _HealthBar);

            // Subscribe to change notifications
            _BasicAttributes.OnAttributeValueChangedEvent += OnAttributeValueChanged;

            mIsInitialized = true;
            
            SetVisibility(CheckVisibilityState());                        
        }

        protected virtual void Update()
        {
            if (!mIsInitialized) { return; }

            // Ensure the HUD is visible when it is supposed to be
            if (_HideWhenFull || _HideOnDeath)
            {
                SetVisibility(CheckVisibilityState());
            }
           
            if (!DisplayInWorldSpace) { return; }

            // When displaying in world space, force the HUD to always face the camera 
            if (Camera.main != null) transform.LookAt(Camera.main.transform.position, Vector3.up);
        }

        /// <summary>
        /// Checks if the HUD should be visible
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckVisibilityState()
        {
            bool lVisible = true;    

            bool lHealthFull = Math.Abs(_HealthBar.value - _HealthBar.maxValue) < 0.01f;
            bool lHealthEmpty = _HealthBar.value < 1;            

            if ((_HideWhenFull && _HideOnDeath) && (lHealthFull || lHealthEmpty))
            {                
                lVisible = false;                
            }
            else if (_HideWhenFull && lHealthFull)
            {
                lVisible = false;
            }
            else if (_HideOnDeath && lHealthEmpty)
            {
                lVisible = false;
            }

            return lVisible;
        }

        /// <summary>
        /// Set the HUD's current visibility
        /// </summary>
        /// <param name="rIsVisible"></param>
        protected virtual void SetVisibility(bool rIsVisible)
        {
            if (DisplayInWorldSpace)
            {
                if (mCanvas != null)
                {
                    mCanvas.enabled = rIsVisible;
                    mCanvas.worldCamera = Camera.main;
                }
                return;
            }           

            if (mCanvasGroup != null)
            {
                mCanvasGroup.alpha = rIsVisible ? 1 : 0;
            }
        }        

        /// <summary>
        /// Set up the Slider's min, max, and current values
        /// </summary>
        /// <param name="rAttributeFloat"></param>
        /// <param name="rSlider"></param>
        protected virtual void SetupSlider(BasicAttributeFloat rAttributeFloat, Slider rSlider)
        {
            
            //Debug.Log(string.Format("[SetupSlider] MinValue: {0}  MaxValue: {1}", rAttributeFloat.MinValue, rAttributeFloat.MaxValue));
            // If no Max Value specified, use the Value entered for the attribute
            rSlider.maxValue = Math.Abs(rAttributeFloat.MaxValue - float.MaxValue) < 0.01 ? rAttributeFloat.Value : rAttributeFloat.MaxValue;
            // If no Min Value specified, use 0
            rSlider.minValue = Math.Abs(rAttributeFloat.MinValue - float.MinValue) < 0.01 ? 0 : rAttributeFloat.MinValue;
            rSlider.value = rAttributeFloat.Value;
        }

        /// <summary>
        /// Handle changes to the health attribute
        /// </summary>
        /// <param name="rAttribute"></param>
        /// <param name="rOldValue"></param>
        protected virtual void OnAttributeValueChanged(BasicAttribute rAttribute, object rOldValue)
        {
            BasicAttributeFloat lAttributeFloat = rAttribute as BasicAttributeFloat;
            if (lAttributeFloat == null
                || lAttributeFloat.ID.ToUpperInvariant() != _HealthKey.ToUpperInvariant())
            {
                return;
            }

            if (_ShowDebugInfo)
            {
                Debug.Log(string.Format("[{0}] {1} value changed: {2} [{3}]", GetType().Name,
                    lAttributeFloat._ID, lAttributeFloat.GetValue<float>(), (float) rOldValue));
            }

            if (_UseEasing)
            {
                StartCoroutine(ChangeSliderValue(lAttributeFloat.Value, _HealthBar));
            }
            else
            {
                _HealthBar.value = lAttributeFloat.Value;
            }
        }

        /// <summary>
        /// Ease the slider value to the new target value
        /// </summary>
        /// <param name="rNewValue"></param>
        /// <param name="rSlider"></param>
        /// <returns></returns>
        protected virtual IEnumerator ChangeSliderValue(float rNewValue, Slider rSlider)
        {            
            float lTimer = 0f;
            while (lTimer < 1f)
            {
                float lValue = Mathf.Lerp(rSlider.value, rNewValue, _EasingCurve.Evaluate(lTimer));
                rSlider.value = lValue;

                lTimer += Time.deltaTime * _EasingSpeed;
                yield return null;
            }

            rSlider.value = rNewValue;

            //SetVisibility(CheckVisibilityState());

            //// Remove the HUD upon the combatant's death (if specified)
            //if (_HideOnDeath && rNewValue < 1 && mContainer != null)
            //{
            //    mContainer.gameObject.SetActive(false);
            //    //Destroy(gameObject);
            //}
        }

        protected virtual void OnDestroy()
        {
            // Unsubscribe event delegates
            if (_BasicAttributes != null && _BasicAttributes.OnAttributeValueChangedEvent != null)
            {
                _BasicAttributes.OnAttributeValueChangedEvent -= OnAttributeValueChanged;
            }
        }
    }
}
