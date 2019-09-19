//Copyright 2019, Davin Carten, All rights reserved


using UnityEngine;
using System.Collections.Generic;

using emotitron;
using emotitron.Utilities.SmartVars;
using emotitron.Debugging;

#if PUN_2_OR_NEWER
using Photon.Pun;
#elif MIRROR
using Mirror;
#elif ENABLE_UNET
using UnityEngine.Networking;

#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable CS0618 // Type or member is obsolete

namespace emotitron.Networking
{

	/// <summary>
	/// An abstraction of the differences between PUN, UNET and MIRROR (and other future libraries)
	/// </summary>
#if PUN_2_OR_NEWER
	[RequireComponent(typeof(PhotonView))]
#else
	[RequireComponent(typeof(NetworkIdentity))]

#endif
	public abstract class GenericNetworkBehaviour : MonoBehaviour
	//#if PUN_2_OR_NEWER
	//		Photon.Pun.MonoBehaviourPunCallbacks
	//#else
	//		NetworkBehaviour
	//#endif

	{
		protected NetObjectLite netObj;

#if PUN_2_OR_NEWER
		protected PhotonView pv;
#else
		protected NetworkIdentity ni;
#endif

		protected virtual void Awake()
		{
			netObj = transform.root.GetComponent<NetObjectLite>();
			if (!netObj)
				netObj = gameObject.AddComponent<NetObjectLite>();

#if PUN_2_OR_NEWER
			pv = GetComponent<PhotonView>();
#else
			ni = GetComponent<NetworkIdentity>();
#endif
		}

		#region Net Library Abstractions

		public bool IsMine
		{
			get
			{
#if PUN_2_OR_NEWER
				return pv.IsMine;
#else
				return ni.hasAuthority;
#endif
			}
		}

		public int NetId
		{
			get
			{
#if PUN_2_OR_NEWER
				return (pv) ? (int)pv.ViewID : 0;
#elif MIRROR
				return (int)ni.netId;
#else
				return (int)ni.netId.Value;
#endif
			}
		}

		public static bool UsingPUN
		{
			get
			{
#if PUN_2_OR_NEWER
				return true;
#else
				return false;
#endif
			}
		}

		public bool AsServer
		{
			get
			{
#if PUN_2_OR_NEWER
				return false;
#else
				return NetworkServer.active;
#endif
			}
		}

		public bool AmServer
		{
			get
			{
#if PUN_2_OR_NEWER
				return PhotonNetwork.IsMasterClient;
#else
				return NetworkServer.active;
#endif
			}
		}

		public bool AmClient
		{
			get
			{
#if PUN_2_OR_NEWER
				return PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.IsMasterClient;
#else
				return ClientScene.readyConnection != null;
#endif
			}
		}

		public bool AmConnectedAndReady
		{
			get
			{
#if PUN_2_OR_NEWER
				return PhotonNetwork.IsConnectedAndReady;
#else
				return ClientScene.ready;
#endif
			}
		}

		#endregion

	}
}

