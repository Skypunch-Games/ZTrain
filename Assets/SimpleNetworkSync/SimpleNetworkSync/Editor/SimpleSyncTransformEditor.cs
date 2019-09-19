// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Networking
{
	[CustomEditor(typeof(SimpleSyncTransform))]
	[CanEditMultipleObjects]
	public class SimpleSyncTransformEditor : SyncObjectTFrameBase
	{
		public override void OnEnable()
		{
			base.OnEnable();
			textTexture = (Texture2D)Resources.Load<Texture2D>("Header/EditorHeaderTransform");
		}
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
}

