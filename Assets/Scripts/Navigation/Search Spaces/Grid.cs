#region Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

// Microsoft Reciprocal License (Ms-RL)
//
// This license governs use of the accompanying software. If you use the software, you accept this
// license. If you do not accept the license, do not use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same
// meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// copyright license to reproduce its contribution, prepare derivative works of its contribution,
// and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or
// otherwise dispose of its contribution in the software or derivative works of the contribution in
// the software.
//
// 3. Conditions and Limitations
// (A) Reciprocal Grants- For any file you distribute that contains code from the software (in
// source code or binary format), you must provide recipients the source code to that file along
// with a copy of this license, which license will govern that file. You may license other files
// that are entirely your own work and do not contain code from the software under any terms you
// choose.
// (B) No Trademark License- This license does not grant you rights to use any contributors' name,
// logo, or trademarks.
// (C) If you bring a patent claim against any contributor over patents that you claim are
// infringed by the software, your patent license from such contributor to the software ends
// automatically.
// (D) If you distribute any portion of the software, you must retain all copyright, patent,
// trademark, and attribution notices that are present in the software.
// (E) If you distribute any portion of the software in source code form, you may do so only under
// this license by including a complete copy of this license with your distribution. If you
// distribute any portion of the software in compiled or object code form, you may only do so under
// a license that complies with this license.
// (F) The software is licensed "as-is." You bear the risk of using it. The contributors give no
// express warranties, guarantees or conditions. You may have additional consumer rights under your
// local laws which this license cannot change. To the extent permitted under your local laws, the
// contributors exclude the implied warranties of merchantability, fitness for a particular purpose
// and non-infringement.

#endregion Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

//using System.Collections.Generic;

using Thot.GameAI;

using UnityEngine;

public class Grid : SearchSpace
{
	public bool drawCellGizmos;
	
	public float cellWidth;

    public float cellHeight;

    public float cellDiagonal;

    public int rows;

    public int columns;

    public bool useDiagonals = true;
	
//	public override void Awake()
//	{
//		base.Awake();	
//	}
	
	public override void Start(){
		base.Start();
		
		rows = (int)(World.Instance.Size.y / cellHeight);
        columns = (int)(World.Instance.Size.x / cellWidth);

        cellDiagonal = Mathf.Sqrt(cellHeight * cellHeight + cellWidth * cellWidth);
		
		Create();		
	}
	
	public override void Create(){
		Graph = new SparseGraph(false);

		for (int row = 0; row < rows; row++){
			for (int column = 0; column < columns; column++){
				var nodePosition = 
					new Vector2(
						column * cellWidth - cellWidth * (columns - 1) / 2,
			    		row * cellHeight - cellHeight * (rows - 1) / 2);
				
				var node = new Node(row * columns + column){ Position = nodePosition };
                int nodeIndex = Graph.AddNode(node);
			
				if (IsPointInObstacle(nodePosition)){
					Graph.GetNode(nodeIndex).Index = Node.INVALID_NODE_INDEX;
				}
				else {
					//AddNodeObject(node, nodePosition);
				}
			}
		}
		
		for (int nodeIndex = 0; nodeIndex < Graph.NumNodes; nodeIndex++){
			if (!Graph.IsNodePresent(nodeIndex)){
                continue;
            }

            Node node = Graph.GetNode(nodeIndex);

            int rightIndex = nodeIndex + 1;
            if (rightIndex % columns != 0 && Graph.IsNodePresent(rightIndex)){
                Node rightNode = Graph.GetNode(rightIndex);

                if (!IsPathObstructed(node.Position, rightNode.Position)){
                    var rightEdge = new Edge(nodeIndex, rightIndex, cellWidth);
                    Graph.AddEdge(rightEdge);
					//AddEdgeObject(rightEdge, node.Position, rightNode.Position);
                }
            }

            int downIndex = nodeIndex + columns;
            if (downIndex < Graph.NumNodes && Graph.IsNodePresent(downIndex)){
                Node downNode = Graph.GetNode(downIndex);

                if (!IsPathObstructed(node.Position, downNode.Position)){
                    var downEdge = new Edge(nodeIndex, downIndex, cellHeight);
                    Graph.AddEdge(downEdge);
					//AddEdgeObject(downEdge, node.Position, downNode.Position);
                }
            }

            if (!useDiagonals){
                continue;
            }

            int diagIndex = nodeIndex + columns + 1;
            if (diagIndex < Graph.NumNodes && diagIndex % columns != 0 &&
                Graph.IsNodePresent(diagIndex)){
                Node diagNode = Graph.GetNode(diagIndex);

                if (!IsPathObstructed(node.Position, diagNode.Position)){
                    var diagEdge = new Edge(nodeIndex, diagIndex, cellDiagonal);
                    Graph.AddEdge(diagEdge);
					//AddEdgeObject(diagEdge, node.Position, diagNode.Position);
                }
            }

            int backDiagIndex = nodeIndex + columns - 1;
            if (backDiagIndex < Graph.NumNodes && backDiagIndex % columns != columns - 1 &&
                Graph.IsNodePresent(backDiagIndex)){
                Node backDiagNode = Graph.GetNode(backDiagIndex);

                if (!IsPathObstructed(node.Position, backDiagNode.Position)){
                    var backDiagEdge = new Edge(nodeIndex, backDiagIndex, cellDiagonal);
                    Graph.AddEdge(backDiagEdge);
					//AddEdgeObject(backDiagEdge, node.Position, backDiagNode.Position);
                }
            }
        }
	}
	
	protected override void DrawNodeGizmo(Node node){	
		if (drawCellGizmos){
			Gizmos.color = Color.gray;
			Gizmos.DrawCube(World.Instance.GroundPositionAt(node.Position), new Vector3(cellWidth - 0.1f, 0.1f, cellHeight - 0.1f));
		}
		
		base.DrawNodeGizmo(node);
	}
}