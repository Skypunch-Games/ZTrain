using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devdog.General2.Editors.ReflectionDrawers
{
    public interface IChildrenDrawer
    {
        List<DrawerBase> children { get; set; }
    }
}
