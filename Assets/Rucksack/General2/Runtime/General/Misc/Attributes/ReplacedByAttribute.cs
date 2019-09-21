using System;

namespace Devdog.General2
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ReplacedByAttribute : Attribute
    {
        public Type type { get; protected set; }

        public ReplacedByAttribute(Type type)
        {
            this.type = type;
        }
    }
}
