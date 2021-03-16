

namespace Thot.GameAI
{
	using System.Collections.Generic;
	
	using UnityEngine;
	
	/// <summary>
    /// Class to represent a path. This path can be used by a path planner in the creation of paths.
    /// </summary>
	public sealed class Path 
	{
		public Path(
		    GameObject movingObject,
            Vector2 source,
            Vector2 destination,
            List<int> pathNodeIndices,
            SparseGraph graph){
            PathEdgeList = new List<PathEdge>();
            Source = source;
            Destination = destination;
			MovingObject = movingObject;
			
			int fromNodeIndex = -1;
			int toNodeIndex = -1;
			Node fromNode = null;
			Node toNode = null;
			Edge currentEdge = null;
			
            Vector2 from = source;
            foreach (int nodeIndex in pathNodeIndices){
				fromNodeIndex = toNodeIndex;
				toNodeIndex = nodeIndex;
				
				fromNode = toNode;	
				toNode = graph.GetNode(nodeIndex);
			
                Vector2 to = toNode.Position;
                if (from == to){
                    // this could happen when source is exactly on a node
                    // position. In this case, skip this redundant edge
                    continue;
                }
				
				if (fromNodeIndex != -1 && toNodeIndex != -1){
					currentEdge = graph.GetEdge(fromNodeIndex, toNodeIndex);
				}

                PathEdgeList.Add(new PathEdge(from, to, fromNode, toNode, currentEdge, MovingObject));
                from = to; // copy
            }

            if (from != destination){
                PathEdgeList.Add(new PathEdge(from, destination, toNode, null, null, MovingObject));
            }
        }

        /// <summary>
        /// Gets or sets the path source position.
        /// </summary>
        public Vector2 Source { get; set; }

        /// <summary>
        /// Gets or sets the path destination position.
        /// </summary>
        public Vector2 Destination { get; set; }

        /// <summary>
        /// Gets the list of edges in in the path.
        /// </summary>
        public List<PathEdge> PathEdgeList { get; private set; }
		
		public GameObject MovingObject { get; private set; }
		
		public bool IsEmpty
		{
			get {
				return PathEdgeList == null || PathEdgeList.Count == 0;
			}
		}
		
		public PathEdge Dequeue(){
			if (IsEmpty){
	            return null;
	        }
	
	        PathEdge firstEdge = PathEdgeList[0];
	        PathEdgeList.RemoveAt(0);
			return firstEdge;
		}
		
		public override string ToString(){
			// TODO: use StringBuilder
			string pathString = string.Empty;
			for (int i = 0; i < PathEdgeList.Count; i++){
				pathString += "[ " + PathEdgeList[i].ToString() + " ] ";
			}
			
			return pathString;
		}
		
		public void ShowPath(bool show){
			if (PathEdgeList != null){
				foreach (PathEdge pathEdge in PathEdgeList){
					pathEdge.ShowEdge(show);
				}
			}
		}
	}
}
