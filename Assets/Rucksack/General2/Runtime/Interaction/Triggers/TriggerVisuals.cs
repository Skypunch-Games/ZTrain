using UnityEngine;

namespace Devdog.General2
{
    [RequireComponent(typeof(Animator))]
    public class TriggerVisuals : MonoBehaviour, ITriggerCallbacks
    {
        [SerializeField]
        private MotionInfo _useAnimation;

        [SerializeField]
        private AudioClipInfo _useAudioClip;
        
        [SerializeField]
        private MotionInfo _unUseAnimation;
        
        [SerializeField]
        private AudioClipInfo _unUseAudioClip;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void DoVisuals()
        {
            if (_useAnimation.motion != null && _animator != null)
            {
                _animator.Play(_useAnimation);
            }

            AudioManager.AudioPlayOneShot(_useAudioClip);
        }

        public void UndoVisuals()
        {
            if (_unUseAnimation.motion != null && _animator != null)
            {
                _animator.Play(_unUseAnimation);
            }

            AudioManager.AudioPlayOneShot(_unUseAudioClip);
        }

        public void OnTriggerUsed(Character character, TriggerEventData eventData)
        {
            DoVisuals();
        }

        public void OnTriggerUnUsed(Character character, TriggerEventData eventData)
        {
            UndoVisuals();
        }
    }
}