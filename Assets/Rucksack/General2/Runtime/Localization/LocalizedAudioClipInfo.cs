using System;
using Devdog.General2.Localization;
using UnityEngine;

namespace Devdog.General2.Localization
{
    [Serializable]
    public class LocalizedAudioClipInfo
    {
        public LocalizedAudioClip audioClip;
        public float volume = 1f;
        public float pitch = 1f;
        public bool loop = false;
    }
}
