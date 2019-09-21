using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    public static partial class AudioSourceExtensionMethods
    {
        public static void Play(this AudioSource source, AudioClipInfo info)
        {
            source.volume = info.volume;
            source.pitch = info.pitch;
            source.loop = info.loop;
            source.clip = info.audioClip;
            source.Play();
        }
    }
}
