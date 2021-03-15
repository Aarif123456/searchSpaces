

namespace Thot.GameAI
{
	using System.Collections.Generic;
	
	using UnityEngine;
	
    public delegate float HeuristicDelegate(SparseGraph graph, int nd1, int nd2);
	
	/// <summary>
    /// This searches a graph using the distance between the target node and the 
    /// currently considered node as a heuristic.
    /// </summary>
	public sealed class AStarSearch 
	{
		public AStarSearch(SparseGraph graph, int source, int target)
        {
            Graph = graph;
            Source = source;
            Target = target;
            ShortestPathTree = new List<Edge>(Graph.NumNodes);
            SearchFrontier = new List<Edge>(Graph.NumNodes);
            GCosts = new List<float>(Graph.NumNodes);
            FCosts = new List<float>(Graph.NumNodes);

            for (int i = 0; i < graph.NumNodes; i++)
            {
                ShortestPathTree.Add(null);
                SearchFrontier.Add(null);
                GCosts.Add(0);
                FCosts.Add(0);
            }

            Search(EuclideanDistance);
        }

        /// <summary>
        /// Gets the list of edges that comprise the shortest path tree - a directed subtree of the
        /// graph that encapsulates the best paths from every node on the SPT to the source node.
        /// </summary>
        public List<Edge> ShortestPathTree { get; private set; }

        /// <summary>
        /// Gets the graph to be searched.
        /// </summary>
        public SparseGraph Graph { get; private set; }

        /// <summary>
        /// Gets an indexed (by node) list of 'parent' edges leading to nodes connected to the SPT
        /// but that have not been added to the SPT yet. This is a little like the stack or queue
        /// used in BST and DST searches. This is also referred to as the search fringe.
        /// </summary>
        public List<Edge> SearchFrontier { get; private set; }

        /// <summary>
        /// Gets the list of g-costs indexed by node. Contains the 'actual' accumulative cost to
        /// that node.
        /// </summary>
        public List<float> GCosts { get; private set; }

        /// <summary>
        /// Gets the list of f-costs indexed by node. Contains the cost of adding GCosts[n] to the
        /// heuristic cost from n to the target node. The priority queue indexes into this list.
        /// </summary>
        public List<float> FCosts { get; private set; }

        /// <summary>
        /// Gets the source node index.
        /// </summary>
        public int Source { get; private set; }

        /// <summary>
        /// Gets the source node index.
        /// </summary>
        public int Target { get; private set; }

        /// <summary>
        /// Gets the number of nodes searched. Used for performance measuring.
        /// </summary>
        public int NodesSearched { get; private set; }

        /// <summary>
        /// Calculate the straight line distance from node nd1 to node nd2.
        /// </summary>
        /// <param name="graph">The search graph.</param>
        /// <param name="nd1">The first node index.</param>
        /// <param name="nd2">The second node index.</param>
        /// <returns>The straight line distance from node nd1 to node nd2.</returns>
        public static float EuclideanDistance(SparseGraph graph, int nd1, int nd2)
        {
            return (graph.GetNode(nd1).Position - graph.GetNode(nd2).Position).magnitude;
        }

        /// <summary>
        /// Gets the total cost to the target.
        /// </summary>
        /// <returns>The total cost to the target.</returns>
        public float GetCostToTarget()
        {
            return GCosts[Target];
        }

        /// <summary>
        /// Gets a list of node indexes that comprise the shortest path from the source to the
        /// target.
        /// </summary>
        /// <returns>
        /// A list of node indexes that comprise the shortest path from the source to the target.
        /// </returns>
        public List<int> GetPathToTarget()
        {
            var path = new List<int>();

            // just return an empty path if no target or no path found
            if (Target < 0)
            {
                return path;
            }

            int nd = Target;

            path.Insert(0, nd);

            while ((nd != Source) && (ShortestPathTree[nd] != null))
            {
                nd = ShortestPathTree[nd].From;

                path.Insert(0, nd);
            }

            return path;
        }

        private void Search(HeuristicDelegate calculate)
        {
            NodesSearched = 0; // used for performance measuring

            // create an indexed priority queue of nodes. The nodes with the
            // lowest overall F cost (G+H) are positioned at the front.
            var pq = new IndexedPriorityQueueLow(FCosts, Graph.NumNodes);

            // put the source node on the queue
            pq.Insert(Source);

            // while the queue is not empty
            while (!pq.Empty())
            {
                // get lowest cost node from the queue
                int nextClosestNode = pq.Pop();
                NodesSearched++;

                // move this node from the frontier to the spanning tree
                ShortestPathTree[nextClosestNode] =
                    SearchFrontier[nextClosestNode];

                // if the target has been found exit
                if (nextClosestNode == Target)
                {
                    return;
                }

                // now to test all the edges attached to this node
                foreach (Edge curEdge in Graph.Edges[nextClosestNode])
                {
                    // calculate (H) the heuristic cost from this node to
                    // the target                     
                    float hCost = calculate(Graph, Target, curEdge.To);

                    // calculate (G) the 'real' cost to this node from the source 
                    float gCost = GCosts[nextClosestNode] + curEdge.Cost;

                    // if the node has not been added to the frontier, add it and
                    // update the G and F costs
                    if (SearchFrontier[curEdge.To] == null)
                    {
                        FCosts[curEdge.To] = gCost + hCost;
                        GCosts[curEdge.To] = gCost;

                        pq.Insert(curEdge.To);

                        SearchFrontier[curEdge.To] = curEdge;
                    }

                    // if this node is already on the frontier but the cost to
                    // get here is cheaper than has been found previously, update
                    // the node costs and frontier accordingly.
                    else if ((gCost < GCosts[curEdge.To]) &&
                        (ShortestPathTree[curEdge.To] == null))
                    {
                        FCosts[curEdge.To] = gCost + hCost;
                        GCosts[curEdge.To] = gCost;

                        pq.ChangePriority(curEdge.To);

                        SearchFrontier[curEdge.To] = curEdge;
                    }
                }
            }
        }
	}
}
