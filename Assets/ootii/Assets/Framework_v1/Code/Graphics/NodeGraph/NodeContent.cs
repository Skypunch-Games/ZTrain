using System;
using UnityEngine;

namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Content that exists inside of a node. It allows the
    /// content to access the wrapper node.
    /// </summary>
    public class NodeContent : ScriptableObject, INodeContent
    {
        /// <summary>
        /// Determines if the content is processed immediately. In this case,
        /// the flow is also immediate and no Update() is used.
        /// </summary>
        public virtual bool IsImmediate
        {
            get { return true; }
        }

        /// <summary>
        /// Node that this content belongs to.
        /// </summary>
        protected Node mNode = null;
        public Node Node
        {
            get { return mNode; }
            set { mNode = value; }
        }

        /// <summary>
        /// Friendly name for the content
        /// </summary>
        public string _Name = "";
        public virtual string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
    }
}
