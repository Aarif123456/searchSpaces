

using UnityEngine.Rendering;

namespace Thot.GameAI
{
	using UnityEngine;
	
	/// <summary>
    /// Class to represent a path edge. This path can be used by a path planner in the creation of
    /// paths. 
    /// </summary>
	public sealed class PathEdge 
	{	
		private GameObject waypointBeacon;
		private GameObject edgeBeacon;
		private GameObject waypointMarker;
		
		public PathEdge(
			Vector2 source, 
		    Vector2 destination, 
		    Node fromNode, 
		    Node toNode,
		    Edge edge,
		    GameObject movingObject)
		{
			Source = source;
            Destination = destination;
			FromNode = fromNode;
			ToNode = toNode;
			Edge = edge;
			MovingObject = movingObject;
			MovingEntity = movingObject.GetComponent<MovingEntity>();
		}

        /// <summary>
        /// Gets or sets the edge source position.
        /// </summary>
        public Vector2 Source { get; set; }

        /// <summary>
        /// Gets or sets the edge destination position.
        /// </summary>
        public Vector2 Destination { get; set; }
		
		public Node FromNode { get; set; }
		public Node ToNode { get; set; }
		public Edge Edge { get; set; }
		public GameObject MovingObject { get; set; }
		public MovingEntity MovingEntity { get; set; }
		public float Radius
		{
			get
			{
				if (MovingEntity != null && MovingEntity.enabled)
				{
					return MovingEntity.Radius;
				}
				else
				{
					return 1;
				}
			}
		}
				
		public override string ToString()
		{
			return Source + "-->" + Destination;
		}
		
		public void ShowEdge(bool show)
		{
			
			if (ToNode != null)
			{		
				if (show)
				{
					if (UniversalPathManager.Instance.showPath)
					{
						waypointBeacon = CreatePointBeacon(ToNode.Position);
						waypointMarker = CreatePointMarker(ToNode.Position);
					}
				}
				else
				{
					Object.Destroy(waypointBeacon);
					waypointBeacon = null;
					Object.Destroy(waypointMarker);
					waypointMarker = null;
				}
			}
			
			if (Edge != null)
			{
				if (show)
				{
					if (UniversalPathManager.Instance.showPath)
					{
//						edgeMarker = CreateEdgeMarker(FromNode.Position, ToNode.Position);
						edgeBeacon = CreateEdgeBeacon(FromNode.Position, ToNode.Position);
					}
				}
				else
				{
					Object.Destroy(edgeBeacon);
					edgeBeacon = null;
				}
			}
			
			if (Edge == null)
			{
				if (FromNode == null && ToNode != null)
				{
					if (show)
					{
						if (UniversalPathManager.Instance.showPath)
						{
//							edgeMarker = CreateEdgeMarker(Source, ToNode.Position);
							edgeBeacon = CreateEdgeBeacon(Source, ToNode.Position);
						}
					}
					else
					{
//						Object.Destroy(edgeMarker);
//						edgeMarker = null;
						Object.Destroy(edgeBeacon);
						edgeBeacon = null;
					}	
				}
				
				if (FromNode != null && ToNode == null)
				{
					if (show)
					{
						if (UniversalPathManager.Instance.showPath)
						{
//							edgeMarker = CreateEdgeMarker(FromNode.Position, Destination);
							edgeBeacon = CreateEdgeBeacon(FromNode.Position, Destination);
							waypointMarker = CreatePointMarker(Destination);
							waypointBeacon = CreatePointBeacon(Destination);
						}
					}
					else
					{
//						Object.Destroy(edgeMarker);
//						edgeMarker = null;
						Object.Destroy(edgeBeacon);
						edgeBeacon = null;
						Object.Destroy(waypointMarker);
						waypointMarker = null;
						Object.Destroy(waypointBeacon);
						waypointBeacon = null;
					}	
				}
			}
		}
		
		private GameObject CreatePointBeacon(Vector2 point)
		{
			GameObject nodeMarkers = GameObject.Find("Game/NodeMarkers");
			GameObject pointMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);	
			pointMarker.transform.position = World.Instance.GroundPositionAt(point);
			pointMarker.transform.localScale = Vector3.one * Radius + Vector3.up * 5;
			pointMarker.transform.parent = nodeMarkers.transform;
			pointMarker.name = "PointBeacon" + point;
			pointMarker.GetComponent<Renderer>().enabled = true;
			pointMarker.GetComponent<Renderer>().material = new Material(MovingObject.GetComponent<Renderer>().material);
			pointMarker.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
			Color color = pointMarker.GetComponent<Renderer>().material.color;
			color.a = 0.4f;
			pointMarker.GetComponent<Renderer>().material.color = color;
			pointMarker.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
			Object.Destroy(pointMarker.GetComponent<Collider>());
			return pointMarker;
		}
		
		private GameObject CreateEdgeBeacon(Vector2 startPoint, Vector2 endPoint)
		{
			GameObject edgeMarkers = GameObject.Find("Game/EdgeMarkers");
			GameObject edgeBeacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
			edgeBeacon.name = "EdgeBeacon " + startPoint + " ---> " + endPoint;
			Vector3 startPosition = World.Instance.GroundPositionAt(startPoint);
			Vector3 endPosition = World.Instance.GroundPositionAt(endPoint);
			edgeBeacon.transform.position = (startPosition + endPosition) / 2;		
			edgeBeacon.transform.localScale = 
				new Vector3(0.3f, 0.3f, (endPosition - startPosition).magnitude);
			
	    	Vector2 direction = endPoint - startPoint;
	    	float angle = Vector2.Angle(Vector2.up, direction);
	    	Vector3 cross = Vector3.Cross(Vector2.up, direction);
	    	if (cross.z > 0)
			{
				angle = 360f - angle;
			}
	
			edgeBeacon.transform.eulerAngles = new Vector3(0, angle, 0);
			
			edgeBeacon.transform.parent = edgeMarkers.transform;
			edgeBeacon.GetComponent<Renderer>().material = new Material(MovingObject.GetComponent<Renderer>().material);
			edgeBeacon.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
			Color color = edgeBeacon.GetComponent<Renderer>().material.color;
			color.a = 0.4f;
			edgeBeacon.GetComponent<Renderer>().material.color = color;	
			edgeBeacon.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
			edgeBeacon.GetComponent<Renderer>().enabled = true;
			Object.Destroy(edgeBeacon.GetComponent<Collider>());
			return edgeBeacon;
		}
		
		private GameObject CreatePointMarker(Vector2 point)
		{
			GameObject nodeMarkers = GameObject.Find("Game/NodeMarkers");
			GameObject pointMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);	
			pointMarker.transform.position = World.Instance.GroundPositionAt(point);
			pointMarker.transform.localScale = Vector3.one * Radius * 1.5f;
			pointMarker.transform.parent = nodeMarkers.transform;
			pointMarker.name = "PointMarker" + point;
			pointMarker.GetComponent<Renderer>().enabled = true;
			pointMarker.GetComponent<Renderer>().material = MovingObject.GetComponent<Renderer>().material;
			pointMarker.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
			Object.Destroy(pointMarker.GetComponent<Collider>());
			return pointMarker;
		}
		
//		private GameObject CreateEdgeMarker(Vector2 startPoint, Vector2 endPoint)
//		{
//			GameObject edgeMarkers = GameObject.Find("Game/EdgeMarkers");
//			GameObject edgeMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
//			edgeMarker.name = "EdgeMarker " + startPoint + " ---> " + endPoint;
//			Vector3 startPosition = World.Instance.GroundPositionAt(startPoint);
//			Vector3 endPosition = World.Instance.GroundPositionAt(endPoint);
//			edgeMarker.transform.position = (startPosition + endPosition) / 2;		
//			edgeMarker.transform.localScale = 
//				new Vector3(0.11f, 0.11f, (endPosition - startPosition).magnitude);
//			
//	    	Vector2 direction = endPoint - startPoint;
//	    	float angle = Vector2.Angle(Vector2.up, direction);
//	    	Vector3 cross = Vector3.Cross(Vector2.up, direction);
//	    	if (cross.z > 0)
//			{
//				angle = 360f - angle;
//			}
//	
//			edgeMarker.transform.eulerAngles = new Vector3(0, angle, 0);
//			
//			edgeMarker.transform.parent = edgeMarkers.transform;
//			edgeMarker.renderer.material = MovingObject.renderer.material;
//			edgeMarker.renderer.castShadows = false;
//			edgeMarker.renderer.enabled = true;
//			Object.Destroy(edgeMarker.collider);
//			return edgeMarker;
//		}
	}
}
