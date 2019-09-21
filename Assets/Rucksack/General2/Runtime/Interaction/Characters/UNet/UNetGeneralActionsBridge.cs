#if UNITY_2017 || UNITY_2018

using UnityEngine.Networking;

namespace Devdog.General2
{
    public class UNetGeneralActionsBridge : NetworkBehaviour
    {
        public bool isHost
        {
            get { return isServer && isClient; }
        }
        
        public Player player { get; protected set; }
        public NetworkIdentity identity { get; set; }

        protected virtual void Awake()
        {
            player = GetComponent<Player>();
            identity = GetComponent<NetworkIdentity>();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void OnDestroy()
        {
            
        }
        
        
        
        [Command]
        public void Cmd_RequestUseTrigger(RequestUseTriggerMessage data)
        {
            DevdogLogger.LogVerbose("[UNet][Server] Client with netID " + netId + " requested to use trigger...", this);
            if (data.triggerIdentity == null)
            {
                return;
            }
            
            var trigger = data.triggerIdentity.GetComponent<ITrigger>();
            if (trigger != null)
            {
                var canUse = trigger.CanUse(player);
                if (canUse)
                {
                    trigger.Server_Use(player);

                    // NOTE: isHost check to avoid doing the same thing twice, which is a bit heavy and spammy to the console...
                    if (isHost == false)
                    {
                        TargetRpc_UseTrigger(connectionToClient, data.triggerIdentity);
                    }
                    
//                    // TODO: Relevancy for players needs to be used here.
//                    // TODO: When this trigger is relevant for a player and it's being used by another player (or NPC), it should notify that player the collection is being used / unused (for visuals).
//                    foreach (var character in trigger.rangeHandler.GetCharactersInRange())
//                    {
//                        if (character == player)
//                        {
//                            continue;
//                        }
//                    
//                        Rpc_TriggerUsedByOtherClient(trigger.GetComponent<NetworkIdentity>());
//                    }
                }
            }
        }

        [Command]
        public void Cmd_RequestUnUseTrigger(RequestUseTriggerMessage data)
        {
            DevdogLogger.LogVerbose("[UNet][Server] Client with netID " + netId + " requested to un-use trigger...", this);

            var trigger = data.triggerIdentity.GetComponent<ITrigger>();
            if (trigger != null)
            {
                var canUnUse = trigger.CanUnUse(player);
                if (canUnUse)
                {
                    trigger.Server_UnUse(player);

                    // NOTE: isHost check to avoid doing the same thing twice, which is a bit heavy and spammy to the console...
                    if (isHost == false)
                    {
                        TargetRpc_UnUseTrigger(connectionToClient, data.triggerIdentity);
                    }
                    
//                    foreach (var character in trigger.rangeHandler.GetCharactersInRange())
//                    {
//                        if (character == player)
//                        {
//                            continue;
//                        }
//                    
//                        Rpc_TriggerUnUsedByOtherClient(trigger.GetComponent<NetworkIdentity>());
//                    }
                }
            }
        }

        [TargetRpc]
        private void TargetRpc_UseTrigger(NetworkConnection target, NetworkIdentity triggerIdentity)
        {
            DevdogLogger.LogVerbose("[UNet][Client] Server told us (netID: " + netId + ") to use trigger with netID: " + triggerIdentity.netId, this);

            var trigger = triggerIdentity.GetComponent<ITrigger>();
            if (trigger != null)
            {
                trigger.Server_Use(player);
            }
        }

        [TargetRpc]
        private void TargetRpc_UnUseTrigger(NetworkConnection target, NetworkIdentity triggerIdentity)
        {
            DevdogLogger.LogVerbose("[UNet][Client] Server told us (netID: " + netId + ") to un-use trigger with netID: " + triggerIdentity.netId, this);

            var trigger = triggerIdentity.GetComponent<ITrigger>();
            if (trigger != null)
            {
                trigger.Server_UnUse(player);
            }
        }
        
//        [ClientRpc]
//        private void Rpc_TriggerUsedByOtherClient(NetworkIdentity triggerIdentity)
//        {
//            DevdogLogger.LogVerbose("[UNet][Client] Server told us (netID: " + netId + ") that another client used a trigger with netID: " + triggerIdentity.netId, this);
//
//            var visuals = triggerIdentity.GetComponent<UNetTriggerVisuals>();
//            if (visuals != null)
//            {
//                visuals.DoVisuals();
//            }
//        }
//        
//        [ClientRpc]
//        private void Rpc_TriggerUnUsedByOtherClient(NetworkIdentity triggerIdentity)
//        {
//            DevdogLogger.LogVerbose("[UNet][Client] Server told us (netID: " + netId + ") that another client un-used a trigger with netID: " + triggerIdentity.netId, this);
//            
//            var visuals = triggerIdentity.GetComponent<UNetTriggerVisuals>();
//            if (visuals != null)
//            {
//                visuals.UndoVisuals();
//            }
//        }
    }
}

#endif