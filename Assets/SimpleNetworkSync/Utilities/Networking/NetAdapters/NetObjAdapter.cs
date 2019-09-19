// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;
using emotitron.Utilities.Networking;
using emotitron.Utilities.GUIUtilities;

#if PUN_2_OR_NEWER
using Photon.Pun;
using Photon.Realtime;
using System;
#elif MIRROR
using Mirror;
#else
using UnityEngine.Networking;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable CS0618 // UNET is obsolete

namespace emotitron.Utilities.Networking
{
	interface INetObject
	{
		bool SendInitialization(out byte[] buffer, out int bytecount);
		void ReceiveInitialization(byte[] buffer, int bytecount);

		bool OnSerialize(int frameId, byte[] buffer, ref int bitposition);
		void OnDeserialize(int sourceFrameId, int originFrameId, int localframeId, byte[] buffer, ref int bitposition);
	}
	/// <summary>
	/// This class contains the abstracted methods for different networking libraries. 
	/// </summary>
	[DisallowMultipleComponent]

#if PUN_2_OR_NEWER

	[RequireComponent(typeof(PhotonView))]
	public class NetObjAdapter : MonoBehaviourPunCallbacks
	{
		public const string ADAPTER_NAME = "PUN2";
		public const int CONN_TO_SELF_ID = 0;
		public const int CONN_NULL = -1;

		private PhotonView pv;

		//public bool IsServer { get { return PhotonNetwork.LocalPlayer.IsMasterClient; } }
		public bool IsLocalPlayer { get { return pv.IsMine; } } // isLocalPlayer; } }
		public bool IsMine { get { return pv.IsMine; } }
		public bool IsMasterOwned { get { return pv.Owner.IsMasterClient; } }

		public uint NetId { get { return (uint)pv.ViewID; } }
		/// TODO: Generate ClientId and keep it range bound rather than using connId
		public int ClientId { get { return pv.Owner.ActorNumber; } }

		/// <summary>
		/// Returns -1 if there is not connection or assigned owner yet.
		/// </summary>
		public int ConnId { get { return pv.Owner == null ? CONN_NULL : pv.Owner.ActorNumber; } }


#else

	[RequireComponent(typeof(NetworkIdentity))]
	public class NetObjAdapter : NetworkBehaviour
	{
		public const string ADAPTER_NAME = "UNET";
		public const int CONN_TO_SELF_ID = 0;
		public const int CONN_NULL = -1;

		//public bool IsServer { get { return isServer; } }
		public bool IsLocalPlayer { get { return isLocalPlayer; } }
		public bool IsMine { get { return hasAuthority; } }
		public bool IsMasterOwned { get { return ni.clientAuthorityOwner != null && ni.clientAuthorityOwner.connectionId == MasterNetAdapter.MasterConnId; } }


#if MIRROR
		public uint NetId { get { return ni.netId; } }
#else
		public uint NetId { get { return netId.Value; } }
#endif
		/// TODO: Generate ClientId and keep it range bound rather than using connId
		public int ClientId { get { return ni.clientAuthorityOwner.connectionId; } }
		public int ConnId
		{
			get
			{
				// TODO: This is still questionable
				return
					(ni == null) ? CONN_TO_SELF_ID :
					(!ni.isServer) ? ((ni.hasAuthority) ? MasterNetAdapter.MyId : MasterNetAdapter.CLNT_CONN_TO_SVR_ID) :
					//(ni.clientAuthorityOwner == null) ? CONN_TO_SELF_ID :
					//(ni.hasAuthority) ? MasterNetAdapter.MyClientId :

					ni.clientAuthorityOwner.connectionId;
			}
		}

		NetworkIdentity ni;
		private INetObject netObj;

#endif

#if UNITY_EDITOR
		private void Reset()
		{
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this, false);
		}
#endif


		// Callbacks that the NST uses for notifications of network events
		private readonly List<IOnConnect> iOnConnect = new List<IOnConnect>();
		private readonly List<IOnStartLocalPlayer> iOnStartLocalPlayer = new List<IOnStartLocalPlayer>();
		private readonly List<IOnNetworkDestroy> iOnNetworkDestroy = new List<IOnNetworkDestroy>();
		private readonly List<IOnChangeAuthority> iOnChangeAuthority = new List<IOnChangeAuthority>();
		private readonly List<IOnStart> iOnStart = new List<IOnStart>();
		private readonly List<IOnNetSerialize> iOnNetSerialize = new List<IOnNetSerialize>();
		private readonly List<IOnNetDeserialize> iOnNetDeserialize = new List<IOnNetDeserialize>();

		//private List<IOnJoinRoom> iOnJoinRoom = new List<IOnJoinRoom>();
		//private List<IOnJoinRoomFailed> iOnJoinRoomFailed = new List<IOnJoinRoomFailed>();

		public void CollectCallbackInterfaces()
		{
			GetComponentsInChildren(true, iOnConnect);
			GetComponentsInChildren(true, iOnStartLocalPlayer);
			GetComponentsInChildren(true, iOnNetworkDestroy);
			GetComponentsInChildren(true, iOnChangeAuthority);
			GetComponentsInChildren(true, iOnStart);
			GetComponentsInChildren(true, iOnNetSerialize);
			GetComponentsInChildren(true, iOnNetDeserialize);
		}

#if PUN_2_OR_NEWER

		void Awake()
		{
			pv = GetComponent<PhotonView>();
			CollectCallbackInterfaces();
		}


		private void Start()
		{
			/// TODO: This substitute for OnStartLocalPlayer is suspect at best, just not sure of a better way at the moment.
			if (pv.IsMine)// info.photonView.isMine)
				foreach (IOnStartLocalPlayer cb in iOnStartLocalPlayer)
					cb.OnStartLocalPlayer();
			
			foreach (IOnStart cb in iOnStart)
				cb.OnStart();
		}

		public override void OnConnectedToMaster()
		{
			
			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Master);
		}

		// Detect changes in ownership
		public override void OnMasterClientSwitched(Player newMasterClient)
		{
			if (newMasterClient.IsLocal)
				if (iOnNetworkDestroy != null)
					foreach (IOnChangeAuthority cb in iOnChangeAuthority)
						cb.OnChangeAuthority(IsMine);
		}

		
		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();
			if (iOnNetworkDestroy != null)
					foreach (IOnChangeAuthority cb in iOnChangeAuthority)
						cb.OnChangeAuthority(IsMine);
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			
			if (iOnNetworkDestroy != null)
				foreach (IOnNetworkDestroy cb in iOnNetworkDestroy)
					cb.OnNetworkDestroy();
		}


#else

		void Awake()
		{
			//cachedAuthModel = (AuthorityModel)NetLibrarySettings.Single.defaultAuthority;

			ni = GetComponent<NetworkIdentity>();
			netObj = GetComponent<INetObject>();
			CollectCallbackInterfaces();
		}

		public override void OnStartServer()
		{

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Server);
		}

		public override void OnStartClient()
		{

			foreach (IOnConnect cb in iOnConnect)
				cb.OnConnect(ServerClient.Client);

		}

		public override void OnStartLocalPlayer()
		{

			foreach (IOnStartLocalPlayer cb in iOnStartLocalPlayer)
				cb.OnStartLocalPlayer();
		}

		public void Start()
		{
			//XDebug.LogError("You appear to have a NetworkIdentity on instantiated object '" + name + "', but that object has NOT been network spawned. " +
			//	"Only use NetworkSyncTransform and NetworkIdentity on objects you intend to spawn normally from the server using NetworkServer.Spawn(). " +
			//		"(Projectiles for example probably don't need to be networked objects).", ni.netId.Value == 0, true);

			//// If this is an invalid NST... abort startup and shut it down.
			//if (ni.netId.Value == 0)
			//{
			//	Destroy(GetComponent<NetworkSyncTransform>());
			//	return;
			//}

			foreach (IOnStart cb in iOnStart)
				cb.OnStart();
		}

		public override void OnNetworkDestroy()
		{
			if (iOnNetworkDestroy != null)
				foreach (IOnNetworkDestroy cb in iOnNetworkDestroy)
					cb.OnNetworkDestroy();
		}

		public override void OnStartAuthority()
		{
			if (iOnNetworkDestroy != null)
				foreach (IOnChangeAuthority cb in iOnChangeAuthority)
					cb.OnChangeAuthority(IsMine);
		}

		public override void OnStopAuthority()
		{
			if (iOnNetworkDestroy != null)
				foreach (IOnChangeAuthority cb in iOnChangeAuthority)
					cb.OnChangeAuthority(IsMine);
		}


		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{

			if (initialState)
			{

				byte[] buffer; int bytecount;
				bool hascontent = netObj.SendInitialization(out buffer, out bytecount);
				
				if (!hascontent)
					return false;

				//int bytecount = (bitposition + 7) >> 3;
				writer.Write((ushort)bytecount);
				for (int i = 0; i < bytecount; ++i)
					writer.Write(buffer[i]);

				return true;
			}
			return false;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				var buffer = NetMsgSends.reusableIncomingBuffer;

				int bytecount = reader.ReadUInt16();
				for (int i = 0; i < bytecount; ++i)
					buffer[i] = reader.ReadByte();

				netObj.ReceiveInitialization(buffer, bytecount);
			}
		}

#endif

	}

#if UNITY_EDITOR

	[CustomEditor(typeof(NetObjAdapter))]
	[CanEditMultipleObjects]
	public class NetObjAdapterEditor : HeaderEditorBase
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox("Abstracted NetLib (UNET, Photon, MIRROR, etc) specific code. This is required by Net Objects to work.", MessageType.None);
		}
	}
#endif

}

#pragma warning restore CS0618 // UNET is obsolete
