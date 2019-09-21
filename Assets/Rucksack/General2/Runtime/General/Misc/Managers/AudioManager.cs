using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Devdog.General2
{
    public partial class AudioManager : ManagerBase<AudioManager>
    {
        [SerializeField]
        private int _reserveAudioSources = 8;

        [SerializeField]
        private AudioMixerGroup _audioMixerGroup;
        
        private static AudioSource[] _audioSources;
        private static GameObject _audioSourceGameObject;
        private static Queue<AudioClipInfo> _audioQueue = new Queue<AudioClipInfo>();

        protected override void Awake()
        {
            base.Awake();

            StartCoroutine(WaitFramesAndEnable(5));
            enabled = false; // Set to enabled at start, initialize, then enable (to avoid playing sound during initialization)

            _audioQueue = new Queue<AudioClipInfo>(_reserveAudioSources);
            CreateAudioSourcePool();
        }

        protected override void Start()
        {
            base.Start();
        }

        // Empty method to show enable / disable icons in Unity inspector.
        private void OnEnable(){ }

        private IEnumerator WaitFramesAndEnable(int frames)
        {
            for(int i = 0; i < frames; i++)
            {
                yield return null;
            }

            enabled = true;
        }

        private void CreateAudioSourcePool()
        {
            _audioSources = new AudioSource[_reserveAudioSources];

            _audioSourceGameObject = new GameObject("_AudioSources");
            _audioSourceGameObject.transform.SetParent(transform);
            _audioSourceGameObject.transform.localPosition = Vector3.zero;

            for (int i = 0; i < _audioSources.Length; i++)
            {
                _audioSources[i] = _audioSourceGameObject.AddComponent<AudioSource>();
                _audioSources[i].outputAudioMixerGroup = _audioMixerGroup;
            }
        }

        protected void Update()
        {
            if (_audioQueue.Count > 0)
            {
                var source = GetNextAudioSource();
                var clip = _audioQueue.Dequeue();

                source.Play(clip);
            }
        }

        private static AudioSource GetNextAudioSource()
        {
            foreach (var audioSource in _audioSources)
            {
                if (audioSource.isPlaying == false)
                    return audioSource;
            }

            DevdogLogger.LogWarning("All sources taken, can't play audio clip...");
            return null;
        }


        /// <summary>
        /// Plays an audio clip, only use this for the UI, it is not pooled so performance isn't superb.
        /// </summary>
        public static void AudioPlayOneShot(AudioClipInfo clip)
        {
            if (clip == null || clip.audioClip == null)
            {
                return;
            }

            if (instance == null)
            {
                DevdogLogger.LogWarning("AudioManager not found, yet trying to play an audio clip....");
            }

            if (_audioQueue.Any(o => o.audioClip == clip.audioClip) == false)
            {
                _audioQueue.Enqueue(clip);
            }
        }
    }
}