using System;
using System.IO;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Actors.Inventory;
using com.ootii.Geometry;
using UnityEditor;

namespace com.ootii.Setup.Modules
{
    public abstract class MotionPackSetupModule : SetupModule
    {
        /// <summary>
        /// Apply the pack's input settings to the Input Manager?
        /// </summary>
        public bool _CreateInputAliases = true;
        public virtual bool CreateInputAliases
        {
            get { return _CreateInputAliases; }
            set { _CreateInputAliases = value; }
        }
     
        /// <summary>
        /// The default movement style used by the pack.
        /// Generally, 0 = Adventure, 1 = Shooter, 2 = MMO-style
        /// </summary>
        public int _MovementStyle = 0;
        public virtual int MovementStyle
        {
            get { return _MovementStyle; }
            set { _MovementStyle = value; }
        }

        /// <summary>
        /// Custom path to the pack's animation files
        /// </summary>        
        public virtual string AnimationPath { get; set; }        

        public virtual BasicInventory CreateInventory(MotionController rMotionController)
        {
            BasicInventory lInventory = rMotionController.GetOrAddComponent<BasicInventory>();
            CharacterSetupHelper.CreateStandardInventorySlots(lInventory);

            return lInventory;
        }

       

    }
}
