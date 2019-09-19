﻿//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if PUN_2_OR_NEWER
using Photon.Pun;
#elif MIRROR
using Mirror;
#else
using UnityEngine.Networking;
#endif

#pragma warning disable CS0618 // UNET is obsolete

namespace emotitron.Utilities.Networking
{

	public class GenericSpawnPoint : MonoBehaviour
	{
		[Header("Spawn Point Blocked Check")]
		[Tooltip("Select the physics layers for colliders to test against. If 'avoidCollision' is true and any colliders on these layers " +
			"are blocking the spawn point, will attempt to find the next spawn point that isn't blocked.")]
		public LayerMask layerMask;
		public float blockedCheckRadius = 2f;

		void OnEnable()
		{
			spawns.Add(this);
#if !PUN_2_OR_NEWER
			NetworkManager.RegisterStartPosition(transform);
#endif
		}

		void OnDisable()
		{
			spawns.Remove(this);
#if !PUN_2_OR_NEWER
			NetworkManager.UnRegisterStartPosition(transform);
#endif
		}

		public bool IsBlocked
		{
			get
			{
				int count = Physics.OverlapSphereNonAlloc(transform.position, blockedCheckRadius, reusable, layerMask);
				return (count == 0) ? false : true;
			}
		}
		// Statics

		public static readonly List<GenericSpawnPoint> spawns = new List<GenericSpawnPoint>();
		private static int lastPicked;
		private static readonly Collider[] reusable = new Collider[8];

		public static Transform GetRandomSpawnPoint(bool avoidCollision = true)
		{
			if (spawns.Count == 0)
				return null;

			int startindex = Random.Range(0, spawns.Count - 1);

			// Try to find a spawn that doesn't conflict
			if (avoidCollision)
				for (int i = 0; i < spawns.Count; i++)
					if (!spawns[(i + startindex) % spawns.Count].IsBlocked)
						return spawns[(i + startindex) % spawns.Count].transform;

			// No collision free spawn found
			return spawns[startindex].transform;
		}

		public static Transform GetNextSpawnPoint(bool avoidCollision = true)
		{
			if (spawns.Count == 0)
				return null;

			lastPicked = (lastPicked + 1) % spawns.Count;

			// Try to find a spawn that doesn't conflict
			if (avoidCollision)
				for (int i = 0; i < spawns.Count; i++)
				{
					int next = (i + lastPicked) % spawns.Count;
					if (!spawns[next].IsBlocked)
					{
						lastPicked = next;
						break;
					}
				}

			return spawns[lastPicked].transform;
		}

		public static Transform GetSpawnPointFromValue(int value)
		{
			if (spawns.Count == 0)
				return null;

			int spawnId = (value + 1) % spawns.Count;

			return spawns[spawnId].transform;
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, blockedCheckRadius);
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(GenericSpawnPoint))]
	[CanEditMultipleObjects]
	public class GenericSpawnPointEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.HelpBox("Replacement for UNET 'NetworkStartPosition' that will work with other network engines.", MessageType.None);
		}
	}

#endif

}

#pragma warning restore CS0618 // UNET is obsolete
