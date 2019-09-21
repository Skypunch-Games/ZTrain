using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    public interface IPoolable
    {
        GameObject gameObject { get; }
        Transform transform { get; }

        void ResetStateForPool();
    }
}
