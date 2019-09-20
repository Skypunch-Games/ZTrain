using UnityEngine;
using com.ootii.Game;
using com.ootii.Geometry;

namespace com.ootii.Setup
{
    /// <summary>
    /// Helper functions for setting up a scene
    /// </summary>
    public class SceneSetupHelper 
    {
#if UNITY_EDITOR
        /// <summary>
        /// Create and configure the Game Core
        /// </summary>
        /// <param name="rInputSourceOwner"></param>
        /// <returns></returns>
        public static GameCore ConfigureGameCore(GameObject rInputSourceOwner)
        {
            // Find the GameCore
            GameCore lGameCore = Object.FindObjectOfType<GameCore>();
            if (lGameCore == null)
            {
                GameObject lGameCoreObject = new GameObject("Game Core");
                lGameCoreObject.transform.Reset();
                lGameCore = lGameCoreObject.AddComponent<GameCore>();
            }

            // Set the input source
            lGameCore.InputSourceOwner = rInputSourceOwner;
            

            return lGameCore;
        }
#endif
    }
}


