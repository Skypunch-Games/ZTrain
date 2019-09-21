using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    public partial class Character : MonoBehaviour
    {
        /// <summary>
        /// The trigger this Character is currently using. Null if the Character is not using a trigger.
        /// </summary>
        public ITrigger currentTrigger { get; set; }
        protected List<ITrigger> triggersInRange = new List<ITrigger>();

        
        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void OnDestroy()
        {
            
        }

        public IEnumerable<ITrigger> GetInRangeTriggers()
        {
            return triggersInRange;
        }

        public bool IsInRangeOfTrigger(ITrigger trigger)
        {
            return triggersInRange.Contains(trigger);
        }

        public void NotifyCameIntoTriggerRange(ITrigger trigger)
        {
            triggersInRange.Add(trigger);
        }

        public void NotifyWentOutOfTriggerRange(ITrigger trigger)
        {
            triggersInRange.Remove(trigger);
        }
    }
}