using System;
using System.Collections.Generic;

namespace Devdog.General2
{
    public interface ITriggerCallbacks
    {
        void OnTriggerUsed(Character character, TriggerEventData eventData);
        void OnTriggerUnUsed(Character character, TriggerEventData eventData);
    }
}
