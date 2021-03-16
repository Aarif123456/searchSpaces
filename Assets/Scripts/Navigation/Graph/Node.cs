
namespace Thot.GameAI
{
	using UnityEngine;
	
	/// <summary>
    /// Node class to be used with graphs.
    /// </summary>
	public sealed class Node 
	{
		/// <summary>
        /// Valid node indices are positive. This signifies an invalid index.
        /// </summary>
        public const int INVALID_NODE_INDEX = -1;
		
		public GameObject gameObject;

        public Node(){
            Index = INVALID_NODE_INDEX;
        }

        public Node(int index){
            Index = index;
        }

        /// <summary>
        /// Gets or sets the node index. Every node has an index. A valid index is >= 0.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the node position.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Tests if the given node index is invalid.
        /// </summary>
        /// <param name="index">The node index.</param>
        /// <returns>True if the node index is valid. Otherwise, false.</returns>
        public static bool IsInvalidIndex(int index){
            return index == INVALID_NODE_INDEX;
        }

        /// <summary>
        /// Generate a string representation of this node.
        /// </summary>
        /// <returns>A string representation of this node.</returns>
        public override string ToString(){
            return "NodeIndex: " + (IsInvalidIndex(Index) ? "INVALID" : Index.ToString());
        }
	}
}