using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Editor represents the entire editor including canvas and panels
    /// </summary>
    public class NodeEditor
    {
        /// <summary>
        /// Delegate for editor events
        /// </summary>
        public delegate void NodeEditorDelegate(NodeEditor rEditor);

        /// <summary>
        /// Path to the root asset we are loading
        /// </summary>
        public string RootAssetPath = "";

        /// <summary>
        /// Asset all other scriptable objects are stored in
        /// </summary>
        public ScriptableObject RootAsset = null;

        /// <summary>
        /// Rectangle that the panel renders in
        /// </summary>
        public Rect Position = new Rect(0f, 0f, 50f, 100f);

        /// <summary>
        /// Canvas representing the main areas
        /// </summary>
        protected NodeCanvas mCanvas = null;
        public NodeCanvas Canvas
        {
            get { return mCanvas; }
        }

        /// <summary>
        /// Individual panel for rendering information
        /// </summary>
        protected List<NodePanel> mPanels = new List<NodePanel>();
        public List<NodePanel> Panels
        {
            get { return mPanels; }
        }

        /// <summary>
        /// Determines which panel will process the input
        /// </summary>
        protected NodePanel mInputHandler = null;

        /// <summary>
        /// Provides a way for the editor window to get repaint requests
        /// </summary>
        [NonSerialized]
        public NodeEditorDelegate RepaintEvent = null;

        /// <summary>
        /// Current mouse position in the editor
        /// </summary>
        [NonSerialized]
        public Vector2 MousePosition = Vector2.zero;

        /// <summary>
        /// Determines where the editor is currently scrolled to
        /// </summary>
        [NonSerialized]
        public Vector2 ScrollPosition = Vector2.zero;

        /// <summary>
        /// Initializes the editor
        /// </summary>
        public virtual void Initialize(float rWidth, float rHeight)
        {
            mCanvas = new NodeCanvas();
            //mCanvas = ScriptableObject.CreateInstance<NodeCanvas>();
            mCanvas.Editor = this;
            mCanvas.Position = new Rect(0, 0, rWidth, rHeight);
        }

        /// <summary>
        /// Repositions the editor based on the window position
        /// </summary>
        public void Reposition(Rect rWindowRect)
        {
            Position.x = rWindowRect.x;
            Position.y = rWindowRect.y;
            Position.width = rWindowRect.width;
            Position.height = rWindowRect.height;

            mCanvas.Reposition(Position);

            for (int i = 0; i < mPanels.Count; i++)
            {
                mPanels[i].Reposition(Position);
            }
        }

        /// <summary>
        /// Loads the root asset at the specified path
        /// </summary>
        /// <param name="rAssetPath">Path to the asset to edit</param>
        public virtual void LoadRootAsset(string rAssetPath)
        {
            RootAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(rAssetPath) as ScriptableObject;
        }

        /// <summary>
        /// Called multiple times per second on all visible windows.
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Update the editor based on the window
        /// </summary>
        public virtual void OnGUI()
        {
            float lScrollBarWidth = 15f;

            float lHeight = Mathf.Max(Position.height, 600f);
            ScrollPosition = GUI.BeginScrollView(new Rect(0, 0, Position.width, Position.height), ScrollPosition, new Rect(0, 0, Position.width - lScrollBarWidth, lHeight - lScrollBarWidth), false, true);

            mCanvas.Draw();

            for (int i = 0; i < mPanels.Count; i++)
            {
                if (mPanels[i].IsEnabled)
                {
                    mPanels[i].Draw();
                }
            }

            GUI.EndScrollView();

            // Process input after drawing or we could change the layout
            if (!EditorApplication.isCompiling)
            {
                ProcessInput();
            }
        }

        /// <summary>
        /// Process input to determine if we're resizing, dagging, etc
        /// </summary>
        public virtual void ProcessInput()
        {
            Event lCurrentEvent = Event.current;
            if (lCurrentEvent == null) { return; }

            EventType lCurrentEventType = lCurrentEvent.type;
            MousePosition = lCurrentEvent.mousePosition;

            switch (lCurrentEventType)
            {
                case EventType.MouseDown:

                    mInputHandler = null;

                    if (GUIUtility.hotControl > 0) { return; }

                    for (int i = 0; i < mPanels.Count; i++)
                    {
                        if (mPanels[i].ContainsPoint(lCurrentEvent.mousePosition))
                        {
                            mInputHandler = mPanels[i];
                            break;
                        }
                    }

                    if (mInputHandler == null && mCanvas.ContainsPoint(lCurrentEvent.mousePosition))
                    {
                        mInputHandler = mCanvas;
                    }

                    break;

                case EventType.MouseMove:

                    Repaint();

                    break;
            }

            if (!object.ReferenceEquals(mInputHandler, null))
            {
                mInputHandler.ProcessInput();
            }
        }

        /// <summary>
        /// Causes the editor to repaint
        /// </summary>
        public virtual void Repaint()
        {
            if (RepaintEvent != null) { RepaintEvent(this); }
        }

        /// <summary>
        /// Used to inform the editor that the object being managed is dirty. However,
        /// this does not set the ScriptableObject itself.
        /// </summary>
        /// <param name="rIsDirty"></param>
        public virtual void SetDirty(bool rIsDirty = true)
        {
        }
    }
}