using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    [CreateAssetMenu(menuName = "Devdog/Raycast trigger selector")]
    public class RaycastBestTriggerSelector : BestTriggerSelectorBase
    {
        public LayerMask layerMask = -1;
        public float raycastDistance = 10f;

        /// <summary>
        /// Get the best trigger based on distance and if it's in front of the player or not.
        /// </summary>
        public override ITrigger GetBestTrigger(Character character, IEnumerable<ITrigger> triggersInRange)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return null;
            }

#if UNITY_EDITOR
            // Raycast from center of screen
            Debug.DrawRay(camera.transform.position, (camera.transform.forward * raycastDistance), Color.red, 0.2f);
#endif
            
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, raycastDistance, layerMask))
            {
                var trigger = hit.transform.GetComponent<ITrigger>();
                var behaviour = trigger as Behaviour;
                if (behaviour == null || behaviour.enabled == false || behaviour.gameObject.activeSelf == false)
                {
                    return null;
                }
                
                return trigger;
            }

            return null;
        }
    }
}