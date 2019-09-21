#if UNITY_2017 || UNITY_2018

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Devdog.General2
{
    public sealed class UNetDestroyComponents : MonoBehaviour
    {
        private enum Type
        {
            Client,
            Server,
            Both
        }

        private enum Authority
        {
            Authority,
            NoAuthority,
            IsLocalPlayer,
            IsNotLocalPlayer,
        }


        [SerializeField]
        private Type _ifType;

        [SerializeField]
        private Authority _ifAuthority;
        
        [SerializeField]
        private UnityEngine.Component[] _toDestroy = new UnityEngine.Component[0];
        
        [SerializeField]
        private GameObject[] _toDestroyGameObjects = new GameObject[0];

        private NetworkIdentity _identity;

        private void Awake()
        {
            _identity = GetComponent<NetworkIdentity>();
        }
        
        private void Start()
        {
            DestroyIfType();
        }

        private void DestroyIfType()
        {
            switch (_ifType)
            {
                case Type.Both:
                {
                    DestroyIfAuthority();
                    break;
                }
                case Type.Client:
                {
                    if (_identity.isClient)
                    {
                        DestroyIfAuthority();
                    }
                    break;
                }
                case Type.Server:
                {
                    if (_identity.isServer)
                    {
                        DestroyIfAuthority();
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DestroyIfAuthority()
        {
            switch (_ifAuthority)
            {
                case Authority.Authority:
                {
                    if (_identity.hasAuthority)
                    {
                        Destroy();
                    }
                    break;
                }
                case Authority.NoAuthority:
                {
                    if (_identity.hasAuthority == false)
                    {
                        Destroy();
                    }
                    break;
                }
                case Authority.IsLocalPlayer:
                {
                    if (_identity.isLocalPlayer)
                    {
                        Destroy();
                    }
                    break;
                }
                case Authority.IsNotLocalPlayer:
                {
                    if (_identity.isLocalPlayer == false)
                    {
                        Destroy();
                    }
                    break;
                }
                 
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Destroy()
        {
            foreach (var component in _toDestroy)
            {
                Destroy(component);
            }

            foreach (var destroyGameObject in _toDestroyGameObjects)
            {
                Destroy(destroyGameObject);
            }
        }
    }
}

#endif