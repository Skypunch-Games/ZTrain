using System;

namespace Devdog.General2.Localization
{
    public interface ILocalizedObject<T> : ILocalizedObject 
        where T: UnityEngine.Object
    {

        T val { get; set; }

    }

    public interface ILocalizedObject
    {
        UnityEngine.Object objectVal { get; set; }
    }
}
