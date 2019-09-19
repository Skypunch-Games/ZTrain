
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PUN_2_OR_NEWER
using Photon.Pun;
using ExitGames.Client.Photon;
#elif MIRROR
using Mirror;
#else
using UnityEngine.Networking;
#endif

#pragma warning disable CS0618 // //  Unet Obsolete supress

namespace emotitron.Utilities.Networking
{
	/// <summary>
	/// Tools that work for all currently supported netlibs, using unified methods.
	/// </summary>
	public static class UnifiedNetTools
	{

		/// <summary>
		/// Unified Network Find GameObject using netid, and return the root component of type T. For Photon PhotonView is the return of the Find already.
		/// </summary>
		public static T FindComponentByNetId<T>(this uint netid) where T : Component
		{
#if PUN_2_OR_NEWER
			PhotonView found = PhotonView.Find((int)netid);
			if (found == null)
			{
				Debug.LogWarning("No Object found for viewID " + netid + ". \nIt is not unsual with PUN2 for messages sometimes to arrive before their target gameobject is ready at startup.");
				return null;
			}
			if (typeof(T) == typeof(PhotonView))
				return (found as T);
			else
				return found.GetComponent<T>();

#else

#if MIRROR
			GameObject found = (NetworkServer.active) ?
				NetworkServer.FindLocalObject(netid) :
				ClientScene.FindLocalObject(netid);
#else
			GameObject found = (NetworkServer.active) ?
				NetworkServer.FindLocalObject(new NetworkInstanceId(netid)) :
				ClientScene.FindLocalObject(new NetworkInstanceId(netid));
#endif

			if (found == null)
			{
				Debug.LogWarning("No Object found for netid " + netid + " " + NetworkServer.active);
				return null;
			}

			return found.GetComponent<T>();
#endif
		}



		/// <summary>
		/// Unitifed Network Find GameObject using netid.
		/// </summary>
		public static GameObject FindGameObjectByNetId(this uint netid)
		{
#if PUN_2_OR_NEWER
			PhotonView found = PhotonView.Find((int)netid);
			if (found == null)
			{
				Debug.LogWarning("No Object found for netid " + netid);
				return null;
			}
			return found.gameObject;
#elif MIRROR
			GameObject go = (NetworkServer.active) ?
				NetworkServer.FindLocalObject(netid) :
				ClientScene.FindLocalObject(netid);

			return go;
#elif ENABLE_UNET
			GameObject go = (NetworkServer.active) ?
				NetworkServer.FindLocalObject(new NetworkInstanceId(netid)) :
				ClientScene.FindLocalObject(new NetworkInstanceId(netid));

			return go;
#endif
		}

	}
}

#pragma warning restore CS0618 //  Unet Obsolete supress


