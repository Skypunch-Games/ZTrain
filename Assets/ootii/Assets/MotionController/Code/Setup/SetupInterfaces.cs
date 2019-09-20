using System;

namespace com.ootii.Setup
{
    /// <summary>
    /// Creates and configures an Animator Controller
    /// </summary>
    public interface IConfigureAnimator
    {
        void ConfigureAnimator();
    }

    /// <summary>
    /// Configures Input Manager settings
    /// </summary>
    public interface IConfigureInput
    {
        void ConfigureInput();

        bool CreateInputAliases { get; set; }
    }

    /// <summary>
    /// Creates and configures Motions on the Motion Controller
    /// </summary>
    public interface IConfigureMotions
    {
        void ConfigureMotions();
    }

    /// <summary>
    /// Creates and configures other components (Basic Attributes, Basic Inventory, etc)
    /// </summary>
    public interface IConfigureComponents
    {
        void ConfigureComponents();
    }

    /// <summary>
    /// Creates and configures scene objects (camera, UI, etc)
    /// </summary>
    public interface IConfigureSceneObjects
    {
        void ConfigureSceneObjects();
    }

    /// <summary>
    /// Indicates that the implementing class requires a specific component
    /// </summary>
    public interface IUsesComponent
    {
        bool UsesComponent(Type rType);
    }

    /// <summary>
    /// Indicates that the implementing class distinguishes between setting up a Player vs an NPC
    /// </summary>
    public interface IIsPlayer
    {
        bool IsPlayer { get; set; }
    }

    /// <summary>
    /// Denotes an object that is serialized using the custom JSON serialization; such objects will
    /// contain their own Inspector GUI code (contained within #if UNITY_EDITOR ... #endif directives),
    /// as the standard pattern for creating custom Editors won't work for them.
    /// </summary>
    public interface ISerializableObject
    {
        string Serialize();
        void Deserialize(string rDefinition);

        bool OnInspectorGUI(UnityEngine.Object rTarget);
    }    
}

