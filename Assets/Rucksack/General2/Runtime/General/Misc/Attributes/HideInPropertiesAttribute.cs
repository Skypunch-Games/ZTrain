using System;
using UnityEngine;

namespace Devdog.General2
{
    /// <summary>
    /// When used this field will not be displayed in the properties sidebar of the node editor.
    /// </summary>
    public class HideInPropertiesAttribute : PropertyAttribute
    {

        public HideInPropertiesAttribute()
        {
            
        }
    }
}
