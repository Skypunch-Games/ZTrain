#pragma warning disable 0067

using System;
using UnityEngine;

namespace Devdog.General2
{
    public sealed class ObjectNullRelevancy : IObjectRelevancy
    {
        public event Action<Character> OnBecameRelevant;
        public event Action<Character> OnBecameIrrelevant;

        public bool IsRelevant(Character character)
        {
            return true;
        }
    }
}
