using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Base;

namespace com.ootii.Graphics.NodeGraph
{
    [Serializable]
    public class Node : ScriptableObject
    {
        /// <summary>
        /// ID that represents the node.
        /// </summary>
        public string ID = "";

        /// <summary>
        /// Canvas the node is being drawn to
        /// </summary>
        [NonSerialized]
        public INodeCanvas Canvas = null;

        /// <summary>
        /// Determines if the node is a starting node
        /// </summary>
        public bool IsStartNode = false;

        /// <summary>
        /// Determines if the node is an ending node for when the graph is stopped
        /// </summary>
        public bool IsEndNode = false;

        /// <summary>
        /// Current state of the node
        /// </summary>
        public int State = EnumNodeState.IDLE;

        /// <summary>
        /// Data that is sent to the contents
        /// </summary>
        [NonSerialized]
        public object Data = null;

        /// <summary>
        /// Contents that the node manages
        /// </summary>
        public ScriptableObject _Content = null;
        public ScriptableObject Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        /// <summary>
        /// Links that connect this node (as the start) to other nodes (the ends)
        /// </summary>
        public List<NodeLink> Links = new List<NodeLink>();

        /// <summary>
        /// Position and size of the node
        /// </summary>
        public Rect Position = new Rect(50f, 50f, 150f, 58f);

        /// <summary>
        /// Position taking canvas panning into account
        /// </summary>
        public virtual Rect CanvasPosition
        {
            get
            {
                Rect lPosition = Position;
                lPosition.position += Canvas.PanOffset;

                return lPosition;
            }
        }

        /// <summary>
        /// Determines if the content is processed immediately. In this case,
        /// the flow is also immediate and no Update() is used.
        /// </summary>
        public virtual bool IsImmediate
        {
            get
            {
                INodeContent lContent = _Content as INodeContent;
                if (lContent != null) { return lContent.IsImmediate; }

                return true;
            }
        }

        /// <summary>
        /// Called when the ScriptableObject is started
        /// </summary>
        protected virtual void Awake()
        {
            ID = GetInstanceID().ToString();
        }

        /// <summary>
        /// Clear any usage data
        /// </summary>
        public virtual void Reset()
        {
            Data = null;
            State = EnumNodeState.IDLE;

            for (int i = 0; i < Links.Count; i++)
            {
                Links[i].Reset();
            }
        }

        /// <summary>
        /// Creates a new content element and releases any existing element
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        public virtual ScriptableObject CreateContent(Type rType)
        {
            // Remove the asset that represents the node
            if (_Content != null) { DestroyContent(); }

            // Create a new instance of the content
            if (rType != null)
            {
                // If we're dealing with a scriptable object, store it
                if (com.ootii.Helpers.ReflectionHelper.IsAssignableFrom(typeof(ScriptableObject), rType))
                //if (typeof(ScriptableObject).IsAssignableFrom(rType))
                {
                    _Content = ScriptableObject.CreateInstance(rType) as ScriptableObject;
                    if (_Content is NodeContent) { ((NodeContent)_Content).Node = this; }

#if UNITY_EDITOR
                    if (_Content != null)
                    {
                        // Add the action as an asset. However, we have to name it this:
                        // http://answers.unity3d.com/questions/1164341/can-a-scriptableobject-contain-a-list-of-scriptabl.html
                        _Content.name = "z " + rType.Name + "  " + _Content.GetInstanceID().ToString();
                        _Content.hideFlags = HideFlags.HideInHierarchy;

                        UnityEditor.AssetDatabase.AddObjectToAsset(_Content, Canvas.RootAsset);

                        UnityEditor.EditorUtility.SetDirty(_Content);
                        UnityEditor.EditorUtility.SetDirty(this);
                        Canvas.SetDirty();
                    }
#endif
                }
                // Otherwise, just create the instance
                else
                {
                    _Content = Activator.CreateInstance(rType) as ScriptableObject;
                }
            }

            // Return the content
            return _Content;
        }

        /// <summary>
        /// Destroy the existing content
        /// </summary>
        public void DestroyContent()
        {
            // Remove the asset that represents the node
            if (_Content != null)
            {
                UnityEngine.Object.DestroyImmediate((UnityEngine.Object)_Content, true);
                Canvas.SetDirty();
               
                _Content = null;
            }
        }

        /// <summary>
        /// Draws the node frame and contents.
        /// </summary>
        public virtual void Draw()
        {
            bool lIsSelected = (Canvas == null ? false : Canvas.SelectedNode == this);

            Rect lPosition = CanvasPosition;
            GUI.BeginGroup(lPosition, (lIsSelected ? NodeEditorStyle.NodeSelected : NodeEditorStyle.Node));

            if (IsStartNode)
            {
                Rect lTag = new Rect(10f, 0f, 55f, 25f);
                GUI.Label(lTag, new GUIContent("Start", "This node is a starting node "), (lIsSelected ? NodeEditorStyle.TagSelected : NodeEditorStyle.Tag));
            }

            if (IsEndNode)
            {
                Rect lTag = new Rect(85f, 0f, 55f, 25f);
                GUI.Label(lTag, new GUIContent("End", "This node is an ending node "), (lIsSelected ? NodeEditorStyle.TagSelected : NodeEditorStyle.Tag));
            }

            lPosition.position = new Vector2(0f, 4f);
            GUILayout.BeginArea(lPosition);

            DrawContent();

            GUILayout.EndArea();

            GUI.EndGroup();
        }

        /// <summary>
        /// Draws the node contents
        /// </summary>
        public virtual void DrawContent()
        {
            if (_Content == null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No content", NodeEditorStyle.NodeText);
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUILayout.FlexibleSpace();

                string lTitle = "";

                NodeContent lContent = _Content as NodeContent;
                if (lContent != null) { lTitle = lContent.Name; }

                if (lTitle.Length == 0) { lTitle = BaseNameAttribute.GetName(_Content.GetType()); }

                GUILayout.Label(lTitle, NodeEditorStyle.NodeText);

                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// Determines if the panel contains the position
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <returns>True if the point is contained</returns>
        public virtual bool ContainsPoint(Vector2 rPoint)
        {
            Rect lPosition = Position;
            lPosition.position += Canvas.PanOffset;
            lPosition.position -= Canvas.ScrollOffset;
            
            return lPosition.Contains(rPoint);
        }
    }
}
