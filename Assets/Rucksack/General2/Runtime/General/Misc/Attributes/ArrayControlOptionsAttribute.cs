using System;
using System.Collections.Generic;

namespace Devdog.General2
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ArrayControlOptionsAttribute : Attribute
    {
//        public bool includeArrayChildren { get; protected set; }
        public bool canRemoveItems = true;
        public bool canAddItems = true;

        public ArrayControlOptionsAttribute()
        {
        }
    }
}
