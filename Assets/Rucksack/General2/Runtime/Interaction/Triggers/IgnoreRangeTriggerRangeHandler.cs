using UnityEngine;
using System.Collections.Generic;

namespace Devdog.General2
{
    public sealed class IgnoreRangeTriggerRangeHandler : MonoBehaviour, ITriggerRangeHandler
    {
        public float useRange
        {
            get { return 9999f; }
        }

        public IEnumerable<Character> GetCharactersInRange()
        {
            return new Character[0];
        }

        public bool IsCharacterInRange(Character character)
        {
            return true;
        }
    }
}