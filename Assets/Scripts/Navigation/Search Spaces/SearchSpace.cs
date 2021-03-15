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

using System.Collections.Generic;

using Thot.GameAI;

using UnityEngine;
using UnityEngine.Rendering;

// Require a MovingEntity to be attached to the parent game object.
[RequireComponent(typeof(MovingEntity))]

public abstract class SearchSpace : MonoBehaviour
{
	public const int NO_CLOSEST_NODE_FOUND = -1;
		
	public bool drawNodeGizmos;
	public bool drawEdgeGizmos;
	public bool createNodeMarkers;
	public bool createEdgeMarkers;
	public bool showNodeMarkers;
	public bool showEdgeMarkers;
	
	private bool createdNodeMarkers;
	private bool createdEdgeMarkers;
	private bool showingNodeMarkers;
	private bool showingEdgeMarkers;
	
	protected GameObject nodeMarkers;
	protected GameObject edgeMarkers;
	
	public SparseGraph Graph { get; protected set; }
	public MovingEntity MovingEntity { get; protected set; }
	
	public bool CreatedNodeMarkers
	{
		get
		{
			return createdNodeMarkers;
		}
		
		set
		{
			if (value && !createdNodeMarkers)
			{
				if (Graph != null)
				{
					foreach (Node node in Graph.Nodes)
					{
						if (node.gameObject == null)
						{
							AddNodeObject(node, node.Position);
						}
					}
				}
			}
			else if (!value && createdNodeMarkers)
			{
				if (Graph != null)
				{
					foreach (Node node in Graph.Nodes)
					{
						if (node.gameObject != null)
						{
							Destroy(node.gameObject);
						}
					}
				}
			}
			
			createdNodeMarkers = value;
		}
	}
	
	public bool CreatedEdgeMarkers
	{
		get
		{
			return createdEdgeMarkers;
		}
		
		set
		{
			if (value && !createdEdgeMarkers)
			{
				if (Graph != null)
				{
					foreach (LinkedList<Edge> edgeList in Graph.Edges)
					{
						foreach (Edge edge in edgeList)
						{
							if (edge.gameObject == null)
							{
								Vector2 fromPosition = Graph.GetNode(edge.From).Position;
								Vector2 toPosition = Graph.GetNode(edge.To).Position;
								AddEdgeObject(edge, fromPosition, toPosition);
							}
						}
					}
				}
			}
			else if (!value && createdEdgeMarkers)
			{
				if (Graph != null)
				{
					foreach (LinkedList<Edge> edgeList in Graph.Edges)
					{
						foreach (Edge edge in edgeList)
						{
							if (edge.gameObject != null)
							{
								Destroy(edge.gameObject);
							}
						}
					}
				}
			}
			
			createdEdgeMarkers = value;
		}
	}
	
	public bool ShowingNodeMarkers
	{
		get
		{
			return showingNodeMarkers;
		}
		
		set
		{
			if (value != showingNodeMarkers)
			{
				if (Graph != null)
				{
					foreach (Node node in Graph.Nodes)
					{
						if (node.gameObject != null)
						{
							node.gameObject.GetComponent<Renderer>().enabled = value;
						}
					}
				}
				
				showingNodeMarkers = value;
			}
		}
	}
	
	public bool ShowingEdgeMarkers
	{
		get
		{
			return showingEdgeMarkers;
		}
		
		set
		{
			if (value != showingEdgeMarkers)
			{
				if (Graph != null)
				{
					foreach (LinkedList<Edge> edgeList in Graph.Edges)
					{
						foreach (Edge edge in edgeList)
						{
							if (edge.gameObject != null)
							{
								edge.gameObject.GetComponent<Renderer>().enabled = value;
							}
						}
					}
				}
				
				showingEdgeMarkers = value;
			}
		}
	}
	
	public virtual void Awake()
	{
		MovingEntity = gameObject.GetComponent<MovingEntity>();
		nodeMarkers = GameObject.Find("Game/NodeMarkers");
		edgeMarkers = GameObject.Find("Game/EdgeMarkers");
	}
	
	public virtual void Start()
	{
	}
	
	public virtual void Update()
	{
		ShowingNodeMarkers = showNodeMarkers;
		ShowingEdgeMarkers = showEdgeMarkers;
		CreatedNodeMarkers = createNodeMarkers;
		CreatedEdgeMarkers = createEdgeMarkers;
	}
	
	public abstract void Create();
	
	/// <summary>
    /// Generate a random position.
    /// </summary>
    /// <remarks>
    /// TODO: This is a dumb approach. Replace.
    /// </remarks>
    /// <returns>A random position.</returns>
    public Vector2 GetRandomPosition()
    {
        int attempts = 0;
        var position = new Vector2();
        while (attempts < 1000) // shouldn't hardcode max attempts
        {
            position.x = Random.Range(
                    World.Instance.Center.x - World.Instance.Size.x / 2,
                    World.Instance.Center.x + World.Instance.Size.x / 2);
            position.y =
                Random.Range(
                World.Instance.Center.y - World.Instance.Size.y / 2,
                World.Instance.Center.y + World.Instance.Size.y / 2);

            if (!IsPointInObstacle(position) && 
			    GetClosestNodeToPosition(position) != NO_CLOSEST_NODE_FOUND)
            {
                return position;
            }

            attempts++;
        }

        // give up. just return a random node position
        return Graph.GetRandomNodePosition();
    }
	
	public Vector2 GetRandomEntityPosition()
    {
        int attempts = 0;
        var position = new Vector2();
        while (attempts < 1000) // shouldn't hardcode max attempts
        {
            position.x = Random.Range(
                    World.Instance.Center.x - World.Instance.Size.x / 2,
                    World.Instance.Center.x + World.Instance.Size.x / 2);
            position.y =
                Random.Range(
                World.Instance.Center.y - World.Instance.Size.y / 2,
                World.Instance.Center.y + World.Instance.Size.y / 2);

            if (!MovingEntity.IsEntityInObstacle(position) && 
			    GetClosestNodeToPosition(position) != NO_CLOSEST_NODE_FOUND)
            {
                return position;
            }

            attempts++;
        }

        // give up. just return a random node position
        return Graph.GetRandomNodePosition();
    }
	
	/// <summary>
    /// Gets the index of the closest visible graph node to the given position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>
    /// The index of the closest visible graph node to the given position.
    /// </returns>
    public int GetClosestNodeToPosition(Vector2 position)
    {
        return GetClosestNodeToPosition(position, false);
    }

    public int GetClosestNodeToPosition(Vector2 position, bool ignoreObstructions)
    {
        float closestSoFar = float.MaxValue;
        int closestNode = NO_CLOSEST_NODE_FOUND;
		
		if (Graph != null)
		{
			for (int nodeIndex = 0; nodeIndex < Graph.NumNodes; nodeIndex++)
	        {
	            if (!Graph.IsNodePresent(nodeIndex))
	            {
	                continue;
	            }
	
	            Node currentNode = Graph.GetNode(nodeIndex);
				
				// if the path between this node and position is unobstructed
	            // calculate the distance
	            if (!ignoreObstructions && IsPathObstructed(position, currentNode.Position))
	            {
	                continue;
	            }
	
	            float distance = Vector2.Distance(position, currentNode.Position);
	
	            // keep a record of the closest so far
	            if (distance >= closestSoFar)
	            {
	                continue;
	            }
	
	            closestSoFar = distance;
	            closestNode = currentNode.Index;
			}
		}
		
		return closestNode;
	}
	
	protected bool IsPointInObstacle(Vector2 point)
	{
		Vector3 groundPosition = World.Instance.GroundPositionAt(point);
		groundPosition.y -= 1;
		
		foreach (RaycastHit hit in Physics.RaycastAll(groundPosition, Vector3.up, float.MaxValue))
		{
			Entity entity = hit.collider.gameObject.GetComponent<Entity>();

			if (entity != null && entity.isObstacle)
			{
				return true;
			}
		}
		
		return false;
	}
	
	protected bool IsPathObstructed(Vector2 startPoint, Vector2 endPoint)
	{
		if (Mathf.Approximately(startPoint.x, endPoint.x) && 
		    Mathf.Approximately(startPoint.y, endPoint.y))
		{
			return false;
		}
		
		Vector3 startGroundPosition = World.Instance.GroundPositionAt(startPoint);
		Vector3 endGroundPosition = World.Instance.GroundPositionAt(endPoint);
		Vector3 direction = endGroundPosition - startGroundPosition;
		float distance = direction.magnitude;
		direction.Normalize();
		Vector3 movingEntityBottomPosition = 
			startGroundPosition + MovingEntity.Center + Vector3.up * MovingEntity.Height / 2;
		Vector3 movingEntityTopPosition = movingEntityBottomPosition + Vector3.up * MovingEntity.Height;
		
		foreach (RaycastHit hit in 
		         Physics.CapsuleCastAll(
		         	movingEntityBottomPosition, 
		            movingEntityTopPosition, 
		            MovingEntity.Radius,
		            direction,
					distance))
		{
			Entity entity = hit.collider.gameObject.GetComponent<Entity>();
			if (entity != null && entity.isObstacle)
			{
				return true;
			}
		}
		
		RaycastHit hitInfo;
		if (Physics.Raycast(startGroundPosition, direction, out hitInfo, distance))
		{
			Entity entity = hitInfo.collider.gameObject.GetComponent<Entity>();

			if (entity != null && entity.isObstacle)
			{
				// TODO: Bug? This should not happen. Why wasn't it caught in the capsule cast?
				Debug.Log("HUH?: " + entity.name);
				Debug.Log("HUH?: " + movingEntityBottomPosition);
				Debug.Log("HUH?: " + movingEntityTopPosition);
				Debug.Log("HUH?: " + MovingEntity.Radius);
				Debug.Log("HUH?: " + direction);
				Debug.Log("HUH?: " + distance);
				Debug.Log("HUH?: " + entity.transform.position);
				Debug.Log("HUH?: " + MovingEntity.transform.position);
				
				return true;
			}
		}
		
		return false;
	}
	
	private void OnDrawGizmosSelected()
	{
		if (Graph == null)
		{
			return;
		}
		
		if (drawNodeGizmos)
		{
			DrawNodeGizmos();
		}
				
		if (drawEdgeGizmos)
		{
			DrawEdgeGizmos();
		}
	}
	
	protected virtual void DrawNodeGizmos()
	{
		foreach (Node node in Graph.Nodes)
		{
			if (node.Index != Node.INVALID_NODE_INDEX)
			{				
				DrawNodeGizmo(node);
			}
		}
	}
	
	protected virtual void DrawNodeGizmo(Node node)
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(World.Instance.GroundPositionAt(node.Position), Vector3.one * MovingEntity.Radius);
	}
	
	protected virtual void DrawEdgeGizmos()
	{
		foreach(LinkedList<Edge> edgeList in Graph.Edges)
		{
			foreach (Edge edge in edgeList)
			{
				int fromIndex = edge.From;
				int toIndex = edge.To;
				
				if (fromIndex == Node.INVALID_NODE_INDEX || toIndex == Node.INVALID_NODE_INDEX)
				{
					Debug.Log("Invalid node");
					continue;
				}
				
				Node fromNode = Graph.GetNode(fromIndex);
				Node toNode = Graph.GetNode(toIndex);
				DrawEdgeGizmo(fromNode, toNode);
			}
		}
	}
	
	protected virtual void DrawEdgeGizmo(Node fromNode, Node toNode)
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(World.Instance.GroundPositionAt(fromNode.Position), World.Instance.GroundPositionAt(toNode.Position));
	}
	
	protected void AddNodeObject(Node node, Vector2 nodePosition)
	{
		if (!createNodeMarkers)
		{
			return;
		}
		
		node.gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);	
		node.gameObject.transform.position = World.Instance.GroundPositionAt(nodePosition);
		node.gameObject.transform.localScale = Vector3.one * MovingEntity.Radius;
		node.gameObject.transform.parent = nodeMarkers.transform;
		node.gameObject.name = "NodeMarker<" + node.Index + ">_" + node.Position;
		node.gameObject.GetComponent<Renderer>().enabled = ShowingNodeMarkers;
		node.gameObject.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
		node.gameObject.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
		Destroy(node.gameObject.GetComponent<Collider>());
	}
	
	protected void AddEdgeObject(Edge edge, Vector2 startPoint, Vector2 endPoint)
	{
		if (!createEdgeMarkers)
		{
			return;
		}
		edge.gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		edge.gameObject.name = "EdgeMarker_" + edge.From + "_[" + edge.Cost.ToString("F1") + "]_" + edge.To;
		Vector3 startPosition = World.Instance.GroundPositionAt(startPoint);
		Vector3 endPosition = World.Instance.GroundPositionAt(endPoint);
		edge.gameObject.transform.position = (startPosition + endPosition) / 2;		
		edge.gameObject.transform.localScale = 
			new Vector3(0.1f, 0.1f, (endPosition - startPosition).magnitude);
		
    	Vector2 direction = endPoint - startPoint;
    	float angle = Vector2.Angle(Vector2.up, direction);
    	Vector3 cross = Vector3.Cross(Vector2.up, direction);
    	if (cross.z > 0)
		{
			angle = 360f - angle;
		}

		edge.gameObject.transform.eulerAngles = new Vector3(0, angle, 0);
		
		edge.gameObject.transform.parent = edgeMarkers.transform;
		edge.gameObject.GetComponent<Renderer>().material = gameObject.GetComponent<Renderer>().material;
		edge.gameObject.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
		edge.gameObject.GetComponent<Renderer>().enabled = ShowingEdgeMarkers;
		Destroy(edge.gameObject.GetComponent<Collider>());
	}
}