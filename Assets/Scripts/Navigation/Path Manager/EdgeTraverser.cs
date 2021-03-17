

using Thot.GameAI;

using UnityEngine;

public struct TraversalCompletedEventPayload
{
    public GameObject gameObject;
    public PathEdge edge;

    public TraversalCompletedEventPayload(
        GameObject gameObject,
        PathEdge edge){
        this.gameObject = gameObject;
        this.edge = edge;
    }
}

public struct TraversalFailedEventPayload
{
    public GameObject gameObject;
    public PathEdge edge;

    public TraversalFailedEventPayload(
        GameObject gameObject,
        PathEdge edge){
        this.gameObject = gameObject;
        this.edge = edge;
    }
}

public sealed class EdgeTraverser : MonoBehaviour
{
	private Vector2 previousPosition;
	
	private MovingEntity movingEntity;
	private AiController aiController;
	private Steering steering;
	private Seek seek;
	private Arrive arrive;
	private float stuckThreshold = 0.1f; 
    private float startTime;
	/// <summary>
    /// Gets the edge to traverse.
    /// </summary>
    public PathEdge EdgeToFollow { get; private set; }

    public int TraversalMargin { get; set; }
	
	public void Awake(){
		TraversalMargin = 3;
		movingEntity = GetComponent<MovingEntity>();
		aiController = GetComponent<AiController>();
		seek = GetComponent<Seek>();
		arrive = GetComponent<Arrive>();
        startTime = Time.time;
	}
	
	public void Update(){
		CheckIfStuck();
	}
	
	private void OnEnable(){
		EventManager.Instance.Subscribe<ArrivalEventPayload>(
			Events.Arrival, 
		    OnArrival);
	}
	
	private void OnDisable(){
		EventManager.Instance.Unsubscribe<ArrivalEventPayload>(
			Events.Arrival, 
		    OnArrival);
	}
	
	public bool Traverse(PathEdge edgeToFollow, bool brakeOnApproach, bool stopOnArrival){	
		if ((movingEntity == null || !movingEntity.enabled) && 
		    (aiController == null || !aiController.enabled)){
			return false;
		}
		
		// Seek must exist and not be active
		if (seek == null || seek.TargetPosition.HasValue){
			return false;
		}
		
		// Arrive must exist and not be active
		if (arrive == null || arrive.TargetPosition.HasValue){
			return false;
		}
		
		seek.enabled = seek.isOn = false;
		arrive.enabled = arrive.isOn = false;

        EdgeToFollow = edgeToFollow;

        if (brakeOnApproach){
			if (steering != null){
				steering.enabled = steering.isOn = false;
			}
			
			steering = arrive;
            steering.TargetPosition = 
				(movingEntity != null && movingEntity.enabled) 
					? movingEntity.PositionAt(EdgeToFollow.Destination) 
					: World.Instance.GroundPositionAt(EdgeToFollow.Destination);
			steering.enabled = steering.isOn = true;
			if ((movingEntity == null || !movingEntity.enabled) && aiController != null && aiController.enabled){
				aiController.SetSteering(steering);
			}
        }
        else {
			if (steering != null){
				steering.enabled = steering.isOn = false;
			}
			
			steering = seek;
            steering.TargetPosition = 
				(movingEntity != null && movingEntity.enabled) 
					? movingEntity.PositionAt(EdgeToFollow.Destination) 
					: World.Instance.GroundPositionAt(EdgeToFollow.Destination);
			steering.enabled = steering.isOn = true;
			if ((movingEntity == null || !movingEntity.enabled) && aiController != null && aiController.enabled){
				aiController.SetSteering(steering);
			}
        }

        return true;
    }

    private void OnArrival(Event<ArrivalEventPayload> eventArg){
		ArrivalEventPayload payload = eventArg.EventData;
	
        if (payload.gameObject != gameObject) // event not for us
        {
            return;
        }

        if (EdgeToFollow != null && payload.destination == EdgeToFollow.Destination){
            EventManager.Instance.Enqueue<TraversalCompletedEventPayload>(
                Events.TraversalCompleted,
                new TraversalCompletedEventPayload(payload.gameObject, EdgeToFollow));
        }
    }

    private Vector2 GetPosition(){
        Vector2 pos = new Vector2(float.MaxValue, float.MaxValue);
        if (movingEntity != null && movingEntity.enabled){
            pos = movingEntity.Position2D;
        }
        else if (aiController != null && aiController.enabled){
            pos = aiController.transform.position.To2D();
        }
        else {
            pos = transform.position.To2D();
        }
        return pos;
    }
    private bool IsStuck(){
        //// stuck test based on difference between current and
        //// previous position (perhaps taking expected velocity and elapsed time into account)
        var curVelocity = ((previousPosition-GetPosition()).magnitude /Time.deltaTime);
        if( curVelocity < stuckThreshold){
            return true;
        }
        return false; // for now, just return false
    }

    private void ShutDown(){
        if (seek != null) seek.enabled = seek.isOn = false;
        if (arrive != null) arrive.enabled = arrive.isOn = false;
        if (steering != null) steering.enabled = steering.isOn = false;
    }

    private void CheckIfStuck(){
        if (IsStuck() && (Time.time-startTime) > 10f){
			GameObject traverserGameObject/* = gameObject */; // depends if the EdgeTraverser is attached to the traversing object
			
			if (movingEntity != null && movingEntity.enabled){
				traverserGameObject = movingEntity.gameObject;
			}
			else if (aiController != null && aiController.enabled){
				traverserGameObject = aiController.gameObject;
			}
			else {
				traverserGameObject = gameObject;
			}

		    ShutDown();
            startTime = Time.time;
            EventManager.Instance.Enqueue<TraversalFailedEventPayload>(
                Events.TraversalFailed,
                new TraversalFailedEventPayload(traverserGameObject, EdgeToFollow));
        }
		
		previousPosition = GetPosition();
    }
}