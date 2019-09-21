using System;
using System.Collections.Generic;

namespace Devdog.General2
{
    [AttributeUsage(AttributeTargets.All)]
    public class CategoryAttribute : Attribute
    {
        public string category { get; private set; }

        public CategoryAttribute(string category)
        {
            this.category = category;
        }

        public override string ToString()
        {
            return category;
        }
    }
}
