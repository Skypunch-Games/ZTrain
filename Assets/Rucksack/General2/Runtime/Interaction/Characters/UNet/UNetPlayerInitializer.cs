#if UNITY_2017 || UNITY_2018

using UnityEngine;
using UnityEngine.Networking;

namespace Devdog.General2
{
    [RequireComponent(typeof(Player))]
    public sealed class UNetPlayerInitializer : MonoBehaviour
    {
        private void Start()
        {
            var identity = GetComponent<NetworkIdentity>();
            if (identity != null)
            {
                if (identity.isClient && identity.isLocalPlayer)
                {
                    GetComponent<Player>().RegisterPlayerAsCurrentPlayer();
                }
            }
        }
    }
}

#endif