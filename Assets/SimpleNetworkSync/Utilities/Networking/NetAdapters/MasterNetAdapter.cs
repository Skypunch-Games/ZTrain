//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Compression;
using System.Collections;
using System.Collections.Generic;
using emotitron.Utilities.CallbackUtils;
using UnityEngine;

#if PUN_2_OR_NEWER
using Photon;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
#elif MIRROR
using Mirror;
#else
using UnityEngine.Networking;
#endif

#pragma warning disable CS0618 // UNET is obsolete

namespace emotitron.Utilities.Networking
{

	public enum NetworkLibrary { UNET, PUN, PUN2 }
	/// <summary>
	/// Unity Unet specific stuff tucked away, to not litter up the sample code with UNET specific stuff.
	/// </summary>
	public class MasterNetAdapter

#if PUN_2_OR_NEWER
	: IConnectionCallbacks, IInRoomCallbacks, IMatchmakingCallbacks
	{
		public const string ADAPTER_NAME = "PUN2";
		public const NetworkLibrary NET_LIB = NetworkLibrary.PUN2;
		public const Authority DEFAULT_AUTHORITY_MODEL = Authority.Owner;

		/// <summary>
		/// Number of bits used for NetObj write length. This dictates the max number of bits that can be writeen each update per NetObject. 13 bits = 8191 max bitcount
		/// </summary>
#if STITCH_WHOLE_BYTES
		public const int WRITE_SIZE_BITS = 10; // (2 ^ BYTE_CNT_BYTES) must produce a larger nuber than MAX_BUFFER_BYTES
#else
		public const int WRITE_SIZE_BITS = 13; // (2 ^ BYTE_CNT_BYTES) must produce a larger nuber than MAX_BUFFER_BYTES
#endif
		public const int MAX_BUFFER_BYTES = 1020;
		public const int MAX_BUFFER_BITS = MAX_BUFFER_BYTES * 8;
		public const Architecture ARCHITECTURE = Architecture.MasterRelay;

		public const int SVR_CONN_TO_SELF_ID = 0;
		public const int CLNT_CONN_TO_SVR_ID = -1;
		public static int MyId { get { return PhotonNetwork.LocalPlayer.ActorNumber; } }
		public static int MasterConnId
		{
			get
			{
				return (PhotonNetwork.MasterClient != null) ? PhotonNetwork.MasterClient.ActorNumber : -1;
			}
		}
#else
	{
		public const string ADAPTER_NAME = "UNET";
		public const NetworkLibrary NET_LIB = NetworkLibrary.UNET;
		public const Authority DEFAULT_AUTHORITY_MODEL = Authority.Master;

		/// <summary>
		/// Number of bits used for NetObj write length. This dictates the max number of bits that can be writeen each update per NetObject. 13 bits = 8191 max bitcount
		/// </summary>
		public const int WRITE_SIZE_BITS = 10; // (2 ^ BYTE_CNT_BYTES) must produce a larger nuber than MAX_BUFFER_BYTES
		public const int MAX_BUFFER_BYTES = 1020;
		public const int MAX_BUFFER_BITS = MAX_BUFFER_BYTES * 8;
		public const Architecture ARCHITECTURE = Architecture.ServerClient;

		public static byte[] byteBuffer = new byte[MAX_BUFFER_BYTES * 2];

		public const int SVR_CONN_TO_SELF_ID = 0;
		public const int CLNT_CONN_TO_SVR_ID = -1;
		public static int MyId
		{
			get
			{
#if MIRROR
				return (NetworkClient.connection == null) ? -1 :
					NetworkClient.connection.connectionId;
#else
				return (!NetworkManager.singleton || NetworkManager.singleton.client == null || NetworkManager.singleton.client.connection == null) ? -1 :
					NetworkManager.singleton.client.connection.connectionId;
#endif
			}
		}
		public static int MasterConnId
		{
			get
			{
				return NetworkServer.active ? SVR_CONN_TO_SELF_ID : -1;
			}
		}

#endif
		public static MasterNetAdapter single;

		public static int overflowBitPos;
		public static ICollection<object> connections;

		
		// Static Constructor
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void ForceAwake()
		{
			if (single == null)
				single = new MasterNetAdapter();

			RegisterNetworkCallbacks();

		}
#if PUN_2_OR_NEWER

		public static void DispatchIncomingMessages()
		{
			/// Make sure Photon isn't holding out on us just because a FixedUpdate didn't happen this Update()
			bool doDispatch = true;
			while (PhotonNetwork.IsMessageQueueRunning && doDispatch)
			{
				// DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
				doDispatch = PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
			}
		}
#endif


		#region Outgoing Callback Interfaces

		public interface IConnectionEvents
		{
			void OnClientConnect(object connObj, int connId);
			void OnClientDisconnect(object connObj, int connId);
			void OnServerConnect(object connObj, int connId);
			void OnServerDisconnect(object connObj, int connId);
		}
		public static List<IConnectionEvents> iConnectionEvents = new List<IConnectionEvents>();

		public interface IOnClientConnect { void OnClientConnect(object connObj, int connId); }
		public static List<IOnClientConnect> iOnClientConnect = new List<IOnClientConnect>();
		public delegate void OnClientConnectCallback(object connObj, int connId);
		public static OnClientConnectCallback onClientConnectCallback;

		public interface IOnClientDisconnect { void OnClientDisconnect(object connObj, int connId); }
		public static List<IOnClientDisconnect> iOnClientDisconnect = new List<IOnClientDisconnect>();
		public delegate void OnClientDisconnectCallback(object connObj, int connId);
		public static OnClientDisconnectCallback onClientDisconnectCallback;

		public interface IOnServerConnect { void OnServerConnect(object connObj, int connId); }
		public static List<IOnServerConnect> iOnServerConnect = new List<IOnServerConnect>();
		public delegate void OnServerConnectCallback(object connObj, int connId);
		public static OnServerConnectCallback onServerConnectCallback;

		public interface IOnServerDisconnect { void OnServerDisconnect(object connObj, int connId); }
		public static List<IOnServerDisconnect> iOnServerDisconnect = new List<IOnServerDisconnect>();
		public delegate void OnServerDisconnectCallback(object connObj, int connId);
		public static OnServerDisconnectCallback onServerDisconnectCallback;

		public static void RegisterCallbackInterfaces(Object obj, bool register = true)
		{
			CallbackUtilities.RegisterInterface(iConnectionEvents, obj, register);
			CallbackUtilities.RegisterInterface(iOnClientConnect, obj, register);
			CallbackUtilities.RegisterInterface(iOnClientDisconnect, obj, register);
			CallbackUtilities.RegisterInterface(iOnServerConnect, obj, register);
			CallbackUtilities.RegisterInterface(iOnServerDisconnect, obj, register);
		}

		#endregion

		#region Incoming Network Event Callbacks



#if PUN_2_OR_NEWER


#else

		#region  NetworkManager SimpleNetManager callbacks

		private static void RegisterNetworkCallbacks()
		{
			//SimpleNetManager.onClientConnect += OnClientConnect;
			//SimpleNetManager.onClientDisconnect += OnClientDisconnect;
			//SimpleNetManager.onServerConnect += OnServerConnect;
			//SimpleNetManager.onServerDisconnect += OnServerDisconnect;
		}

		//private static void OnClientConnect(NetworkConnection conn)
		//{
		//	Debug.Log("Client Connected " + conn);
		//	foreach (var cb in iConnectionEvents)
		//		cb.OnClientConnect(conn, conn.connectionId);

		//	foreach (var cb in iOnClientConnect)
		//		cb.OnClientConnect(conn, conn.connectionId);

		//	if (onClientConnectCallback != null)
		//		onClientConnectCallback.Invoke(conn, conn.connectionId);
		//}

		//private static void OnClientDisconnect(NetworkConnection conn)
		//{
		//	Debug.Log("Client Disconnected " + conn);
		//	foreach (var cb in iConnectionEvents)
		//		cb.OnClientDisconnect(conn, conn.connectionId);

		//	foreach (var cb in iOnClientDisconnect)
		//		cb.OnClientDisconnect(conn, conn.connectionId);

		//	if (onClientDisconnectCallback != null)
		//		onClientDisconnectCallback.Invoke(conn, conn.connectionId);
		//}

		//private static void OnServerConnect(NetworkConnection conn)
		//{
		//	Debug.Log("Server Connected " + conn);
		//	foreach (var cb in iConnectionEvents)
		//		cb.OnServerConnect(conn, conn.connectionId);

		//	foreach (var cb in iOnServerConnect)
		//		cb.OnServerConnect(conn, conn.connectionId);

		//	if (onServerConnectCallback != null)
		//		onServerConnectCallback.Invoke(conn, conn.connectionId);
		//}

		//private static void OnServerDisconnect(NetworkConnection conn)
		//{
		//	Debug.Log("Client Disconnected " + conn);
		//	foreach (var cb in iConnectionEvents)
		//		cb.OnServerDisconnect(conn, conn.connectionId);

		//	foreach (var cb in iOnServerDisconnect)
		//		cb.OnServerDisconnect(conn, conn.connectionId);

		//	if (onServerDisconnectCallback != null)
		//		onServerDisconnectCallback.Invoke(conn, conn.connectionId);
		//}

		#endregion



#endif

		#endregion


		#region Properties

#if PUN_2_OR_NEWER


		public static bool Connected { get { return PhotonNetwork.IsConnected; } }
		public static bool ReadyToSend { get { return PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnectedAndReady; } }
		//public static bool ReadyToSend { get { return PhotonNetwork.isMasterClient || PhotonNetwork.isNonMasterClientInRoom; } }
		public static bool ServerIsActive { get { return PhotonNetwork.IsMasterClient; } }
		//public static bool ClientIsActive { get { return PhotonNetwork.isNonMasterClientInRoom; } }
		public static bool ClientIsActive { get { return PhotonNetwork.InRoom; } }
		public static bool ClientIsReady { get { return PhotonNetwork.InRoom; } }
		public static bool NetworkIsActive { get { return PhotonNetwork.IsMasterClient || PhotonNetwork.InRoom; } }

		/// <summary> Cached value for defaultAuthority since this is hotpath </summary>


		//public static bool ClientIsReady { get { return ClientScene.ready; } }
		public static int MaxConnections { get { return PhotonNetwork.CurrentRoom.MaxPlayers; } }

		public const byte LowestMsgTypeId = 0;
		public const byte HighestMsgTypeId = 199;
		//public const byte DefaultMsgTypeId = 190;

#else

		public static bool Connected { get { return NetworkServer.active || NetworkClient.active; } }
#if MIRROR
		public static bool ReadyToSend { get { return ClientScene.readyConnection != null; } }
#else
		public static bool ReadyToSend { get { return NetworkServer.active || (NetworkManager.singleton.client != null && NetworkManager.singleton.client.isConnected); } }
#endif
		public static bool ServerIsActive { get { return NetworkServer.active; } }
		public static bool ClientIsActive { get { return NetworkClient.active; } }
		public static bool ClientIsReady { get { return ClientScene.ready; } }
		public static bool NetworkIsActive { get { return NetworkClient.active || NetworkServer.active; } }
		public static int MaxConnections { get { return NetworkManager.singleton.maxConnections; } }
		public const short LowestMsgTypeId = (short)MsgType.Highest;
		public const short HighestMsgTypeId = short.MaxValue;
		public const short DefaultMsgTypeId = 190;
#endif
		#endregion


		#region  Messages


#if PUN_2_OR_NEWER



		// Add PUN2 message handlers here

		public static void RegisterDefaultSerializeMsgIds()
		{
			///// Register the Static handler method for incoming UNET messages.
			//{
			//	if (NetworkServer.active && !NetworkServer.handlers.ContainsKey(DEF_CLNT_TO_SVR_MSGID))
			//	{
			//		NetworkServer.RegisterHandler(DEF_CLNT_TO_SVR_MSGID, ServerReceiveMsg);
			//	}
			//	else if (NetworkClient.active && !NetworkManager.singleton.client.handlers.ContainsKey(DEF_SVR_TO_CLNT_MSGID))
			//	{
			//		NetworkManager.singleton.client.RegisterHandler(DEF_SVR_TO_CLNT_MSGID, ClientReceiveMsg);
			//	}
			//}
		}


		//private static readonly RaiseEventOptions optsOthers = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
		//private static readonly RaiseEventOptions optsSvr = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
		//private static readonly RaiseEventOptions optsTarget = new RaiseEventOptions() { Receivers = ReceiverGroup.Others, TargetActors = new int[1] };
		//private static readonly SendOptions sendOpts = new SendOptions();

		/// <summary>
		/// Room callbacks
		/// </summary>
		/// <param name="newPlayer"></param>

		public void OnPlayerEnteredRoom(Player newPlayer)
		{
			Debug.Log("OnPlayerEnteredRoom " + newPlayer);
			foreach (var cb in iConnectionEvents)
				cb.OnServerConnect(newPlayer, newPlayer.ActorNumber);

			foreach (var cb in iOnServerConnect)
				cb.OnServerConnect(newPlayer, newPlayer.ActorNumber);

			if (onServerConnectCallback != null)
				onServerConnectCallback.Invoke(newPlayer, newPlayer.ActorNumber);


			foreach (var cb in iConnectionEvents)
				cb.OnClientConnect(newPlayer, newPlayer.ActorNumber);

			foreach (var cb in iOnClientConnect)
				cb.OnClientConnect(newPlayer, newPlayer.ActorNumber);

			if (onClientConnectCallback != null)
				onClientConnectCallback.Invoke(newPlayer, newPlayer.ActorNumber);
		}

		public void OnPlayerLeftRoom(Player otherPlayer)
		{
			Debug.Log("OnPlayerLeftRoom " + otherPlayer + onServerDisconnectCallback);

			foreach (var cb in iConnectionEvents)
				cb.OnServerDisconnect(otherPlayer, otherPlayer.ActorNumber);

			foreach (var cb in iOnServerDisconnect)
				cb.OnServerDisconnect(otherPlayer, otherPlayer.ActorNumber);

			if (onServerDisconnectCallback != null)
				onServerDisconnectCallback.Invoke(otherPlayer, otherPlayer.ActorNumber);


			foreach (var cb in iConnectionEvents)
				cb.OnClientDisconnect(otherPlayer, otherPlayer.ActorNumber);

			foreach (var cb in iOnClientDisconnect)
				cb.OnClientDisconnect(otherPlayer, otherPlayer.ActorNumber);

			if (onClientDisconnectCallback != null)
				onClientDisconnectCallback.Invoke(otherPlayer, otherPlayer.ActorNumber);

		}

		public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			//throw new System.NotImplementedException();
		}

		public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
		{
			//throw new System.NotImplementedException();
		}

		public void OnMasterClientSwitched(Player newMasterClient)
		{
			//throw new System.NotImplementedException();
		}


		/// Matchmaking callbacks

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			//throw new System.NotImplementedException();
		}

		public void OnCreatedRoom()
		{
			//throw new System.NotImplementedException();
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			//throw new System.NotImplementedException();
		}

		public void OnJoinedRoom()
		{
			Debug.Log("OnJoinedRoom ");
			foreach (var cb in iConnectionEvents)
				cb.OnClientConnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			foreach (var cb in iOnClientConnect)
				cb.OnClientConnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			if (onClientConnectCallback != null)
				onClientConnectCallback.Invoke(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			//throw new System.NotImplementedException();
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			//throw new System.NotImplementedException();
		}

		public void OnLeftRoom()
		{
			Debug.Log("OnLeftRoom - Client Disconnected? ");
			foreach (var cb in iConnectionEvents)
				cb.OnClientDisconnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			foreach (var cb in iOnClientDisconnect)
				cb.OnClientDisconnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			if (onClientDisconnectCallback != null)
				onClientDisconnectCallback.Invoke(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
		}



		/// <summary>
		/// Connection callbacks
		/// </summary>

		public void OnConnected()
		{
			//Debug.Log("OnConnected ");
			//foreach (var cb in iConnectionEvents)
			//	cb.OnClientConnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			//if (onClientConnectCallback != null)
			//	onClientConnectCallback.Invoke(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
		}

		public void OnConnectedToMaster()
		{
			//Debug.Log("OnConnectedToMaster");
			//foreach (var cb in iConnectionEvents)
			//	cb.OnClientConnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			//if (onClientConnectCallback != null)
			//	onClientConnectCallback.Invoke(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
		}

		public void OnDisconnected(DisconnectCause cause)
		{
			//Debug.Log("OnDisconnected - Client Disconnected? ");
			//foreach (var cb in iConnectionEvents)
			//	cb.OnClientDisconnect(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);

			//if (onClientConnectCallback != null)
			//	onClientConnectCallback.Invoke(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
			//throw new System.NotImplementedException();
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
			//throw new System.NotImplementedException();
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
			//throw new System.NotImplementedException();
		}

		/// <summary>
		/// Force Photon to dispatch any pending incoming events. This may be needed on Updates where no FixedUpdate fired.
		/// </summary>
		private static void RegisterNetworkCallbacks()
		{
			if (PhotonNetwork.NetworkingClient == null)
				return;


			if (Application.isPlaying)
			{
				PhotonNetwork.NetworkingClient.AddCallbackTarget(single);
				//PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
			}

		}



		//private static void OnEvent(EventData photonEvent)
		//{
		//	byte code = photonEvent.Code;
		//	//Debug.Log("OnEvent Data" + photonEvent + " from: " + photonEvent.Sender + " toSvr? " + (photonEvent.Code == CLIENT_TO_SVR));

		//	if (code != NetMsgCallbacks.DEF_MSG_ID && photonEvent.Code != SVR_TO_CLIENT)
		//		return;

		//	bool asServer = code == CLIENT_TO_SVR;
		//	bool fromServer = code == SVR_TO_CLIENT;

		//	// ignore messages from self.
		//	if (MyId == photonEvent.Sender)
		//	{
		//		Debugging.XDebug.LogWarning("Talking to self? Normal occurance for a few seconds after Master leaves the game and a new master is selected.");
		//		return;
		//	}

		//	NM_Deserialize.Deserialize(photonEvent.Sender, photonEvent.CustomData as byte[], asServer, fromServer);

		//	//UdpBitStream bitstream = new UdpBitStream(photonEvent.CustomData as byte[]);
		//	//UdpBitStream outstream = new UdpBitStream(NSTMaster.outstreamByteArray);

		//	//bool mirror = PhotonNetwork.IsMasterClient && NetLibrarySettings.single.defaultAuthority == DefaultAuthority.ServerAuthority;

		//	//Serverr(ref bitstream, ref outstream, mirror, photonEvent.Sender);

		//	//if (mirror)// authorityModel == DefaultAuthority.ServerClient)
		//	//{
		//	//	ArraySegment<byte> byteseg = new ArraySegment<byte>(outstream.Data, 0, outstream.BytesUsed);

		//	//	PhotonNetwork.NetworkingClient.OpRaiseEvent(DefaultMsgTypeId, byteseg, optsOthers, sendOpts);

		//	//	PhotonNetwork.NetworkingClient.Service();
		//	//}
		//}




		///// <summary>
		///// Write the contents of a byte[] out to a UNET NetworkWriter and send.
		///// </summary>
		//public static void NetSend(object connection, byte[] buffer, int bitcount, int channel, bool asServer)
		//{
		//	NetworkConnection nc = connection as NetworkConnection;

		//	if (asServer)
		//		SvrSendToConnection(nc, buffer, bitcount, channel);
		//	else if (!NetworkServer.active)
		//		SendPayloadToServer(buffer, bitcount, channel);
		//}

		//public static void NetSend(object connection, int connId, byte[] buffer, int bitposition, bool asServer)
		//{
			
		//	int bytepos = (bitposition + 7) >> 3;
		//	System.ArraySegment<byte> byteseg = new System.ArraySegment<byte>(buffer, 0, bytepos);

		//	optsTarget.TargetActors[0] = connId;

		//	PhotonNetwork.NetworkingClient.OpRaiseEvent((asServer ? SVR_TO_CLIENT : CLIENT_TO_SVR), byteseg, optsTarget, sendOpts);
		//	//PhotonNetwork.NetworkingClient.OpRaiseEvent(DefaultMsgTypeId, byteseg, (isServerClient && !PhotonNetwork.IsMasterClient) ? optsSvr : optsOthers, sendOpts);

		//	if (!Time.inFixedTimeStep)
		//		PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands(); //.Service();

		//}

		///// <summary>
		///// Command the Network Library to enumerate all connections, and pass each those connection objects and conn id to the NetMaster
		///// for stitching. This is a bit tangled, as the NetMaster calls this, then this calls methods in the NetMaster, which call NetworkSends back here
		///// in the MasterNetAdapter.
		///// </summary>
		///// <param name="localFrameId"></param>
		///// <param name="buffer"></param>
		///// <param name="bitposition"></param>
		///// <param name="asServer"></param>
		//public static void StitchAndSendToConnections(int localFrameId, bool asServer)
		//{
		//	foreach (var player in PhotonNetwork.PlayerListOthers)
		//	{
		//		bool isToServer = player.IsMasterClient;
		//		NM_Stitcher.StitchForConnAndSend(player, player.ActorNumber, localFrameId, asServer, isToServer);
		//	}
		//}

#else
		//static readonly NetworkWriter unetwriter = new NetworkWriter();

		///// <summary>
		///// Register the Server and Client message handlers
		///// </summary>
		///// <param name="OnSvrRcv"></param>
		///// <param name="OnClientsRcv"></param>
		//public static void RegisterHandlers(NetworkMessageDelegate OnSvrRcv, NetworkMessageDelegate OnClientsRcv)
		//{
		//	/// Register the Static handler method for incoming UNET messages.
		//	{
		//		if (OnSvrRcv != null && NetworkServer.active && !NetworkServer.handlers.ContainsKey(CLIENT_TO_SVR))
		//		{
		//			NetworkServer.RegisterHandler(CLIENT_TO_SVR, OnSvrRcv);
		//		}
		//		else if (OnClientsRcv != null && NetworkClient.active && !NetworkManager.singleton.client.handlers.ContainsKey(SVR_TO_CLIENT))
		//		{
		//			NetworkManager.singleton.client.RegisterHandler(SVR_TO_CLIENT, OnClientsRcv);
		//		}
		//	}
		//}


		//public static void RegisterDefaultSerializeMsgIds()
		//{
		//	/// Register the Static handler method for incoming UNET messages.
		//	{
		//		if (NetworkServer.active && !NetworkServer.handlers.ContainsKey(CLIENT_TO_SVR))
		//		{
		//			NetworkServer.RegisterHandler(CLIENT_TO_SVR, ServerReceiveMsg);
		//		}
		//		else if (NetworkClient.active && !NetworkManager.singleton.client.handlers.ContainsKey(SVR_TO_CLIENT))
		//		{
		//			NetworkManager.singleton.client.RegisterHandler(SVR_TO_CLIENT, ClientReceiveMsg);
		//		}
		//	}
		//}

		//[System.Obsolete("These shouldn't really exists. Use NetMsg for comms.")]
		//public static void ServerReceiveMsg(NetworkMessage msg)
		//{
		//	//Debug.Log("SvrRcv msgLen:" + msg.reader.Length);
		//	int bitposition = 0;
		//	msg.reader.Read(byteBuffer, ref bitposition);
		//	emotitron.Networking.NM_Deserialize.Deserialize(msg.conn.connectionId, byteBuffer, true, msg.msgType == SVR_TO_CLIENT);
		//}

		//[System.Obsolete("These shouldn't really exists. Use NetMsg for comms.")]
		//public static void ClientReceiveMsg(NetworkMessage msg)
		//{
		//	//Debug.Log("ClientRcv msgLen:" + msg.reader.Length);
		//	int bitposition = 0;
		//	msg.reader.Read(byteBuffer, ref bitposition);
		//	emotitron.Networking.NM_Deserialize.Deserialize(-1, byteBuffer, false, msg.msgType == SVR_TO_CLIENT);

		//}


		///// <summary>
		///// Write the contents of a byte[] out to a UNET NetworkWriter and send.
		///// </summary>
		//public static void NetSend(object connection, int connId, byte[] buffer, int bitcount, bool asServer)
		//{
		//	NetworkConnection nc = connection as NetworkConnection;

		//	if (asServer)
		//		SvrSendToConnection(nc, buffer, bitcount);
		//	else if (!NetworkServer.active)
		//		SendPayloadToServer(buffer, bitcount);
		//}


		///// <summary>
		///// Copy the byte[] of NetworkMessage into your bitstream.
		///// </summary>
		///// <param name="netMsg"></param>
		///// <param name="bitstream"></param>
		//public static void NetworkMessageToBitstream(NetworkMessage netMsg, ref Bitstream bitstream)
		//{
		//	bitstream.Reset();

		//	for (uint i = netMsg.reader.Position; i < netMsg.reader.Length; ++i)
		//		bitstream.WriteByte(netMsg.reader.ReadByte());
		//}

		///// <summary>
		///// Command the Network Library to enumerate all connections, and pass each those connection objects and conn id to the NetMaster
		///// for stitching. This is a bit tangled, as the NetMaster calls this, then this calls methods in the NetMaster, which call NetworkSends back here
		///// in the MasterNetAdapter.
		///// </summary>
		///// <param name="localFrameId"></param>
		///// <param name="buffer"></param>
		///// <param name="bitposition"></param>
		///// <param name="asServer"></param>
		//public static void StitchAndSendToConnections(int localFrameId, bool asServer)
		//{
		//	foreach (NetworkConnection nc in NetworkServer.connections)
		//	{
		//		if (nc == null)
		//			continue;

		//		/// Don't send to self if Host
		//		if (nc.connectionId == 0)
		//			continue;

		//		if (nc.isReady)
		//		{
		//			/// TODO: needs to be isolated from the networking lib
		//			emotitron.Networking.NM_Stitcher.StitchForConnAndSend(nc, nc.connectionId, localFrameId, asServer, false);
		//		}
		//	}
		//}

		//public static void SvrSendToConnection(object connection, byte[] buffer, int bitcount, int channel = Channels.DefaultUnreliable)
		//{
		//	NetworkConnection nc = (connection as NetworkConnection);
		//	int bytecount = (bitcount + 7) >> 3;

		//	unetwriter.StartMessage(SVR_TO_CLIENT);
		//	unetwriter.Write(buffer, bytecount);
		//	unetwriter.FinishMessage();
		//	nc.SendWriter(unetwriter, channel);
		//}


		//public static void SendPayloadToServer(byte[] buffer, int bitcount, int channel = Channels.DefaultUnreliable)
		//{
		//	short msgid = CLIENT_TO_SVR;

		//	unetwriter.StartMessage(msgid);
		//	int bytecount = (bitcount + 7) >> 3;
		//	//int bytecount = (bitcount >> 3) + ((bitcount % 8 != 0) ? 1 : 0);
		//	unetwriter.Write(buffer, bytecount);
		//	unetwriter.FinishMessage();
		//	ClientScene.readyConnection.SendWriter(unetwriter, channel);
		//}
#endif

		#endregion


		#region Instantiation

#if PUN_2_OR_NEWER

		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject go = PhotonNetwork.Instantiate(prefab.name, position, rotation, 0);
			go.transform.parent = parent;
			return go;
		}

		public static void UnSpawn(GameObject obj)
		{
			if (obj.GetComponent<PhotonView>().IsMine && PhotonNetwork.IsConnected)
			{
				PhotonNetwork.Destroy(obj);
			}
		}


#else

		public static int cloneId = 0;
		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject go = GameObject.Instantiate(prefab, position, rotation, parent);
			NetworkServer.Spawn(go);
			return go;
		}

		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, NetworkConnection nc)
		{
			GameObject go = GameObject.Instantiate(prefab, position, rotation, parent);
			NetworkServer.SpawnWithClientAuthority(go, nc);
			return go;
		}

		public static void UnSpawn(GameObject go)
		{
			if (NetworkServer.active)
				NetworkServer.UnSpawn(go);
		}

		public static void ServerChangeScene(string sceneName)
		{
			if (NetworkServer.active)
				NetworkManager.singleton.ServerChangeScene(sceneName);
		}
#endif
		#endregion

	}

}
#pragma warning restore CS0618 // UNET is obsolete
