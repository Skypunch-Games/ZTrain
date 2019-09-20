using System;
using UnityEngine;
using UnityEditor;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Window that represents the frame
    /// </summary>
    public class NodeEditorWindow : EditorWindow
    {
        /// <summary>
        /// Provides global access to the window
        /// </summary>
        public static NodeEditorWindow Instance = null;

        /// <summary>
        /// Editor that is actually running in the window
        /// </summary>
        public NodeEditor Editor = null;

        /// <summary>
        /// Asset path that we're loading
        /// </summary>
        public string AssetPath = "";

        /// <summary>
        /// Opens the Node Editor window and loads the last session
        /// </summary>
        //[MenuItem("Window/ootii Tools/Node Editor")]
        public static NodeEditorWindow OpenNodeEditor()
        {
            //Debug.Log("NodeEditorWindow.OpenNodeEditor() Instance:" + (Instance == null ? "null" : "value"));

            Instance = GetWindow<NodeEditorWindow>();
            Instance.minSize = new Vector2(400f, 300f);
            Instance.titleContent = new GUIContent("Node Editor");

            Instance.Editor = new NodeEditor();
            Instance.Editor.Initialize(Instance.position.width, Instance.position.height);
            Instance.Editor.RepaintEvent = Instance.OnRepaint;

            Instance.wantsMouseMove = true;

            return Instance;
        }

        /// <summary>
        /// Called multiple times per second on all visible windows.
        /// </summary>
        protected virtual void OnEnable()
        {
            Instance = this;

            if (Editor == null)
            {
                Editor = new NodeEditor();
                //Editor = ScriptableObject.CreateInstance<SpellEditor>();
                Editor.Initialize(Instance.position.width, Instance.position.height);
                Editor.RepaintEvent = Instance.OnRepaint;
            }

            string lPath = Editor.RootAssetPath;
            if (lPath.Length == 0) { lPath = AssetPath; }
            if (lPath.Length > 0) { Editor.LoadRootAsset(lPath); }
        }

        /// <summary>
        /// Called multiple times per second on all visible windows.
        /// </summary>
        protected virtual void OnDisable()
        {
            Editor = null;
        }

        /// <summary>
        /// Called multiple times per second on all visible windows.
        /// </summary>
        protected virtual void Update()
        {
            if (EditorApplication.isCompiling) { return; }
            if (object.ReferenceEquals(Editor, null)) { return; }

            Editor.Update();
        }

        /// <summary>
        /// Render GUI here
        /// </summary>
        protected virtual void OnGUI()
        {
            if (object.ReferenceEquals(Editor, null)) { return; }

            Editor.Reposition(position);
            Editor.OnGUI();
        }

        /// <summary>
        /// Callback for repainting the window
        /// </summary>
        protected virtual void OnRepaint(NodeEditor rEditor)
        {
            Repaint();
        }
    }
}
