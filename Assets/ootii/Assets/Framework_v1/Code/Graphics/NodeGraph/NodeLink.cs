using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// A link is used to connect two nodes together. A link
    /// has single direction. However, there can be mulitple links
    /// between the same nodes.
    /// </summary>
    [Serializable]
    public class NodeLink : ScriptableObject
    {
        /// <summary>
        /// Friendly name to identify the link
        /// </summary>
        public string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Flags the link as usable or not
        /// </summary>
        public bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// Start node
        /// </summary>
        public Node StartNode = null;

        /// <summary>
        /// End node
        /// </summary>
        public Node EndNode = null;

        /// <summary>
        /// Potential list of actions to determine if the link can be traversed
        /// </summary>
        public List<NodeLinkAction> Actions = null;

        /// <summary>
        /// Number of times the link has been traversed
        /// </summary>
        protected int mActivationCount = 0;
        public int ActivationCount
        {
            get { return mActivationCount; }
        }

        /// <summary>
        /// Clear any usage data
        /// </summary>
        public virtual void Reset()
        {
            // We do this to keep from cycling back when our tree has loops.
            if (mActivationCount == 0) { return; }

            // Reset actions if they exist
            if (Actions != null)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    Actions[i].Reset();
                }
            }

            // Set the activation count
            mActivationCount = 0;

            // Reset the end node
            if (EndNode != null)
            {
                EndNode.Reset();
            }
        }

        /// <summary>
        /// Determine if the link should be activated
        /// </summary>
        /// <returns></returns>
        public virtual bool TestActivate(object rUserData = null)
        {
            if (!IsEnabled) { return false; }

            if (Actions != null && Actions.Count > 0)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (!Actions[i].TestActivate(rUserData))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (StartNode.State != EnumNodeState.SUCCEEDED)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Activates the link, running the activation function of any action
        /// </summary>
        public virtual void Activate()
        {
            mActivationCount++;

            if (Actions != null)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    Actions[i].Activate();
                }
            }
        }

        /// <summary>
        /// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
        /// </summary>
        public virtual void Draw()
        {
            Rect lStart = StartNode.CanvasPosition;
            Rect lEnd = EndNode.CanvasPosition;

            Vector3 lStartPos = new Vector3(lStart.x + lStart.width / 2f, lStart.y + lStart.height / 2f, 0f);
            Vector3 lEndPos = new Vector3(lEnd.x + lEnd.width / 2f, lEnd.y + lEnd.height / 2f, 0f);
            DrawArrow(lStartPos, lEndPos);
        }

        /// <summary>
        /// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
        /// </summary>
        public virtual void Draw(Vector3 rEnd)
        {
            Rect lStart = StartNode.CanvasPosition;

            Vector3 lStartPos = new Vector3(lStart.x + lStart.width / 2f, lStart.y + lStart.height / 2f, 0f);
            Vector3 lEndPos = new Vector3(rEnd.x, rEnd.y, 0f);
            DrawArrow(lStartPos, lEndPos);
        }

        /// <summary>
        /// Determines if the panel contains the position
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <returns>True if the point is contained</returns>
        public virtual bool ContainsPoint(Vector2 rPoint)
        {
#if UNITY_EDITOR

            Rect lStart = StartNode.CanvasPosition;
            lStart.position -= StartNode.Canvas.ScrollOffset;

            Rect lEnd = EndNode.CanvasPosition;
            lEnd.position -= EndNode.Canvas.ScrollOffset;

            Vector3 lStartPos = new Vector3(lStart.x + lStart.width / 2f, lStart.y + lStart.height / 2f, 0f);
            Vector3 lEndPos = new Vector3(lEnd.x + lEnd.width / 2f, lEnd.y + lEnd.height / 2f, 0f);

            Vector3 lStartTan = lStartPos + Vector3.right * 30f;
            Vector3 lEndTan = lEndPos + Vector3.left * 30f;

            Vector3[] lPoints = UnityEditor.Handles.MakeBezierPoints(lStartPos, lEndPos, lStartTan, lEndTan, 20);

            int lIndex = NodeEditorStyle.LinkHeaderIndex;
            Rect lHeadPosition = new Rect(lPoints[lIndex].x - 16, lPoints[lIndex].y - 16, 32, 32);

            return lHeadPosition.Contains(rPoint);

#else
            return false;
#endif
        }

        /// <summary>
        /// Renders out the line with an arrow
        /// </summary>
        /// <param name="rStart">Start position</param>
        /// <param name="rEnd">End position</param>
        private void DrawArrow(Vector3 rStart, Vector3 rEnd)
        {
#if UNITY_EDITOR

            bool lIsSelected = (StartNode.Canvas.SelectedLink == this);
            Color lColor = (lIsSelected ? NodeEditorStyle.LinkSelectedColor : (IsEnabled ? NodeEditorStyle.LinkColor : NodeEditorStyle.LinkDisabledColor));

            Vector3 lStartTan = rStart + Vector3.right * 30f;
            Vector3 lEndTan = rEnd + Vector3.left * 30f;

            if (IsEnabled)
            {
                for (int i = 0; i < 2; i++)
                {
                    UnityEditor.Handles.DrawBezier(rStart, rEnd, lStartTan, lEndTan, NodeEditorStyle.LinkShadowColor, null, (i + 1) * 5);
                }
            }

            UnityEditor.Handles.DrawBezier(rStart, rEnd, lStartTan, lEndTan, lColor, null, 2);

            Vector3[] lPoints = UnityEditor.Handles.MakeBezierPoints(rStart, rEnd, lStartTan, lEndTan, 20);

            int lIndex = NodeEditorStyle.LinkHeaderIndex;
            Rect lHeadPosition = new Rect(lPoints[lIndex].x - 16, lPoints[lIndex].y - 16, 32, 32);
            Vector3 lHeadPivot = new Vector3(lHeadPosition.xMin + 16, lHeadPosition.yMin + 16);

            Vector3 lToEnd = (lPoints[lIndex] - lPoints[lIndex + 2]).normalized;

            Vector2 lDirection = new Vector2(lToEnd.x, lToEnd.y);
            float lAngle = Vector2.Angle(Vector2.up, lDirection);
            Vector3 lCross = Vector3.Cross(Vector2.up, lDirection);
            if (lCross.z > 0) { lAngle = 360f - lAngle; }

            UnityEngine.Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(-lAngle, lHeadPivot);
            GUI.DrawTexture(lHeadPosition, (lIsSelected ? NodeEditorStyle.LinkHeadSelected : (IsEnabled ? NodeEditorStyle.LinkHead : NodeEditorStyle.LinkHeadDisabled)));
            GUI.matrix = matrixBackup;

            if (Actions != null && Actions.Count > 0)
            {
                GUI.DrawTexture(lHeadPosition, (lIsSelected ? NodeEditorStyle.LinkActionSelected : (IsEnabled ? NodeEditorStyle.LinkAction : NodeEditorStyle.LinkActionDisabled)));
            }
#endif
        }

        /// <summary>
        /// Creates a node link action and attaches it to this node
        /// </summary>
        /// <param name="rType">Type of action to create</param>
        /// <returns>NodeLinkAction that was created or null</returns>
        public virtual NodeLinkAction CreateLinkAction(Type rType)
        {
            if (Actions == null) { Actions = new List<NodeLinkAction>(); }

            NodeLinkAction lAction = ScriptableObject.CreateInstance(rType) as NodeLinkAction;
            if (lAction != null)
            {
                lAction.name = "z NodeCanvas.NodeLinkAction " + Actions.Count.ToString("D4");
                lAction.hideFlags = HideFlags.HideInHierarchy;

                Actions.Add(lAction);

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.AddObjectToAsset(lAction, StartNode.Canvas.RootAsset);

                UnityEditor.EditorUtility.SetDirty(lAction);
                UnityEditor.EditorUtility.SetDirty(this);
                StartNode.Canvas.SetDirty();
#endif
            }

            return lAction;
        }

        /// <summary>
        /// Removes the action from the list and destroys it
        /// </summary>
        /// <param name="rAction">Action to remove</param>
        public virtual void DestoryLinkAction(NodeLinkAction rAction)
        {
            if (rAction == null) { return; }

            if (Actions != null) { Actions.Remove(rAction); }

            UnityEngine.Object.DestroyImmediate(rAction, true);
            StartNode.Canvas.SetDirty();
        }
    }
}
