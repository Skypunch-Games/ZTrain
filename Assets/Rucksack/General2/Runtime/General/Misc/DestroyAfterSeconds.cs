using System;
using UnityEngine;

namespace Devdog.General2
{
    public class DestroyAfterSeconds : MonoBehaviour
    {
        public float seconds = 1f;

        protected void Awake()
        {
            Destroy(gameObject, seconds);
        }
    }
}
