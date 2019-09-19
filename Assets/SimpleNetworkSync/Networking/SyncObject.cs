// Copyright 2019, Davin Carten, All rights reserved
// This code may be used for game development, but may not be used in any tools or assets that are sold to other developers.


using UnityEngine;
using emotitron.Utilities.Networking;

#if UNITY_EDITOR
using emotitron.Utilities;
using UnityEditor;
#endif


namespace emotitron.Networking
{
	public abstract class SyncObject : MonoBehaviour
	{

		public const int VersionMajor = 0;
		public const int VersionMinor = 9;
		public const int VersionRevision = 0;
		public const int Build = 0900;


		[Tooltip("Manually set the order in which callbacks occur. When components share a order value, " +
			"they will execute in the order in which they exist in the GameObjects hierarchy." +
			"It is recommended you leave this setting at the default, as strange behavior can result with some component orders.")]
		[Range(0, NetObjectLite.MAX_ORDER_VAL)]
		[SerializeField] protected int _applyOrder = 5;
		public int ApplyOrder { get { return _applyOrder; } }


		/// <summary>
		/// Used for shared cached items.
		/// </summary>
		[HideInInspector][SerializeField] protected int prefabInstanceId;

		protected NetObjAdapter noa;
		protected NetObjectLite netObj;
		public NetObjectLite NetObj { get { return netObj; } }

		protected double snapshotTime;

		protected bool hasInitialization;

		protected virtual void Reset()
		{

#if UNITY_EDITOR
			/// Only check the instanceId if we are not playing. Once we build out this is set in stone to ensure all instances and prefabs across network agree.
			if (!Application.isPlaying)
				prefabInstanceId = GetInstanceID();
#endif
			netObj = transform.root.GetComponent<NetObjectLite>();
			if (!netObj)
				netObj = transform.root.gameObject.AddComponent<NetObjectLite>();
		}
		protected virtual void OnValidate()
		{
			netObj = transform.root.GetComponent<NetObjectLite>();
			if (!netObj)
				netObj = transform.root.gameObject.AddComponent<NetObjectLite>();
		}

		#region Inspector Fields

		private Authority authority = Authority.Owner;

		#endregion

		#region Initialization / Shutdown

		/// <summary>
		/// Be sure to use base.Awake() when overriding.
		/// </summary>
		protected virtual void Awake()
		{
			netObj = transform.root.GetComponent<NetObjectLite>();
			if (netObj == null)
				netObj = transform.root.gameObject.AddComponent<NetObjectLite>();

			noa = transform.root.GetComponent<NetObjAdapter>();
			if (noa == null)
				noa = transform.root.gameObject.AddComponent<NetObjAdapter>();

			/// Inherit authority from the netobj, or master settings if it is set to auto
			if (authority == Authority.Auto)
				authority = netObj.authority == Authority.Auto ? MasterNetAdapter.DEFAULT_AUTHORITY_MODEL : netObj.authority;
			
		}

#endregion

		/// <summary>
		/// Does this connection have the final world on values for this NetObj?
		/// </summary>
		public virtual bool AmActingAuthority(bool asServer)
		{
			if (authority == Authority.Owner && noa.IsMine)
				return true;

			if (asServer && authority == Authority.Master)
				return true;

			return false;
		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(SyncObject))]
	[CanEditMultipleObjects]
	public class SyncObjectBase : HeaderEditorBase
	{

		protected readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

		}

		protected void Divider()
		{
			EditorGUILayout.Space();
			Rect r = EditorGUILayout.GetControlRect(false, 2);
			EditorGUI.DrawRect(r, Color.black);
			EditorGUILayout.Space();
		}

	}
#endif

}
