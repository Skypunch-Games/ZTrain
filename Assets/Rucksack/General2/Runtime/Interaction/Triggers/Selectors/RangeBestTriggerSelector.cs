using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    [CreateAssetMenu(menuName = "Devdog/Range trigger selector")]
    public class RangeBestTriggerSelector : BestTriggerSelectorBase
    {
        /// <summary>
        /// Item infront has 20% effect on making the best decision
        /// </summary>
        public float forwardEffect = 0.2f;

        /// <summary>
        /// Get the best trigger based on distance and if it's in front of the player or not.
        /// </summary>
        public override ITrigger GetBestTrigger(Character character, IEnumerable<ITrigger> triggersInRange)
        {
            float bestCheck = -999.0f;
            ITrigger closestTrigger = null;
            foreach (var trigger in triggersInRange)
            {
                var behaviour = trigger as Behaviour;
                if (behaviour == null || behaviour.enabled == false || behaviour.gameObject.activeSelf == false)
                {
                    continue;
                }

                var toPlayerVec = behaviour.transform.position - character.transform.position;
                var dist = Vector3.Magnitude(toPlayerVec);

                float inFrontFactor = Mathf.Clamp01(Vector3.Dot(character.transform.forward, toPlayerVec / dist));
                inFrontFactor *= forwardEffect; 

                float final = (trigger.rangeHandler.useRange - dist) * inFrontFactor;
                final = Mathf.Abs(final);

                if (final > bestCheck)
                {
                    closestTrigger = trigger;
                    bestCheck = final;
                }
            }

            return closestTrigger;
        }
    }
}