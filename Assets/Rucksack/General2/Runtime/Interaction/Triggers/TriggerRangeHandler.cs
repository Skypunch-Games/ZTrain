using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Devdog.General2
{
    public partial class TriggerRangeHandler : MonoBehaviour, ITriggerRangeHandler
    {
        [SerializeField]
        private float _useRange = 10f;
        public float useRange
        {
            get { return _useRange; }
            set
            {
                _useRange = value;
                _sphereCollider.radius = _useRange;
            }
        }

        private readonly List<Character> _charactersInRange = new List<Character>();
        private SphereCollider _sphereCollider;
        private Rigidbody _rigidbody;
        private ITrigger _trigger;

        protected void Awake()
        {
            _trigger = GetComponentInParent<ITrigger>();
            Assert.IsNotNull(_trigger, "TriggerRangeHandler used but no TriggerBase found on object.");

            _rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;

            _sphereCollider = gameObject.GetOrAddComponent<SphereCollider>();
            _sphereCollider.isTrigger = true;
            _sphereCollider.radius = useRange;

            gameObject.layer = 2; // Ignore raycasts
        }

        protected void OnDestroy()
        {
            foreach (var character in _charactersInRange)
            {
                if (character != null)
                {
                    character.NotifyWentOutOfTriggerRange(_trigger);
                }
            }
        }
        
        protected void OnTriggerEnter(Collider other)
        {
            var character = other.gameObject.GetComponent<Character>();
            if (character != null)
            {
                // Check just in case the player object has more than 1 collider.
                if (_charactersInRange.Contains(character) == false)
                {
                    _charactersInRange.Add(character);
                    character.NotifyCameIntoTriggerRange(_trigger);
                }
            }   
        }

        protected void OnTriggerExit(Collider other)
        {
            var character = other.gameObject.GetComponent<Character>();
            if (character != null)
            {
                // Check just in case the player object has more than 1 collider.
                if (_charactersInRange.Contains(character))
                {
                    character.NotifyWentOutOfTriggerRange(_trigger);
                    _trigger.UnUse(character);
                
                    _charactersInRange.Remove(character);   
                }
            }
        }

        public IEnumerable<Character> GetCharactersInRange()
        {
            return _charactersInRange;
        }

        public bool IsCharacterInRange(Character character)
        {
            return _charactersInRange.Contains(character);
        }
    }
}