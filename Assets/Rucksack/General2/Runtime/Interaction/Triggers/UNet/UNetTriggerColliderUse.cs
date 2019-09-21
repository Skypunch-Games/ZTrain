#if UNITY_2017 || UNITY_2018

using UnityEngine;
using UnityEngine.Networking;

namespace Devdog.General2
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class UNetTriggerColliderUse : MonoBehaviour, ITriggerColliderUse
    {
        [SerializeField]
        private bool _onlyForPlayers = true;

        private ITrigger _trigger;
        private NetworkIdentity _identity;
        private void Awake()
        {
            _trigger = GetComponentInParent<ITrigger>();
            _identity = GetComponentInParent<NetworkIdentity>();
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
            if (_identity.isServer == false)
            {
                return;
            }
            
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

#endif