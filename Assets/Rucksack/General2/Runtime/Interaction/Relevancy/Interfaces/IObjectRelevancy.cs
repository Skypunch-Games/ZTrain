using System;
using UnityEngine;

namespace Devdog.General2
{
    /// <summary>
    /// Used to detect wether an object is relevant to the game or not. 
    /// When not relevant performance can be saved to disable or delete it.
    /// </summary>
    public interface IObjectRelevancy
    {
        event Action<Character> OnBecameRelevant;
        event Action<Character> OnBecameIrrelevant;

        bool IsRelevant(Character character);
    }
}
