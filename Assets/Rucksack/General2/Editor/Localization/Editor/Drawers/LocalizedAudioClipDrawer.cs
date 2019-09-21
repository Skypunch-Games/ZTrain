using System;
using System.Reflection;
using Devdog.General2.Editors.ReflectionDrawers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Devdog.General2.Localization.Editors
{
    [CustomDrawer(typeof(LocalizedAudioClip))]
    public class LocalizedAudioClipDrawer : LocalizedObjectDrawerBase<AudioClip, LocalizedAudioClip>
    {
        public LocalizedAudioClipDrawer(FieldInfo fieldInfo, object value, object parentValue, int arrayIndex)
            : base(fieldInfo, value, parentValue, arrayIndex)
        {
        }

        public override DrawerBase FindDrawerRelative(string fieldName)
        {
            return null;
        }
    }
}
