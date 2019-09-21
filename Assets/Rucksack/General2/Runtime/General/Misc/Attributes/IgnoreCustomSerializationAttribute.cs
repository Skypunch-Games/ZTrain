using System;
using System.Collections.Generic;

namespace Devdog.General2
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreCustomSerializationAttribute : Attribute
    {

    }
}
