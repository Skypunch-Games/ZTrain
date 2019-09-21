using System;
using UnityEngine;

namespace Devdog.General2
{
    [Serializable]
    public class AudioClipInfo
    {
        public AudioClip audioClip;
        public float volume = 1f;
        public float pitch = 1f;
        public bool loop = false;
    }
}
