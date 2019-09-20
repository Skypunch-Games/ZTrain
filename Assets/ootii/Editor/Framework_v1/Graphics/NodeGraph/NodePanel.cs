using UnityEngine;
using UnityEditor;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// A NodePanel is a place for menus, toolbars, and windows.
    /// </summary>
    public class NodePanel
    {
        /// <summary>
        /// Determines if we render and get input
        /// </summary>
        public bool IsEnabled = true;

        /// <summary>
        /// Reference to the current editor
        /// </summary>
        public NodeEditor Editor = null;

        /// <summary>
        /// Title of the panel
        /// </summary>
        public string Title = "";

        /// <summary>
        /// Rectangle that the panel renders in
        /// </summary>
        public Rect Position = new Rect(0f, 0f, 50f, 120f);  
        
        /// <summary>
        /// Determine the panel background style
        /// </summary>
        public virtual GUIStyle PanelStyle
        {
            get { return NodeEditorStyle.Panel; }
        }

        /// <summary>
        /// Determine the panel title style
        /// </summary>
        public virtual GUIStyle PanelTitleStyle
        {
            get { return NodeEditorStyle.PanelTitle; }
        }

        /// <summary>
        /// Clears the contents of the panel
        /// </summary>
        public virtual void Clear()
        {
        }

        /// <summary>
        /// Repositions the panel based on the window position
        /// </summary>
        public virtual void Reposition(Rect rWindowRect)
        {
        }

        /// <summary>
        /// Draws the area to the window
        /// </summary>
        public virtual void Draw()
        {
            Rect lPosition = Position;
            GUI.BeginGroup(lPosition);

            float lHeaderPixels = DrawHeader();

            lPosition.x = 0f;
            lPosition.y = lHeaderPixels;
            GUILayout.BeginArea(lPosition);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(PanelStyle);    // Panel start

            DrawTitle();

            GUILayout.Space(36f);                   // Title space before content

            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);                   // Content left padding
            GUILayout.BeginVertical();              // Content start center

            float lLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = (lPosition.width - 20f) * 0.4f;

            DrawContent();

            EditorGUIUtility.labelWidth = lLabelWidth;

            GUILayout.EndVertical();                // Conent end center
            GUILayout.Space(10);                    // Content right padding

            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.EndVertical();                // Panel end
            GUILayout.Space(5f);                    // Panel padding for right shadow
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUI.EndGroup();
        }

        /// <summary>
        /// Draws an area above the title
        /// </summary>
        /// <returns>Amount of vertical pixels needed for the header</returns>
        public virtual float DrawHeader()
        {
            return 0f;
        }

        /// <summary>
        /// Draws in the title area of the window
        /// </summary>
        public virtual void DrawTitle()
        {
            GUI.Label(new Rect(12f, 8f, Position.width - 12f, 20f), Title, PanelTitleStyle);
        }

        /// <summary>
        /// Draws in the content area of the window
        /// </summary>
        public virtual void DrawContent()
        {
        }

        /// <summary>
        /// Provides input handling for the panel
        /// </summary>
        public virtual void ProcessInput()
        {
        }

        /// <summary>
        /// Raised when a canvas node is selected or when the selection is cleared
        /// </summary>
        /// <param name="rNode">Node that is selected or null if deselected</param>
        public virtual void OnCanvasNodeSelected(Node rNode)
        {
        }

        /// <summary>
        /// Raised when a canvas link is selected or when the selection is cleared
        /// </summary>
        /// <param name="rLink">Node link that is selected or null if deselected</param>
        public virtual void OnCanvasLinkSelected(NodeLink rLink)
        {
        }

        /// <summary>
        /// Determines if the panel contains the position
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <returns>True if the point is contained</returns>
        public virtual bool ContainsPoint(Vector2 rPoint)
        {
            if (!IsEnabled) { return false; }

            Rect lPosition = Position;
            lPosition.position -= Editor.ScrollPosition;

            return lPosition.Contains(rPoint);
        }
    }
}