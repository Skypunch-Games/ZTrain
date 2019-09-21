using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class ObjectDistanceRelevancy : MonoBehaviour, IObjectRelevancy
    {
        public event Action<Character> OnBecameRelevant;
        public event Action<Character> OnBecameIrrelevant;

        private readonly List<Character> _inRange = new List<Character>();

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            var s = gameObject.GetOrAddComponent<SphereCollider>();
            s.radius = Mathf.Max(10f, s.radius);
            s.isTrigger = true;
            s.gameObject.layer = 2;
        }

        public bool IsRelevant(Character character)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return true; // in-editor we always are considered relevant.
            }
#endif

            return _inRange.Contains(character);
        }

        private void OnTriggerEnter(Collider other)
        {
            DoOnTriggerEnter(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            DoOnTriggerEnter(other.gameObject);
        }

        private void DoOnTriggerEnter(GameObject obj)
        {
            var player = obj.GetComponent<Player>();
            if (player != null)
            {
                _inRange.Add(player);
                if (OnBecameRelevant != null)
                {
                    OnBecameRelevant(player);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            DoOnTriggerExit(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            DoOnTriggerExit(other.gameObject);
        }

        private void DoOnTriggerExit(GameObject obj)
        {
            var player = obj.GetComponent<Player>();
            if (player != null)
            {
                if (OnBecameIrrelevant != null)
                {
                    OnBecameIrrelevant(player);
                }
            }
        }
    }
}
