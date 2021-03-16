
namespace Thot.GameAI
{
	using System.Collections.Generic;
	using System.Text;
	
	using UnityEngine;
	
	/// <summary>
    /// Class implementing a sparse graph.
    /// </summary>
	public sealed class SparseGraph 
	{
		public SparseGraph(bool isDigraph){
            Edges = new List<LinkedList<Edge>>();
            Nodes = new List<Node>();
            NextNodeIndex = 0;
            IsDigraph = isDigraph;
        }

        /// <summary>
        /// Gets the nodes that comprise this graph.
        /// </summary>
        public List<Node> Nodes { get; private set; }

        /// <summary>
        /// Gets a list of adjacency edge lists. (each node index keys into the 
        /// list of edges associated with that node).
        /// </summary>
        public List<LinkedList<Edge>> Edges { get; private set; }

        /// <summary>
        /// Gets the next free node index.
        /// </summary>
        public int NextFreeNodeIndex
        {
            get { return NextNodeIndex; }
        }

        /// <summary>
        /// Gets the number of active + inactive nodes present in the graph.
        /// </summary>
        public int NumNodes
        {
            get { return Nodes.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the graph is directed.
        /// </summary>
        public bool IsDigraph { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the graph contains no nodes.
        /// </summary>
        public bool IsEmpty
        {
            get { return Nodes.Count == 0; }
        }

        /// <summary>
        /// Gets or sets the index of next node to be added.
        /// </summary>
        public int NextNodeIndex { get; set; }

        /// <summary>
        /// Determines the number of active nodes present in the graph (this
        /// method's performance can be improved greatly by caching the value).
        /// </summary>
        /// <returns>The number of active nodes present in the graph.</returns>
        public int NumActiveNodes(){
            int numActiveNodes = 0;

            for (int n = 0; n < Nodes.Count; ++n){
                if (!Node.IsInvalidIndex(Nodes[n].Index)){
                    ++numActiveNodes;
                }
            }

            return numActiveNodes;
        }

        /// <summary>
        /// Determines the total number of edges present in the graph.
        /// </summary>
        /// <returns>The total number of edges present in the graph.</returns>
        public int NumEdges(){
            int numEdges = 0;

            foreach (LinkedList<Edge> edgeList in Edges){
                numEdges += edgeList.Count;
            }

            return numEdges;
        }

        /// <summary>
        /// Clears the graph ready for new node insertions.
        /// </summary>
        public void Clear(){
            NextNodeIndex = 0;
            Nodes.Clear();
            Edges.Clear();
        }

        /// <summary>
        /// Tests if a node with the given index is present in the graph.
        /// </summary>
        /// <param name="nodeIndex">The node index.</param>
        /// <returns>
        /// True if a node with the given index is present in the graph.
        /// </returns>
        public bool IsNodePresent(int nodeIndex){
            return (nodeIndex < Nodes.Count) && !Node.IsInvalidIndex(Nodes[nodeIndex].Index);
        }

        /// <summary>
        /// Tests if an edge with the given from/to is present in the graph.
        /// </summary>
        /// <param name="from">The from node index.</param>
        /// <param name="to">The to node index.</param>
        /// <returns>
        /// True if an edge with the given from/to is present in the graph.
        /// </returns>
        public bool IsEdgePresent(int from, int to){
            if (IsNodePresent(from) && IsNodePresent(to)){
                foreach (Edge edge in Edges[from]){
                    if (edge.To == to){
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Method for obtaining a reference to a specific node.
        /// </summary>
        /// <param name="index">The node index.</param>
        /// <returns>The node with the given index.</returns>
        public Node GetNode(int index){
            if (index >= Nodes.Count || index < 0){
                Debug.LogError("SparseGraph.GetNode: invalid index.");
                throw new System.Exception("SparseGraph.GetNode: invalid index.");
            }

            return Nodes[index];
        }

        /// <summary>
        /// Method for obtaining a reference to a specific edge.
        /// </summary>
        /// <param name="from">The from index.</param>
        /// <param name="to">The two index.</param>
        /// <returns>The edge between the given node indices.</returns>
        public Edge GetEdge(int from, int to){
            if (from >= Nodes.Count || from < 0 || Node.IsInvalidIndex(Nodes[from].Index)){
                Debug.LogError("SparseGraph.GetEdge: invalid 'from' index.");
                throw new System.Exception("SparseGraph.GetEdge: invalid 'from' index.");
            }

            if (to >= Nodes.Count || to < 0 || Node.IsInvalidIndex(Nodes[to].Index)){
                Debug.LogError("SparseGraph.GetEdge: invalid 'to' index.");
                throw new System.Exception("SparseGraph.GetEdge: invalid 'to' index.");
            }

            foreach (Edge edge in Edges[from]){
                if (edge.To == to){
                    return edge;
                }
            }

            Debug.LogError("SparseGraph.GetEdge: edge does not exist.");
            throw new System.Exception("SparseGraph.GetEdge: edge does not exist.");
        }

        /// <summary>
        /// Use this to add an edge to the graph. The method will ensure that the edge passed as a
        /// parameter is valid before adding it to the graph. If the graph is a digraph then a
        /// similar edge connecting the nodes in the opposite direction will be automatically added.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public void AddEdge(Edge edge){
            // first make sure the from and to nodes exist within the graph 
            if (edge.From >= NextNodeIndex || edge.To >= NextNodeIndex){
                Debug.LogError("SparseGraph.AddEdge: invalid node index.");
                throw new System.Exception("SparseGraph.AddEdge: invalid node index.");
            }

            // make sure both nodes are active before adding the edge
            if (Node.IsInvalidIndex(Nodes[edge.To].Index) ||
                Node.IsInvalidIndex(Nodes[edge.From].Index)){
                return;
            }

            // add the edge, first making sure it is unique
            if (UniqueEdge(edge.From, edge.To)){
                Edges[edge.From].AddLast(edge);
            }

            // if the graph is undirected we must add another connection
            // in the opposite direction
            if (IsDigraph){
                return;
            }

            // check to make sure the edge is unique before adding
            if (!UniqueEdge(edge.To, edge.From)){
                return;
            }

            var newEdge = new Edge(edge){ To = edge.From, From = edge.To };
            Edges[edge.To].AddLast(newEdge);
        }

        /// <summary>
        /// Remove the edge(s) between the given node indices.
        /// </summary>
        /// <param name="from">The from index.</param>
        /// <param name="to">The to index.</param>
        public void RemoveEdge(int from, int to){
            if (from >= Nodes.Count || to >= Nodes.Count){
                Debug.LogError("SparseGraph.RemoveEdge: invalid node index.");
                throw new System.Exception("SparseGraph.RemoveEdge: invalid node index.");
            }

            if (!IsDigraph){
                foreach (Edge curEdge in Edges[to]){
                    if (curEdge.To != from){
                        continue;
                    }

                    Edges[to].Remove(curEdge);
                    break;
                }
            }

            foreach (Edge curEdge in Edges[from]){
                if (curEdge.To != to){
                    continue;
                }

                Edges[from].Remove(curEdge);
                break;
            }
        }

        /// <summary>
        /// Given a node this method first checks to see if the node has been added previously but
        /// is now inactive. If it is, it is reactivated.
        /// 
        /// If the node has not been added previously, it is checked to make sure its index matches
        /// the next node index before being added to the graph.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The node index.</returns>
        public int AddNode(Node node){
            if (node.Index < Nodes.Count){
                // make sure the client is not trying to add a node with the same
                // Id as a currently active node
                if (!Node.IsInvalidIndex(Nodes[node.Index].Index)){
                    Debug.LogError(
                        "SparseGraph.AddNode: Attempting to add a node with a duplicate Id.");
                    throw new System.Exception(
                        "SparseGraph.AddNode: Attempting to add a node with a duplicate Id.");
                }

                Nodes[node.Index] = node;

                return NextNodeIndex;
            }

            // make sure the new node has been indexed correctly
            if (node.Index != NextNodeIndex){
                Debug.LogError("SparseGraph.AddNode: invalid index.");
                throw new System.Exception("SparseGraph.AddNode: invalid index.");
            }

            Nodes.Add(node);
            Edges.Add(new LinkedList<Edge>());

            return NextNodeIndex++;
        }

        /// <summary>
        /// Iterates through all the edges in the graph and removes any that point to an invalidated
        /// node.
        /// </summary>
        public void CullInvalidEdges(){
            foreach (LinkedList<Edge> curEdgeList in Edges){
                foreach (Edge curEdge in curEdgeList){
                    if (Node.IsInvalidIndex(Nodes[curEdge.To].Index) ||
                        Node.IsInvalidIndex(Nodes[curEdge.From].Index)){
                        curEdgeList.Remove(curEdge);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a node from the graph and removes any links to neighbouring nodes.
        /// </summary>
        /// <param name="node">The node index.</param>
        public void RemoveNode(int node){
            if (node >= Nodes.Count){
                Debug.LogError("SparseGraph.RemoveNode: invalid node index.");
                throw new System.Exception("SparseGraph.RemoveNode: invalid node index.");
            }

            // set this node's index to invalid_node_index
            Nodes[node].Index = Node.INVALID_NODE_INDEX;

            // if the graph is not directed remove all edges leading to this node
            // and then clear the edges leading from the node
            if (!IsDigraph){
                // visit each neighbour and erase any edges leading to this node
                foreach (Edge curEdge in Edges[node]){
                    foreach (Edge curE in Edges[curEdge.To]){
                        if (curE.To != node){
                            continue;
                        }

                        Edges[curEdge.To].Remove(curE);
                        break;
                    }
                }

                // finally, clear this node's edges
                Edges[node].Clear();
            }

            // if a digraph, remove the edges the slow way
            else {
                CullInvalidEdges();
            }
        }

        /// <summary>
        /// Sets the cost of a specific edge.
        /// </summary>
        /// <param name="from">The from index.</param>
        /// <param name="to">The to index.</param>
        /// <param name="newCost">The new edge cost.</param>
        public void SetEdgeCost(int from, int to, float newCost){
            // make sure the nodes given are valid
            if (from >= Nodes.Count || to >= Nodes.Count){
                Debug.LogError("SparseGraph.SetEdgeCost: invalid index.");
                throw new System.Exception("SparseGraph.SetEdgeCost: invalid index.");
            }

            // visit each neighbour and erase any edges leading to this node
            foreach (Edge curEdge in Edges[from]){
                if (curEdge.To != to){
                    continue;
                }

                curEdge.Cost = newCost;
                break;
            }
        }

        /// <summary>
        /// Test if the edge is not present in the graph. Used when adding edges to prevent
        /// duplication.
        /// </summary>
        /// <param name="from">The from index.</param>
        /// <param name="to">The to index.</param>
        /// <returns>
        /// True if the edge is not present in the graph. 
        /// </returns>
        public bool UniqueEdge(int from, int to){
            foreach (Edge curEdge in Edges[from]){
                if (curEdge.To == to){
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the position of a graph node selected at random.
        /// </summary>
        /// <returns>The position of a graph node selected at random.</returns>
        public Vector2 GetRandomNodePosition(){
            int nodeIndex = Random.Range(0, NumNodes);
            int startNodeIndex = nodeIndex;

            while (!IsNodePresent(nodeIndex)){
                nodeIndex++;
                if (nodeIndex >= NumNodes){
                    nodeIndex = 0;
                }

                if (nodeIndex == startNodeIndex){
                    throw new System.Exception("SparseGraph: no active node found!");
                }
            }

            Node node = Nodes[nodeIndex];
            if(node !=null) return node.Position;
            return new Vector2(0, 0);
        }

        /// <summary>
        /// Generate a string representation of graph.
        /// </summary>
        /// <returns>A string representation of graph.</returns>
        public override string ToString(){
            var sb = new StringBuilder();
            sb.Append("Navigation Graph:");
            sb.AppendLine();

            sb.Append("Nodes:");
            foreach (Node curNode in Nodes){
                sb.AppendLine();
                sb.Append(curNode.ToString());
            }

            sb.AppendLine();

            sb.Append("Edges:");
            int nodeIndex = 0;
            foreach (LinkedList<Edge> edgeList in Edges){
                sb.AppendLine();
                sb.AppendFormat("NodeIndex: {0}", nodeIndex);
                foreach (Edge curEdge in edgeList){
                    sb.AppendLine();
                    sb.Append(curEdge.ToString());
                }

                nodeIndex++;
            }

            return sb.ToString();
        }
	}
}
