

using Thot.GameAI;

using UnityEngine;

public struct FollowCompletedEventPayload
{
    public GameObject gameObject;
    public Path path;

    public FollowCompletedEventPayload(
        GameObject gameObject,
        Path path)
    {
        this.gameObject = gameObject;
        this.path = path;
    }
}

public struct FollowFailedEventPayload
{
    public GameObject gameObject;
    public Path path;

    public FollowFailedEventPayload(
        GameObject gameObject,
        Path path)
    {
        this.gameObject = gameObject;
        this.path = path;
    }
}

public sealed class PathFollower : MonoBehaviour 
{
	private EdgeTraverser edgeTraverser;
	
	/// <summary>
    /// Gets a local copy of the path returned by the path planner.
    /// </summary>
    public Path PathToFollow { get; private set; }
	
	public bool IsFollowing { get; private set; }
	
	public bool BrakeOnFinalApproach { get; private set; }

	public bool StopOnFinalArrival { get; private set; }

	public bool BrakeOnEachApproach { get; private set; }

	public bool StopOnEachArrival { get; private set; }
	
	private void OnEnable()
	{
		EventManager.Instance.Subscribe<TraversalCompletedEventPayload>(
			Events.TraversalCompleted, 
		    OnTraversalCompleted);
		
		EventManager.Instance.Subscribe<TraversalFailedEventPayload>(
			Events.TraversalFailed, 
		    OnTraversalFailed);
		
		EventManager.Instance.Subscribe<PathReadyEventPayload>(
			Events.PathReady, 
		    OnPathReady);
	}
	
	private void OnDisable()
	{
		EventManager.Instance.Unsubscribe<TraversalCompletedEventPayload>(
			Events.TraversalCompleted, 
		    OnTraversalCompleted);
		
		EventManager.Instance.Unsubscribe<TraversalFailedEventPayload>(
			Events.TraversalFailed, 
		    OnTraversalFailed);
		
		EventManager.Instance.Unsubscribe<PathReadyEventPayload>(
			Events.PathReady, 
		    OnPathReady);
	}
	
	public bool Follow(Path pathToFollow)
    {
        return Follow(pathToFollow, true, true, false, false);
    }
	
	public bool Follow(
		Path pathToFollow,
        bool brakeOnFinalApproach,
        bool stopOnFinalArrival,
        bool brakeOnEachApproach,
        bool stopOnEachArrival)
    {
		if (edgeTraverser == null)
		{
        	edgeTraverser = GetComponent<EdgeTraverser>();
		}
		
        if (edgeTraverser == null)
        {
            return false;
        }
		
		StopIfFollowingPath();
		
        PathToFollow = pathToFollow;
        BrakeOnFinalApproach = brakeOnFinalApproach;
        StopOnFinalArrival = stopOnFinalArrival;
        BrakeOnEachApproach = brakeOnEachApproach;
        StopOnEachArrival = stopOnEachArrival;
        IsFollowing = true;
		
		if (PathToFollow != null)
		{
			PathToFollow.ShowPath(true);
		}

        TraverseNextEdge();

        return true;
    }
	
	private void StopIfFollowingPath()
	{
		if (PathToFollow != null)
		{
			PathToFollow.ShowPath(false);
			
			if (IsFollowing)
			{
				IsFollowing = false;
				
				// TODO: Perhaps this should be a FollowCancelled event
				EventManager.Instance.Enqueue<FollowCompletedEventPayload>(
				    Events.FollowCompleted,
	                new FollowCompletedEventPayload(gameObject, PathToFollow)); 
			}	
		}
	}
	
	private void TraverseNextEdge()
    {
        if (PathToFollow == null)
        {
            return;
        }
		
        ////TODO: probably should add NextEdge method to Path class
        PathEdge edgeToFollow = PathToFollow.Dequeue();
		
		if (edgeToFollow == null)
		{
			return;
		}

        if (PathToFollow.IsEmpty) // last edge
        {
            edgeTraverser.Traverse(
                 edgeToFollow,
                 BrakeOnFinalApproach,
                 StopOnFinalArrival);
        }
        else
        {
            edgeTraverser.Traverse(
                 edgeToFollow,
                 BrakeOnEachApproach,
                 StopOnEachArrival);
        }
    }

    private void OnTraversalCompleted(Event<TraversalCompletedEventPayload> eventArg)
    {
		TraversalCompletedEventPayload payload = eventArg.EventData;
		
        if (payload.gameObject != gameObject) // event not for us
        {
            return;
        }
		
		if (payload.edge != null)
		{
			payload.edge.ShowEdge(false);
		}

        if (PathToFollow != null && PathToFollow.IsEmpty)
        {
            IsFollowing = false;
            EventManager.Instance.Enqueue<FollowCompletedEventPayload>(
			    Events.FollowCompleted,
                new FollowCompletedEventPayload(payload.gameObject, PathToFollow));
            return;
		}

        TraverseNextEdge();
    }

    private void OnTraversalFailed(Event<TraversalFailedEventPayload> eventArg)
    {
		TraversalFailedEventPayload payload = eventArg.EventData;
		
        if (payload.gameObject != gameObject) // event not for us
        {
            return;
        }
		
		if (payload.edge != null)
		{
			payload.edge.ShowEdge(false);
		}

        IsFollowing = false;
        EventManager.Instance.Enqueue<FollowFailedEventPayload>(
		    Events.FollowFailed,
            new FollowFailedEventPayload(payload.gameObject, PathToFollow));
    }

    private void OnPathReady(Event<PathReadyEventPayload> eventArg)
    {	
		PathReadyEventPayload payload = eventArg.EventData;
			
        if (payload.gameObject != gameObject) // event not for us
        {
            return;
        }

        Follow(payload.path);
    }
}