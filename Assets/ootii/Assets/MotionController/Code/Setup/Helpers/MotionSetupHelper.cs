using UnityEngine;
using com.ootii.Actors.AnimationControllers;

#if UNITY_EDITOR

#endif

namespace com.ootii.Setup
{
    public class MotionSetupHelper
    {
#if UNITY_EDITOR
        /// <summary>
        /// Ensures that the Motion Controller has the correct number of Motion Layers, with an Empty Motion 
        /// on each layer above the base
        /// </summary>
        /// <param name="rMotionController"></param>                
        /// <param name="rLastLayerIndex"></param>
        public static void EnsureMotionLayers(MotionController rMotionController, int rLastLayerIndex = 1)
        {
            if (rLastLayerIndex < 0) { rLastLayerIndex = 0; }
            if (rLastLayerIndex > EnumMotionLayer.UPPER_ADDITIVE) { rLastLayerIndex = EnumMotionLayer.UPPER_ADDITIVE; }

            if (rMotionController == null) { return; }

            for (int i = 0; i <= rLastLayerIndex; i++)
            {
                if (rMotionController.MotionLayers.Count < (i + 1))
                {
                    rMotionController.MotionLayers.Add(new MotionControllerLayer());
                }
            }

            // Configure the motion layers
            for (int i = 0; i < rMotionController.MotionLayers.Count; i++)
            {                
                rMotionController.MotionLayers[i].Name = EnumMotionLayer.Names[i];
                // Ignore Motion Override on the extended layers
                rMotionController.MotionLayers[i].IgnoreMotionOverride = i > EnumMotionLayer.UPPER_BODY;

                // Ensure there is an Empty Motion on every Layer above the Base Layer
                if (i > 0) { CreateEmptyMotion(rMotionController, i); }
            }
            
            // Initialize the motions on each Layer
            if (Application.isPlaying) { return; }

            for (int i = 0; i < rMotionController.MotionLayers.Count; i++)
            {
                rMotionController.MotionLayers[i].AnimatorLayerIndex = i;
                rMotionController.MotionLayers[i].MotionController = rMotionController;
                rMotionController.MotionLayers[i].InstanciateMotions();
            }
        }

        /// <summary>
        /// Ensures that the Motion Controller has the correct number of Motion Layers, with an Empty Motion 
        /// on each layer above the base
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rUseHandPoses">If true, configures extended motion layers 2 through 4; if false, only configures layers 0 and 1</param>
        public static void EnsureMotionLayers(MotionController rMotionController, bool rUseHandPoses)
        {
            EnsureMotionLayers(rMotionController, rUseHandPoses ? EnumMotionLayer.RIGHT_HAND : EnumMotionLayer.UPPER_BODY);
        }

        /// <summary>
        /// Create an Empty motion on the specified animator layer
        /// </summary>
        /// <param name="rMotionController">The Motion Controller to which the animator is attached</param>
        /// <param name="rLayerIndex">The layer on which to create the Empty motion</param>
        public static void CreateEmptyMotion(MotionController rMotionController, int rLayerIndex)
        {
            Empty lEmpty = rMotionController.GetMotion<Empty>(rLayerIndex, true);
            if (lEmpty == null) { rMotionController.CreateMotion<Empty>(rLayerIndex); }
        }

        /// <summary>
        /// Create and configure a new motion layer
        /// </summary>
        /// <param name="rMotionController"></param>
        /// <param name="rLayerIndex"></param>
        /// <param name="rLayerName"></param>
        /// <param name="rIgnoreOverride"></param>
        /// <returns></returns>
        public static MotionControllerLayer CreateMotionLayer(MotionController rMotionController, int rLayerIndex, string rLayerName, bool rIgnoreOverride = false)
        {
            if (rMotionController.MotionLayers.Count < rLayerIndex + 1)
            {
                rMotionController.MotionLayers.Add(new MotionControllerLayer());
            }

            // Create an empty motion on the new layer
            Empty lEmpty = rMotionController.GetMotion<Empty>(rLayerIndex);
            if (lEmpty == null) { rMotionController.CreateMotion<Empty>(rLayerIndex); }


            rMotionController.MotionLayers[rLayerIndex].Name = rLayerName;
            rMotionController.MotionLayers[rLayerIndex].IgnoreMotionOverride = rIgnoreOverride;

            return rMotionController.MotionLayers[rLayerIndex];
        }
#endif
    }
}

