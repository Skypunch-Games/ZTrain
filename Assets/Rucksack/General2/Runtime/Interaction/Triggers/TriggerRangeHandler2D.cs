using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Devdog.General2
{
    public partial class TriggerRangeHandler2D : MonoBehaviour, ITriggerRangeHandler
    {
        [SerializeField]
        private float _useRange = 10f;
        public float useRange
        {
            get { return _useRange; }
        }

        private readonly List<Character> _charactersInRange = new List<Character>();
        private CircleCollider2D _circleCollider;
        private Rigidbody2D _rigidbody2D;
        private ITrigger _trigger;

        protected void Awake()
        {
            _trigger = GetComponentInParent<ITrigger>();
            Assert.IsNotNull(_trigger, "TriggerRangeHandler used but no ITrigger found on object.");

            _rigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
            _rigidbody2D.isKinematic = true;

            _circleCollider = gameObject.GetOrAddComponent<CircleCollider2D>();
            _circleCollider.isTrigger = true;
            _circleCollider.radius = useRange;

            gameObject.layer = 2; // Ignore raycasts
        }

        public IEnumerable<Character> GetCharactersInRange()
        {
            return _charactersInRange;
        }

        public bool IsCharacterInRange(Character character)
        {
            return _charactersInRange.Contains(character);
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            var player2D = other.gameObject.GetComponent<Player2D>();
            if (player2D != null)
            {
                // Check just in case the player object has more than 1 collider.
                if (_charactersInRange.Contains(player2D) == false)
                {
                    _charactersInRange.Add(player2D);
                    player2D.NotifyCameIntoTriggerRange(_trigger);
                }
            }   
        }

        protected void OnTriggerExit2D(Collider2D other)
        {
            var character = other.gameObject.GetComponent<Player2D>();
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
    }
}