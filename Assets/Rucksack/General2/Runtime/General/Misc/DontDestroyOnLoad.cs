using System;
using UnityEngine;

namespace Devdog.General2
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        protected void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}
