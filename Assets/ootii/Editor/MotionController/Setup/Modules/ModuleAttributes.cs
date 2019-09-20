using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;
using UnityEngine;

namespace com.ootii.Setup.Modules
{
    /// <summary>
    /// The name of the Setup Module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModuleNameAttribute : Attribute
    {
        /// <summary>
        /// Name of the setup module
        /// </summary>
        protected string mValue;
        public string Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue"></param>
        public ModuleNameAttribute(string rValue)
        {
            mValue = rValue;
        }

        /// <summary>
        /// Attempts to find a fiendly name. If found, returns it.
        /// </summary>
        /// <param name="rType">Type whose friendly name we want</param>
        /// <returns>String that is the friendly name for class name</returns>
        public static string GetName(Type rType)
        {            
            var lAttribute = ReflectionHelper.GetAttribute<ModuleNameAttribute>(rType);

            return (lAttribute != null)
                ? lAttribute.Value
                : rType.ToString();            
        }
    }

    /// <summary>
    /// The category to which the Setup Module belongs to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleCategoryAttribute : Attribute
    {
        /// <summary>
        /// Name of the setup module
        /// </summary>
        protected string mValue;
        public string Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue"></param>
        public ModuleCategoryAttribute(string rValue)
        {
            mValue = rValue;
        }

        /// <summary>
        /// Attempts to find a fiendly name. If found, returns it.
        /// </summary>
        /// <param name="rType">Type whose friendly name we want</param>
        /// <returns>String that is the friendly name for class name</returns>
        public static string GetCategory(Type rType)
        {
            var lAttribute = ReflectionHelper.GetAttribute<ModuleCategoryAttribute>(rType);

            return (lAttribute != null)
                ? lAttribute.Value
                : string.Empty;
        }
    }

    /// <summary>
    /// Description of the Setup Module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModuleDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Description of the module
        /// </summary>
        protected string mValue;
        public string Value
        {
            get { return mValue; }
        }

        /// <summary>
        /// Constructor for the attribute
        /// </summary>
        /// <param name="rValue">Value that is the tooltip</param>
        public ModuleDescriptionAttribute(string rValue)
        {
            mValue = rValue;
        }

        /// <summary>
        /// Attempts to find a fiendly description. If found, returns it.
        /// </summary>
        /// <param name="rType">Type whose friendly name we want</param>
        /// <returns>String that is the friendly name for class name</returns>
        public static string GetDescription(Type rType)
        {
            var lAttribute = ReflectionHelper.GetAttribute<ModuleDescriptionAttribute>(rType);

            return (lAttribute != null)
                ? lAttribute.Value
                : string.Empty;
        }
    }

    /// <summary>
    /// Indicates that a Setup Module is dependent upon another Setup Module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleRequiresAttribute : Attribute
    {       
        protected Type mValue;
        public Type Value { get { return mValue; } }

        public ModuleRequiresAttribute(Type rValue)
        {
            mValue = rValue;
        }

        public static List<Type> GetRequiredTypes(Type rType)
        {
            var lTypes = new List<Type>();
            var lAttributes = ReflectionHelper.GetAttributes<ModuleRequiresAttribute>(rType);
            if (lAttributes != null)
            {
                lTypes.AddRange(from lAttribute in lAttributes
                                where lAttribute != null
                                select lAttribute.Value);
            }

            return lTypes;
        }

        public static Type GetRequired(Type rType)
        {            
            var lAttribute = ReflectionHelper.GetAttribute<ModuleRequiresAttribute>(rType);
            return (lAttribute != null)
                ? lAttribute.Value
                : null;
        }
    }

    /// <summary>
    /// Indicates that a Setup Module requires a component to be set up on the character
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ModuleUsesComponentAttribute : Attribute
    {
        protected Type mValue;
        public Type Value { get { return mValue; } }

        public ModuleUsesComponentAttribute(Type rValue)
        {
            mValue = rValue;
        }

        public static List<Type> GetComponentTypes(Type rType)
        {
            var lTypes = new List<Type>();

            var lAttributes = ReflectionHelper.GetAttributes<ModuleUsesComponentAttribute>(rType);
            if (lAttributes != null)
            {
                lTypes.AddRange(from lAttribute in lAttributes
                                where lAttribute != null
                                select lAttribute.Value);
            }

            return lTypes;
        }
    }

    /// <summary>
    /// The highest-indexed Animator layer used by the module 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModuleLastAnimatorLayer : Attribute
    {
        protected int mValue;
        public int Value { get { return mValue; } }

        public ModuleLastAnimatorLayer(int rValue)
        {
            mValue = rValue;
        }

        public static int GetLayer(Type rType)
        {
            var lAttribute = ReflectionHelper.GetAttribute<ModuleLastAnimatorLayer>(rType);
            return lAttribute != null ? lAttribute.Value : EnumMotionLayer.UPPER_BODY;            
        }
    }

    /// <summary>
    /// The priority of the module (0 = lowest priority, 100 = highest priority)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModulePriorityAttribute : Attribute
    {       
        protected int mValue;
        public int Value { get { return mValue; } }

        public ModulePriorityAttribute(int rValue)
        {
            mValue = Mathf.Clamp(rValue, 0, 100);
        }

        public static int GetPriority(Type rType)
        {
            var lAttribute = ReflectionHelper.GetAttribute<ModulePriorityAttribute>(rType);
            return lAttribute != null ? lAttribute.Value : SetupModule.DefaultPriority;
        }
    }

    /// <summary>
    /// Used to hide a module when displaying a list of items in the Inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HideModuleAttribute : Attribute
    {
        protected bool mValue;
        public bool Value { get { return mValue; } }

        public HideModuleAttribute(bool rValue)
        {
            mValue = rValue;            
        }

        public static bool GetHidden(Type rType)
        {
            var lAttribute = ReflectionHelper.GetAttribute<HideModuleAttribute>(rType);

            // Default to showing items, so the attribute is really only needed when we want to hide an item
            return (lAttribute != null) && lAttribute.Value;
            
        }
    }  
}
