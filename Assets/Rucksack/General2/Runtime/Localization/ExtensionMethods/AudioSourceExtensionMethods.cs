using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.General2.Localization;

namespace Devdog.General2
{
    public static partial class AudioSourceExtensionMethods
    {
        public static void Play(this AudioSource source, LocalizedAudioClipInfo info)
        {
            source.volume = info.volume;
            source.pitch = info.pitch;
            source.loop = info.loop;
            source.clip = info.audioClip.val;
            source.Play();
        }
    }
}