using System;
using System.Collections.Generic;

namespace Devdog.General2
{
    public interface ITriggerInputHandler
    {
        /// <summary>
        /// The action name to trigger this trigger.
        /// </summary>
        TriggerActionInfo actionInfo { get; }

        bool AreKeysDown();
        void Use(Character character);
    }
}
