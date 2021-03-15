
namespace Thot.GameAI
{
	using System.Text;
	
	using UnityEngine;
	
	/// <summary>
    /// Class to define an edge connecting two nodes. An edge has an associated cost.
    /// </summary>
	public sealed class Edge 
	{
		public GameObject gameObject;
		
		public Edge(int from, int to, float cost)
        {
            Cost = cost;
            From = from;
            To = to;
        }

        public Edge(int from, int to)
            : this(from, to, 1.0f)
        {
        }

        public Edge()
            : this(
                Node.INVALID_NODE_INDEX,
                Node.INVALID_NODE_INDEX,
                1.0f)
        {
        }

        public Edge(Edge src)
            : this(
                src.From,
                src.To,
                src.Cost)
        {
        }

        /// <summary>
        /// Gets or sets the edge's 'from' node index An edge connects two nodes. Valid node indices
        /// are always positive.
        /// </summary>
        public int From { get; set; }

        /// <summary>
        /// Gets or sets the edge's 'to' node index. An edge connects two nodes. Valid node indices
        /// are always positive.
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// Gets or sets the cost of traversing the edge.
        /// </summary>
        public float Cost { get; set; }

        /// <summary>
        /// Generate a string representation of this edge.
        /// </summary>
        /// <returns>A string representation of this edge.</returns>
        public override string ToString()
        {
            var edgeText = new StringBuilder();
            edgeText.AppendFormat("{0}--[{1}]-->{2}", From, Cost, To);
            return edgeText.ToString();
        }
	}
}
