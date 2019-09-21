using System;
using System.Collections;
using UnityEngine;

namespace Devdog.General2.UI
{
    [RequireComponent(typeof(UIWindow))]
    [RequireComponent(typeof(Animator))]
    public class UIWindowVisuals : MonoBehaviour, IUIWindowVisuals
    {
        /// <summary>
        /// The animation played when showing the window, if null the item will be shown without animation.
        /// </summary>
        [Header("Audio & Visuals")]
        [SerializeField]
        private MotionInfo _showAnimation;
        private int _showAnimationHash;

        /// <summary>
        /// The animation played when hiding the window, if null the item will be hidden without animation. 
        /// </summary>
        [SerializeField]
        private MotionInfo _hideAnimation;
        private int _hideAnimationHash;

        public AudioClipInfo showAudioClip;
        public AudioClipInfo hideAudioClip;

        private Animator _animator;
        public Animator animator
        {
            get
            {
                if (_animator == null)
                    _animator = GetComponent<Animator>();

                return _animator;
            }
        }
        
        private UIWindow _window;
        private IEnumerator _animationCoroutine;
        
        protected void Start()
        {
            if (_showAnimation.motion != null)
                _showAnimationHash = Animator.StringToHash(_showAnimation.motion.name);

            if (_hideAnimation.motion != null)
                _hideAnimationHash = Animator.StringToHash(_hideAnimation.motion.name);
        }
        
        protected void OnEnable()
        {
            _window = GetComponent<UIWindow>();
        }

        public void OnShow()
        {
            _window.SetChildrenActive(true);
            animator.enabled = true;

            PlayAnimation(_showAnimation, _showAnimationHash, null);
            PlayAudio(showAudioClip);
        }

        public void OnHide()
        {
            PlayAnimation(_hideAnimation, _hideAnimationHash, () =>
            {
                // Still invisible? Maybe it got shown while we waited.
                if (_window.isVisible == false)
                {
                    _window.SetChildrenActive(false);
                    animator.enabled = false;
                }
            });

            PlayAudio(hideAudioClip);
        }

        public void PlayAudio(AudioClipInfo clip)
        {
            var source = GetComponent<AudioSource>();
            if (source != null)
            {
                source.Play(clip);
            }
        }

        private void PlayAnimation(MotionInfo clip, int hash, Action callback)
        {
            if (clip.motion != null)
            {
                if (_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                }

                _animationCoroutine = _PlayAnimationAndDisableAnimator(clip.motion.averageDuration + 0.1f, hash, callback);
                StartCoroutine(_animationCoroutine);
            }
            else
            {
                animator.enabled = false;
                if (callback != null)
                {
                    callback();
                }
            }
        }
        
        /// <summary>
        /// Hides object after animation is completed.
        /// </summary>
        protected IEnumerator _PlayAnimationAndDisableAnimator(float waitTime, int hash, Action callback)
        {
            yield return null; // Needed for some reason, Unity bug??

            var before = _animationCoroutine;
            animator.enabled = true;
            animator.Play(hash);

            yield return StartCoroutine(CoroutineUtility.WaitRealtime(waitTime));

            // If action completed without any other actions overriding isVisible should be true. It could be hidden before the coroutine finished.
            animator.enabled = false;
            if (callback != null)
                callback();

            if (before == _animationCoroutine)
            {
                // Didn't change curing coroutine
                _animationCoroutine = null;
            }
        }
    }
}