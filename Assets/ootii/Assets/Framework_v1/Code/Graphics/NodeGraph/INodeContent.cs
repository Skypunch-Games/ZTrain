namespace com.ootii.Graphics.NodeGraph
{
    /// <summary>
    /// Content that exists inside of a node. It allows the
    /// content to access the wrapper node.
    /// </summary>
    public interface INodeContent
    {
        /// <summary>
        /// Determines if the content is processed immediately. In this case,
        /// the flow is also immediate and no Update() is used.
        /// </summary>
        bool IsImmediate { get; }
    }
}
