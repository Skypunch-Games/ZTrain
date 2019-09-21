#if UNITY_2017 || UNITY_2018

using UnityEngine.Networking;

namespace Devdog.General2
{
    public sealed class RequestUseTriggerMessage : MessageBase
    {
//        public byte[] triggerGuidBytes;
        public NetworkIdentity triggerIdentity;
    }
}

#endif