using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Devdog.General2.Localization.Editors
{
    [CustomPropertyDrawer(typeof(LocalizedAudioClip), true)]
    public class LocalizedAudioClipEditor : LocalizedObjectEditorBase<AudioClip, LocalizedAudioClip>
    {
        
    }
}
