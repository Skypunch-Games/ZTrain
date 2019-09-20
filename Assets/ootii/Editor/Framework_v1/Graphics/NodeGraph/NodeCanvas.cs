using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// NodeCanvas is the area that we render our nodes in. It may not be
    /// the full size of the window (for toolbars and such).
    /// </summary>
    public class NodeCanvas : NodePanel, INodeCanvas
    {
        /// <summary>
        /// Delegate for events dealing with nodes
        /// </summary>
        public delegate void NodeCanvasNodeDelegate(Node rNode);

        /// <summary>
        /// Delegate for links dealing with nodes
        /// </summary>
        public delegate void NodeCanvasLinkDelegate(NodeLink rLink);

        /// <summary>
        /// Scriptable object we'll store content to
        /// </summary>
        public ScriptableObject RootAsset
        {
            get { return Editor.RootAsset; }
        }

        /// <summary>
        /// Offset created when the canvas is panned
        /// </summary>
        protected Vector2 mPanOffset = Vector2.zero;
        public Vector2 PanOffset
        {
            get { return mPanOffset; }
        }

        /// <summary>
        /// Offset created when the canvas is panned
        /// </summary>
        public Vector2 ScrollOffset
        {
            get { return Editor.ScrollPosition; }
        }

        /// <summary>
        /// Currently selected node.
        /// </summary>
        public Node mSelectedNode = null;
        public Node SelectedNode
        {
            get { return mSelectedNode; }
        }

        /// <summary>
        /// Currently selected link.
        /// </summary>
        public NodeLink mSelectedLink = null;
        public NodeLink SelectedLink
        {
            get { return mSelectedLink; }
        }

        /// <summary>
        /// Nodes that are on the canvas.
        /// </summary>
        public List<Node> Nodes = new List<Node>();

        /// <summary>
        /// Event for when a node is added
        /// </summary>
        public NodeCanvasNodeDelegate NodeAddedEvent = null;

        /// <summary>
        /// Event for when a node is removed
        /// </summary>
        public NodeCanvasNodeDelegate NodeRemovedEvent = null;

        /// <summary>
        /// Event for when a node is selected
        /// </summary>
        public NodeCanvasNodeDelegate NodeSelectedEvent = null;

        /// <summary>
        /// Event for when a link is added
        /// </summary>
        public NodeCanvasLinkDelegate LinkAddedEvent = null;

        /// <summary>
        /// Event for when a link is removed
        /// </summary>
        public NodeCanvasLinkDelegate LinkRemovedEvent = null;

        /// <summary>
        /// Event for when a link is selected
        /// </summary>
        public NodeCanvasLinkDelegate LinkSelectedEvent = null;

        /// <summary>
        /// Node that is actively being moved.
        /// </summary>
        private Node mActiveNode = null;

        /// <summary>
        /// Link that is actively being used.
        /// </summary>
        private NodeLink mActiveLink = null;

        private float mZoom = 1f;

        private Vector2 mZoomPosition = Vector2.zero;

        private bool mIsLinking = false;

        private bool mIsPanning = false;

        private Vector2 mDragStart = Vector2.zero;

        private Vector2 mDragOffset = Vector2.zero;

        /// <summary>
        /// Repositions the panel based on the window position
        /// </summary>
        public override void Reposition(Rect rWindowRect)
        {
            Position.x = 0;
            Position.y = 0;
            Position.width = rWindowRect.width;
            Position.height = rWindowRect.height;
        }

        /// <summary>
        /// Resets the nodes and links that are in the canvas
        /// </summary>
        public virtual void Reset()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Reset();
            }
        }

        /// <summary>
        /// Clears the contents of the panel
        /// </summary>
        public override void Clear()
        {
            mSelectedNode = null;
            mActiveNode = null;
            mActiveLink = null;
            mIsPanning = false;
            mIsLinking = false;
                       
            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    Nodes[i].Links.Clear();
            //}

            Nodes.Clear();
        }

        /// <summary>
        /// Draws the area to the window
        /// </summary>
        public override void Draw()
        {
            // Render the background using any panning we created
            if (Event.current.type == EventType.Repaint)
            {
                float lHeight = Mathf.Max(Position.height, 600f);

                // Draw Background when Repainting
                // Size in pixels the inividual background tiles will have on screen
                float lZoomWidth = mZoom / NodeEditorStyle.CanvasBackground.width;
                float lZoomHeight = mZoom / NodeEditorStyle.CanvasBackground.height;

                // Offset of the grid relative to the GUI origin
                Vector2 lOffset = (mZoomPosition + mPanOffset) / mZoom;

                // Rect in UV space that defines how to tile the background texture
                Rect lWindow = new Rect(Position.x, Position.y, Position.width, lHeight);
                Rect uvDrawRect = new Rect(-lOffset.x * lZoomWidth, (lOffset.y - lHeight) * lZoomHeight, Position.width * lZoomWidth, lHeight * lZoomHeight);
                GUI.DrawTextureWithTexCoords(lWindow, NodeEditorStyle.CanvasBackground, uvDrawRect);
            }

            // Render out the links first
            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int j = 0; j < Nodes[i].Links.Count; j++)
                {
                    Nodes[i].Links[j].Draw();
                }
            }

            // Render out the new link
            if (mIsLinking && mActiveLink != null)
            {
                mActiveLink.Draw(Editor.MousePosition + Editor.ScrollPosition);
            }

            // Render out the nodes last
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Draw();
            }

            //Rect lMouse = new Rect(Editor.MousePosition.x, Editor.MousePosition.y, 200f, 20f);
            //GUI.Label(lMouse, "mouse:" + Editor.MousePosition.ToString());
            //GUI.Label(new Rect(0, 0, 200f, 20f), "mouse:" + Editor.MousePosition.ToString());
        }

        /// <summary>
        /// Provides input handling for the panel
        /// </summary>
        public override void ProcessInput()
        {
            Event lCurrentEvent = Event.current;
            EventType lCurrentEventType = lCurrentEvent.type;

            Node lClickedNode = null;
            NodeLink lClickedLink = null;

            switch (lCurrentEventType)
            {
                // Handles keypresses
                case EventType.KeyUp:

                    switch (lCurrentEvent.keyCode)
                    {
                        case KeyCode.Delete:

                            if (GUIUtility.keyboardControl == 0)
                            {
                                if (mSelectedNode != null) { RemoveNode(mSelectedNode); }
                                if (mSelectedLink != null) { RemoveLink(mSelectedLink); }

                                Editor.Repaint();
                            }

                            break;
                    }

                    break;

                // Handles the mouse down event
                case EventType.MouseDown:

                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        if (Nodes[i].ContainsPoint(lCurrentEvent.mousePosition))
                        {
                            lClickedLink = null;
                            lClickedNode = Nodes[i];
                        }

                        if (lClickedNode == null)
                        {
                            for (int j = 0; j < Nodes[i].Links.Count; j++)
                            {
                                if (Nodes[i].Links[j].ContainsPoint(lCurrentEvent.mousePosition))
                                {
                                    lClickedLink = Nodes[i].Links[j];
                                    break;
                                }
                            }
                        }

                        if (lClickedNode != null || lClickedLink != null) { break; }
                    }

                    mIsPanning = false;

                    // With left mouse button, select
                    if (lCurrentEvent.button == 0)
                    {
                        if (mIsLinking && mActiveLink != null)
                        {
                            if (lClickedNode != null && lClickedNode != mActiveLink.StartNode)
                            {
                                mActiveLink.EndNode = lClickedNode;
                                AddLink(mActiveLink);
                            }

                            mActiveLink = null;
                            mActiveNode = null;

                            mSelectedNode = null;
                        }
                        else
                        {
                            if (lClickedNode != null) { GUIUtility.keyboardControl = 0; }
                            if (lClickedNode != mSelectedNode && NodeSelectedEvent != null) { NodeSelectedEvent(lClickedNode); }

                            if (lClickedLink != null) { GUIUtility.keyboardControl = 0; }
                            if (lClickedLink != mSelectedLink && LinkSelectedEvent != null) { LinkSelectedEvent(lClickedLink); }

                            mActiveNode = lClickedNode;
                            mSelectedNode = lClickedNode;

                            mActiveLink = lClickedLink;
                            mSelectedLink = lClickedLink;

                            if (lClickedNode != null)
                            {
                                mDragStart = lCurrentEvent.mousePosition;
                                mDragOffset = Vector2.zero;
                            }
                        }

                        mIsLinking = false;

                        Editor.Repaint();
                    }
                    // With right mouse button, show the menu
                    else if (lCurrentEvent.button == 1)
                    {
                        mActiveNode = lClickedNode;
                        mActiveLink = lClickedLink;

                        if (mActiveNode != null)
                        {
                            GenericMenu lMenu = new UnityEditor.GenericMenu();
                            lMenu.AddItem(new GUIContent("Add Link"), false, OnStartAddLink, mActiveNode);
                            lMenu.AddSeparator("");
                            lMenu.AddItem(new GUIContent("Delete Node"), false, OnRemoveNode, mActiveNode);
                            lMenu.ShowAsContext();
                            lCurrentEvent.Use();
                        }
                        else if (mActiveLink != null)
                        {
                            GenericMenu lMenu = new UnityEditor.GenericMenu();
                            lMenu.AddItem(new GUIContent("Delete Link"), false, OnRemoveLink, mActiveLink);
                            lMenu.ShowAsContext();
                            lCurrentEvent.Use();
                        }
                        else
                        {
                            GenericMenu lMenu = new UnityEditor.GenericMenu();
                            lMenu.AddItem(new GUIContent("Add Node"), false, OnAddNode, typeof(Node));
                            lMenu.ShowAsContext();
                            lCurrentEvent.Use();
                        }
                    }
                    // With right mouse buttion, drag the canvas
                    else if (lCurrentEvent.button == 2)
                    {
                        mIsPanning = true;
                        mDragStart = lCurrentEvent.mousePosition;
                        mDragOffset = Vector2.zero;
                    }

                    break;

                // Handles the mouse drag event
                case EventType.MouseDrag:

                    // Pan the canvas as needed
                    if (mIsPanning)
                    {
                        Vector2 lDragOffset = mDragOffset;
                        mDragOffset = lCurrentEvent.mousePosition - mDragStart;

                        Vector2 lOffset = (mDragOffset - lDragOffset) * mZoom;
                        mPanOffset = mPanOffset + lOffset;

                        Editor.Repaint();
                    }
                    // Move the active node as needed
                    else if (mActiveNode != null)
                    {
                        Vector2 lDragOffset = mDragOffset;
                        mDragOffset = lCurrentEvent.mousePosition - mDragStart;

                        Vector2 lOffset = (mDragOffset - lDragOffset) * mZoom;
                        mActiveNode.Position.x = mActiveNode.Position.x + lOffset.x;
                        mActiveNode.Position.y = mActiveNode.Position.y + lOffset.y;

                        Editor.Repaint();
                    }

                    break;

                // Handles the mouse up event
                case EventType.MouseUp:

                    if (mActiveNode != null)
                    {
                        if (mDragOffset.sqrMagnitude > 0f)
                        {
                            mDragOffset = Vector2.zero;

                            // Reorder the links left to right
                            for (int i = 0; i < Nodes.Count; i++)
                            {
                                bool lReorder = false;
                                for (int j = 0; j < Nodes[i].Links.Count; j++)
                                {
                                    if (Nodes[i].Links[j].EndNode == mActiveNode)
                                    {
                                        lReorder = true;
                                        break;
                                    }
                                }

                                if (lReorder)
                                {
                                    Nodes[i].Links = Nodes[i].Links.OrderBy(x => x.EndNode.Position.x).ToList<NodeLink>();
                                }
                            }

                            // Flag the canvas as dirty
                            EditorUtility.SetDirty(mActiveNode);
                            SetDirty();
                        }
                    }

                    // Ensure we don't pan or move a node
                    mIsPanning = false;
                    mActiveNode = null;

                    break;
            }
        }

        /// <summary>
        /// Flags the editor as dirty
        /// </summary>
        public void SetDirty()
        {
            Editor.SetDirty();
        }

        /// <summary>
        /// Used to create a node with the specified type
        /// </summary>
        /// <param name="rUserData">Type argument that defines the type of node to create</param>
        public void AddNode(Type rType)
        {
            if (rType == null) { return; }

            Node lNode = ScriptableObject.CreateInstance(rType) as Node;
            lNode.Canvas = this;
            lNode.Position.x = Editor.MousePosition.x - mPanOffset.x + Editor.ScrollPosition.x;
            lNode.Position.y = Editor.MousePosition.y - mPanOffset.y + Editor.ScrollPosition.y;

            Nodes.Add(lNode);

            // Add the action as an asset. However, we have to name it this:
            // http://answers.unity3d.com/questions/1164341/can-a-scriptableobject-contain-a-list-of-scriptabl.html
            lNode.name = "z NodeCanvas.Node " + Nodes.Count.ToString("D4");
            lNode.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(lNode, Editor.RootAsset);

            UnityEditor.EditorUtility.SetDirty(lNode);
            SetDirty();

            // Report the node activity
            if (NodeAddedEvent != null) { NodeAddedEvent(lNode); }

            // Report the selected node
            mSelectedLink = null;
            mSelectedNode = lNode;
            if (LinkSelectedEvent != null) { LinkSelectedEvent(null); }
            if (NodeSelectedEvent != null) { NodeSelectedEvent(lNode); }

            Editor.Repaint();
        }

        /// <summary>
        /// Remove the node from the canvas
        /// </summary>
        /// <param name="rNode"></param>
        public void RemoveNode(Node rNode)
        {
            if (rNode == null) { return; }

            if (mActiveNode == rNode) { mActiveNode = null; }

            if (mSelectedNode == rNode)
            {
                mSelectedNode = null;
                if (NodeSelectedEvent != null) { NodeSelectedEvent(null); }
            }

            for (int i = rNode.Links.Count - 1; i >= 0; i--)
            {
                RemoveLink(rNode.Links[i]);
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int j = Nodes[i].Links.Count - 1; j >= 0; j--)
                {
                    if (Nodes[i].Links[j].EndNode == rNode)
                    {
                        RemoveLink(Nodes[i].Links[j]);
                    }
                }
            }

            Nodes.Remove(rNode);

            // Report the node activity
            if (NodeRemovedEvent != null) { NodeRemovedEvent(rNode); }

            // Remove the asset that represents the node
            UnityEngine.Object.DestroyImmediate(rNode, true);

            SetDirty();

            Editor.Repaint();
        }

        /// <summary>
        /// Adds a link to the canvas based on the contents of the link.
        /// </summary>
        /// <param name="rLink">NodeLink that we're adding</param>
        public void AddLink(NodeLink rLink)
        {
            if (rLink == null || rLink.StartNode == null || rLink.EndNode == null) { return; }

            rLink.StartNode.Links.Add(rLink);

            // Add the action as an asset. However, we have to name it this:
            // http://answers.unity3d.com/questions/1164341/can-a-scriptableobject-contain-a-list-of-scriptabl.html
            rLink.name = "z NodeCanvas.Link " + Nodes.Count.ToString("D4");
            rLink.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(rLink, Editor.RootAsset);

            UnityEditor.EditorUtility.SetDirty(rLink);
            UnityEditor.EditorUtility.SetDirty(rLink.StartNode);
            SetDirty();

            // Report the link activity
            if (LinkAddedEvent != null) { LinkAddedEvent(rLink); }

            mSelectedLink = rLink;
            mSelectedNode = null;
            if (NodeSelectedEvent != null) { NodeSelectedEvent(null); }
            if (LinkSelectedEvent != null) { LinkSelectedEvent(rLink); }

            Editor.Repaint();
        }

        /// <summary>
        /// Removes a link from the canvas based on the contents of the link.
        /// </summary>
        /// <param name="rLink">NodeLink to remove.</param>
        public void RemoveLink(NodeLink rLink)
        {
            if (rLink == null) { return; }

            if (mActiveLink == rLink) { mActiveLink = null; }

            if (mSelectedLink == rLink)
            {
                mSelectedLink = null;
                if (LinkSelectedEvent != null) { LinkSelectedEvent(null); }
            }

            if (rLink.StartNode != null) { rLink.StartNode.Links.Remove(rLink); }

            // Report the link activity
            if (LinkRemovedEvent != null) { LinkRemovedEvent(rLink); }

            // Remove the asset that represents the link
            UnityEngine.Object.DestroyImmediate(rLink, true);

            UnityEditor.EditorUtility.SetDirty(rLink.StartNode);
            SetDirty();

            Editor.Repaint();
        }

        /// <summary>
        /// Used to create a node with the specified type
        /// </summary>
        /// <param name="rUserData">Type argument that defines the type of node to create</param>
        private void OnAddNode(object rUserData)
        {
            AddNode(rUserData as Type);
        }

        /// <summary>
        /// Used to remove an existing node
        /// </summary>
        /// <param name="rUserData">Node to remove</param>
        private void OnRemoveNode(object rUserData)
        {
            RemoveNode(rUserData as Node);
        }

        /// <summary>
        /// Used as the start of adding a link between nodes
        /// </summary>
        /// <param name="rUserData">Node argument that is the start</param>
        private void OnStartAddLink(object rUserData)
        {
            if (rUserData == null) { return; }

            Node lNode = rUserData as Node;
            if (lNode == null) { return; }

            mActiveLink = ScriptableObject.CreateInstance<NodeLink>();
            mActiveLink.StartNode = lNode;

            mIsLinking = true;
        }

        /// <summary>
        /// Used to remove an existing link
        /// </summary>
        /// <param name="rUserData">link to remove</param>
        private void OnRemoveLink(object rUserData)
        {
            RemoveLink(rUserData as NodeLink);
        }

        /// <summary>
        /// Determines if the panel contains the position
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <returns>True if the point is contained</returns>
        public override bool ContainsPoint(Vector2 rPoint)
        {
            if (!IsEnabled) { return false; }

            Rect lPosition = Position;
            //lPosition.position -= Editor.ScrollPosition;

            return lPosition.Contains(rPoint);
        }
    }
}
