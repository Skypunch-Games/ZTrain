using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Devdog.General2.Editors.ReflectionDrawers
{
    public class ValueTypeDrawer : ChildrenValueDrawerBase
    {
        public ValueTypeDrawer(FieldInfo fieldInfo, object value, object parentValue, int arrayIndex)
            : base(fieldInfo, value, parentValue, arrayIndex)
        {

        }
    }
}
