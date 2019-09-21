using System;
using UnityEditor;

namespace Devdog.General2.Localization.Editors
{
    [CustomPropertyDrawer(typeof(LocalizedObject), true)]
    public class LocalizedUnityEngineObjectEditor : LocalizedObjectEditorBase<UnityEngine.Object, LocalizedObject>
    {
        
    }
}
