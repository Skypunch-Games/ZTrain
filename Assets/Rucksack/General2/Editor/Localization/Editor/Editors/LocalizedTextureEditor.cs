using System;
using UnityEditor;
using UnityEngine;

namespace Devdog.General2.Localization.Editors
{
    [CustomPropertyDrawer(typeof(LocalizedTexture2D), true)]
    public class LocalizedTextureEditor : LocalizedObjectEditorBase<Texture2D, LocalizedTexture2D>
    {
        
    }
}
