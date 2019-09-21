using UnityEngine;

namespace Devdog.General2
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TriggerColliderUse : MonoBehaviour, ITriggerColliderUse
    {
        [SerializeField]
        private bool _onlyForPlayers = true;

        private ITrigger _trigger;
        private void Awake()
        {
            _trigger = GetComponentInParent<ITrigger>();
        }
        
        private void Start()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;

            var rigid = GetComponent<Rigidbody>();
            rigid.useGravity = false;
            rigid.isKinematic = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_onlyForPlayers)
            {
                var player = other.GetComponent<Player>();
                if (player != null)
                {
                    _trigger.Server_Use(player);
                }
            }
            else
            {
                var character = other.GetComponent<Character>();
                if (character != null)
                {
                    _trigger.Server_Use(character);
                }
            }
        }
    }
}