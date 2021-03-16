

namespace Thot.GameAI
{
    using System.Collections.Generic;
    using UnityEngine;
    public delegate float HeuristicDelegate(SparseGraph graph, int node1, int node2);
    public enum CalculationType{
        EuclideanDistance,
        ManhattanDistance
    }
    [AddComponentMenu("Scripts/Navigation/Graph/Time Sliced A* Search")]
    /* This searches a graph using the distance between the target node and the 
    *  currently considered node as a heuristic. The time-sliced version hold temporary answers
    *  - so until it keep complete it will just keep updating the answer 
    */
    public class TimeSlicedAStarSearch: MonoBehaviour
    {
        /*Gets the list of edges that comprise the shortest path tree - a directed subtree of the
        graph that encapsulates the best paths from every node on the SPT to the source node.*/
        public List<Edge> ShortestPathTree { get; private set; }
        /*Gets the graph to be searched.*/
        public SparseGraph Graph { get; private set; }
        /* Gets an indexed (by node) list of 'parent' edges leading to nodes connected to the SPT
        but that have not been added to the SPT yet. This is a little like the stack or queue
        used in BST and DST searches. This is also referred to as the search fringe.*/
        public List<Edge> SearchFrontier { get; private set; }
        /*Gets the list of g-costs indexed by node. Contains the 'actual' accumulative cost to that node.*/
        public List<float> GCosts { get; private set; }
        /*Gets the list of f-costs indexed by node. Contains the cost of adding GCosts[n] to the
        heuristic cost from n to the target node. The priority queue indexes into this list.*/
        public List<float> FCosts { get; private set; }
        /* Gets the source node index.*/
        public int Source { get; private set; }
        /*Gets the source node index.*/
        public int Target { get; private set; }
        /*Gets the number of nodes searched. Used for performance measuring.*/
        public int NodesSearched { get; private set; }
        /* Priority queue to hold nodes */
        public IndexedPriorityQueueLow pq;
        public List<int> finalPath;
        public CalculationType CalculationType { get; set; }

        /* Constructor create graph and initialize search*/
        public TimeSlicedAStarSearch(SparseGraph graph, int source, int target){
            Reset(graph, source, target);
        }
        public void Start(){
            Graph = new SparseGraph(false);
            Source = 0;
            Target = 0;
            ShortestPathTree = new List<Edge>();
            SearchFrontier = new List<Edge>();
            GCosts = new List<float>();
            FCosts = new List<float>();
            pq = new IndexedPriorityQueueLow(FCosts, Graph.NumNodes);
            Done();
        }

        public void Reset(SparseGraph graph, int source, int target){
            Graph = graph;
            Source = source;
            Target = target;
            ShortestPathTree = new List<Edge>(Graph.NumNodes);
            SearchFrontier = new List<Edge>(Graph.NumNodes);
            GCosts = new List<float>(Graph.NumNodes);
            FCosts = new List<float>(Graph.NumNodes);

            for (int i = 0; i < graph.NumNodes; i++){
                ShortestPathTree.Add(null);
                SearchFrontier.Add(null);
                GCosts.Add(0);
                FCosts.Add(0);
            }

            // used for performance measuring
            NodesSearched = 0; 
            /* Create an indexed priority queue of nodes. The nodes with the
            *  lowest overall F cost (G+H) are positioned at the front.
            */
            pq = new IndexedPriorityQueueLow(FCosts, Graph.NumNodes);

            // put the source node on the queue
            pq.Insert(Source);
            /* Used to track our done status as well */
            finalPath = null;
        }

        /*Calculate the straight line distance from the first node to the second node */
        public static float EuclideanDistance(SparseGraph graph, int node1, int node2){
            return (graph.GetNode(node1).Position - graph.GetNode(node2).Position).magnitude;
        }

        /* Gets the total cost to the target.*/
        public float GetCostToTarget(){
            return GCosts[Target];
        }

        /* Override Get path function- if we know our new node*/
        public List<int> GetPathToTarget(int source){
            if((finalPath !=null && finalPath.Count >0 && finalPath[0] != source)){
                int index = finalPath.Find(nd => nd==source);
                finalPath.RemoveRange(0, source-Source);
            }
            Source = source;
            return GetPathToTarget();
        }

        /* Gets a list of node indexes that comprise the shortest path from the source to the target */
        public List<int> GetPathToTarget(){ 
            /* If we have the path we don't need to recalculate */
            if(finalPath !=null) return finalPath;
            var path = new List<int>();
            // just return an empty path if no target or no path found
            if (Target < 0 || ShortestPathTree ==null || (ShortestPathTree.Count == 0)) return path;
            int nd = Target;
            path.Insert(0, nd);
            while ((nd != Source) && (ShortestPathTree[nd] != null)){
                nd = ShortestPathTree[nd].From;
                path.Insert(0, nd);
            }
            return path;
        }

        private void Done(){
            finalPath = GetPathToTarget();
        }

        public bool IsDone(){
            return finalPath != null;
        }

        private HeuristicDelegate GetCalculateMethod(){
            switch(CalculationType){
                case CalculationType.ManhattanDistance:
                    Debug.LogWarning("ManhattanDistance not implemented");
                    return EuclideanDistance;
                    // return ManhattanDistance;
                case CalculationType.EuclideanDistance:
                    return EuclideanDistance;
            }
            Debug.LogWarning("Unexpected fall through when getting calculate method in A* Search - defaulting to EuclideanDistance");
            return EuclideanDistance;
        }
        public void Update(){
            if(pq.Empty()) Done();
            if(IsDone()) return;
            // get lowest cost node from the queue
            int nextClosestNode = pq.Pop();
            NodesSearched++;
            // move this node from the frontier to the spanning tree
            ShortestPathTree[nextClosestNode] = SearchFrontier[nextClosestNode];
            // if the target has been found exit
            if (nextClosestNode == Target){
                Done();
                return;
            }
            HeuristicDelegate calculate = GetCalculateMethod();
            // now to test all the edges attached to this node
            foreach (Edge curEdge in Graph.Edges[nextClosestNode]){
                // calculate (H) the heuristic cost from this node to the target                     
                float hCost = calculate(Graph, Target, curEdge.To);
                // calculate (G) the 'real' cost to this node from the source 
                float gCost = GCosts[nextClosestNode] + curEdge.Cost;
                // if the node has not been added to the frontier, add it and update the G and F costs
                if (SearchFrontier[curEdge.To] == null){
                    FCosts[curEdge.To] = gCost + hCost;
                    GCosts[curEdge.To] = gCost;
                    pq.Insert(curEdge.To);
                    SearchFrontier[curEdge.To] = curEdge;
                }

                // if this node is already on the frontier but the cost to
                // get here is cheaper than has been found previously, update
                // the node costs and frontier accordingly.
                else if ((gCost < GCosts[curEdge.To]) && (ShortestPathTree[curEdge.To] == null)){
                    FCosts[curEdge.To] = gCost + hCost;
                    GCosts[curEdge.To] = gCost;
                    pq.ChangePriority(curEdge.To);
                    SearchFrontier[curEdge.To] = curEdge;
                }
            }
        }
    }
}
