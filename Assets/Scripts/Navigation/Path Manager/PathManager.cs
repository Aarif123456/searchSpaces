
using Thot.GameAI;

using UnityEngine;

public struct PathRequestEventPayload
{
	public GameObject gameObject;
	public Vector2 destination;
	
	public PathRequestEventPayload(
        GameObject gameObject,
        Vector2 destination)
    {
        this.gameObject = gameObject;
        this.destination = destination;
    }
}

public struct PathReadyEventPayload
{
	public GameObject gameObject;
	public Path path;
	
	public PathReadyEventPayload(
        GameObject gameObject,
        Path path)
    {
        this.gameObject = gameObject;
        this.path = path;
    }
}

public sealed class PathManager : MonoBehaviour
{
	public bool searchSpaceCanChange;
	public bool requestPath;
	public SearchSpace searchSpace;
	public int currentSource;
	public bool ignoreObstructions;

	public bool showPath = true;
	
	public void Start()
	{
		SetSearchSpace();
	}
	
	public void Update()
	{
		if (searchSpaceCanChange)
		{
			SetSearchSpace();
		}
				
		if (searchSpace == null || !searchSpace.enabled)
		{
			return;
		}
		
		// This is for manual testing via the Inspector
		if (requestPath)
		{
			PathRequestEventPayload request = 
				new PathRequestEventPayload(searchSpace.gameObject, searchSpace.GetRandomEntityPosition());
			EventManager.Instance.Enqueue<PathRequestEventPayload>(Events.PathRequest, request);
			requestPath = false;
		}
	}
	
	private void OnEnable()
	{
		EventManager.Instance.Subscribe<PathRequestEventPayload>(Events.PathRequest, OnPathRequest);
	}
	
	private void OnDisable()
	{
		EventManager.Instance.Unsubscribe<PathRequestEventPayload>(Events.PathRequest, OnPathRequest);
	}
	
	private void OnPathRequest(Event<PathRequestEventPayload> eventArg)
	{
		if (searchSpace == null)
		{
			return;
		}
		
		PathRequestEventPayload request = eventArg.EventData;
		if (request.gameObject != searchSpace.gameObject)
		{
			return; // request not for us
		}
		
		MovingEntity movingEntity = request.gameObject.GetComponent<MovingEntity>();
		Vector2 requestorPosition2D = (movingEntity != null && movingEntity.enabled) ? movingEntity.Position2D : request.gameObject.transform.position.To2D();
		
		int source = searchSpace.GetClosestNodeToPosition(requestorPosition2D);
		
		if (source != Node.INVALID_NODE_INDEX)
		{
			// Requestor may be inside or too close to obstruction
            // so let's find the closest node to warp to
			source = searchSpace.GetClosestNodeToPosition(requestorPosition2D, true);
            if (source == SearchSpace.NO_CLOSEST_NODE_FOUND)
            {
                return; // screwed
            }
		}
		
		int target = searchSpace.GetClosestNodeToPosition(request.destination);
        if (target == SearchSpace.NO_CLOSEST_NODE_FOUND)
        {
            ////TODO: should we instead move the target to closest valid node??
            return;
        }
		
		var currentSearch = new AStarSearch(searchSpace.Graph, source, target);
		
		var path = new Path(
		        this,
		        searchSpace.gameObject,
                requestorPosition2D, 
                request.destination, 
                currentSearch.GetPathToTarget(),
                searchSpace.Graph);
		
		PathReadyEventPayload result =
			new PathReadyEventPayload(request.gameObject, path);
		EventManager.Instance.Enqueue<PathReadyEventPayload>(Events.PathReady, result); 
	}

	private void SetSearchSpace()
	{
		if (searchSpace == null || !searchSpace.enabled)
		{
			foreach (SearchSpace ss in GetComponents<SearchSpace>())
			{
				if (ss.enabled)
				{
					searchSpace = ss;
					break;
				}
			}
		}
	}
}