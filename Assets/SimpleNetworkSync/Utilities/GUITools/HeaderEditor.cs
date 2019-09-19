//Copyright 2018, Davin Carten, All rights reserved


using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Utilities
{
	

#if UNITY_EDITOR
	/// <summary>
	/// All of this just draws the pretty NST header graphic on components. Nothing to see here.
	/// </summary>
	[CustomEditor(typeof(Component))]
	[CanEditMultipleObjects]
	public class HeaderEditorBase : Editor
	{
		protected Texture2D textTexture;
		protected Texture2D backTexture;
		protected Texture2D teapotTexture;

		public virtual void OnEnable()
		{
			if (textTexture == null)
				textTexture = (Texture2D)Resources.Load<Texture2D>("EditorHeaderText");

			if (teapotTexture == null)
				teapotTexture = (Texture2D)Resources.Load<Texture2D>("EditorHeaderTeapot");

			if (backTexture == null)
				backTexture = (Texture2D)Resources.Load<Texture2D>("EditorHeaderBack");

		}

		public void OnUndoRedo()
		{
			Repaint();
		}

		public override void OnInspectorGUI()
		{
			OverlayHeader();
			base.OnInspectorGUI();
		}

		protected void OverlayHeader()
		{
			
			Rect r = EditorGUILayout.GetControlRect(true, 34);

			float vw = r.width + 18;
			float pad = 6;
			if (backTexture != null)
				GUI.DrawTexture(new Rect(pad, r.yMin + 2, vw - pad * 2, 32), backTexture);
			if (teapotTexture != null)
				GUI.DrawTexture(new Rect(vw - 256 - pad, r.yMin + 2, 256, 32), teapotTexture);
			if (textTexture != null)
				GUI.DrawTexture(new Rect(pad, r.yMin + 2, 256, 32), textTexture);
		}
	}

#endif

}

