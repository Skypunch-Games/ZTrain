using UnityEngine;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Interface to a canvas that renders nodes. 
    /// </summary>
    public interface INodeCanvas
    {
        /// <summary>
        /// Scriptable object we'll store content to
        /// </summary>
        ScriptableObject RootAsset { get; }

        /// <summary>
        /// Currently selected node.
        /// </summary>
        Node SelectedNode { get; }

        /// <summary>
        /// Currently selected link.
        /// </summary>
        NodeLink SelectedLink { get; }

        /// <summary>
        /// Offset created when the canvas is panned.
        /// </summary>
        Vector2 PanOffset { get; }

        /// <summary>
        /// Offset created when the editor is scrolled.
        /// </summary>
        Vector2 ScrollOffset { get; }

        /// <summary>
        /// Sets the editor as dirty
        /// </summary>
        void SetDirty();

        /// <summary>
        /// Resets all the nodes and links in the canvas
        /// </summary>
        void Reset();
    }
}
