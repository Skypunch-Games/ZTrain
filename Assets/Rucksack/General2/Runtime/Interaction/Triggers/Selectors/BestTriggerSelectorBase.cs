using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    public abstract class BestTriggerSelectorBase : ScriptableObject
    {
        public abstract ITrigger GetBestTrigger(Character character, IEnumerable<ITrigger> triggersInRange);
    }
}
