

using UnityEngine;
using System.Collections.Generic;
using System;
namespace Thot.GameAI{
    public struct PathRequestEventPayload
    {
        public GameObject gameObject;
        public Vector2 destination;

        public PathRequestEventPayload(
            GameObject gameObject,
            Vector2 destination){
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
            Path path){
            this.gameObject = gameObject;
            this.path = path;
        }
    }
    
    [Serializable]
    public class Weeble 
    {
        public GameObject weeble  { get; private set; }
        /* Used for manual debugging */
        public bool ignoreObstruction { get; set; }
        /* Used to control path obstruction -unimplemented */
        public bool requestPath  { get; set; }
        public SearchSpace searchSpace  { get; private set; }
        public MovingEntity Mover { get; private set; }
        public TimeSlicedAStarSearch Searcher { get; private set; }
        public Vector2 CurrentDestination { get; set; }
        public Vector2 RequestorPosition2D {
            get {
                return (Mover != null && Mover.enabled) ? Mover.Position2D : weeble.transform.position.To2D();
            }
        }

        public Weeble(GameObject weebleObject){
            weeble = weebleObject;
            ignoreObstruction = false;
            requestPath= false;
            Searcher=null;
            Mover = weebleObject.GetComponent<MovingEntity>();

            weebleObject.AddComponent<TimeSlicedAStarSearch>();
            Searcher = weeble.GetComponent<TimeSlicedAStarSearch>();
            searchSpace = Weeble.GetSearchSpace(weebleObject);
        }

        private static SearchSpace GetSearchSpace(GameObject weeble){
            foreach (SearchSpace ss in weeble.GetComponents<SearchSpace>()){
                if (ss.enabled){
                    return ss;
                }
            }
            Debug.LogWarning("no search-space found :(");
            return null;
        }

        public bool OngoingUpdatePath(){
            return !Searcher.IsDone();
        }

        public void UpdatePath(int source){
            var CurrentPath = Searcher.GetPathToTarget(source);
            var path = new Path(
                    weeble,
                    RequestorPosition2D, 
                    CurrentDestination, 
                    CurrentPath,
                    searchSpace.Graph);
            PathReadyEventPayload result = new PathReadyEventPayload(weeble, path);
            EventManager.Instance.Fire<PathReadyEventPayload>(Events.PathReady, result); 
        }

        public int GetClosestNodeToWeeble(){
            var source = searchSpace.GetClosestNodeToPosition(RequestorPosition2D);
            if (source != Node.INVALID_NODE_INDEX){
                // Requestor may be inside or too close to obstruction
                // so let's find the closest node to warp to
                source = searchSpace.GetClosestNodeToPosition(RequestorPosition2D, true);
                if (source == SearchSpace.NO_CLOSEST_NODE_FOUND) Debug.LogWarning("Returning Invalid source node ");
           }
           return source;
       }

        public void ResetSearch(Vector2 Destination){
            CurrentDestination = Destination;
            int source = GetClosestNodeToWeeble();
            if(source == SearchSpace.NO_CLOSEST_NODE_FOUND) return; //screwed
            int target = searchSpace.GetClosestNodeToPosition(CurrentDestination);
            ////TODO: should we instead move the target to closest valid node??
            if (target == SearchSpace.NO_CLOSEST_NODE_FOUND) return;
            Searcher.Reset(searchSpace.Graph, source, target);
        }
    }

    // Add to the component menu.
    [AddComponentMenu("Scripts/Managers/Universal Path Manager")]

    public sealed class UniversalPathManager : MonoBehaviour
    {
        private static UniversalPathManager _instance;

        /// Gets the accessor for the <see cref="UniversalPathManager"/> singleton instance.
        public static UniversalPathManager Instance
        {
            get {
                return _instance;
            }
        }

        public void Awake(){
            if (_instance != null){
                Debug.LogWarning("Multiple instances of UniversalPathManager!");
            }
            
            _instance = this;
        }

       
        [SerializeField]
        public List<Weeble> weebles;
        /* Used to show marker */
        public bool showPath = true;

        public void Start(){
            GameObject[] weeblesGameObjects =  GameObject.FindGameObjectsWithTag("Weeble");
            weebles = new List<Weeble>();
            foreach(var weebleObject in weeblesGameObjects){
                var weeble = new Weeble(weebleObject);
                weebles.Add(weeble);
            }
        }

        public void Update(){
            foreach(var weeble in weebles){
                var searchSpace = weeble.searchSpace;
                if (searchSpace == null || !searchSpace.enabled) continue;

                int source = weeble.GetClosestNodeToWeeble();
                if(weeble.OngoingUpdatePath()){
                    weeble.UpdatePath(source);
                }
                // This is for manual testing via the Inspector
                if (weeble.requestPath){
                    PathRequestEventPayload request = 
                        new PathRequestEventPayload(searchSpace.gameObject, searchSpace.GetRandomEntityPosition());
                    EventManager.Instance.Enqueue<PathRequestEventPayload>(Events.PathRequest, request);
                    weeble.requestPath = false;
                }
            }    
        }

        private void OnEnable(){
            EventManager.Instance.Subscribe<PathRequestEventPayload>(Events.PathRequest, OnPathRequest);
        }
        
        private void OnDisable(){
            EventManager.Instance.Unsubscribe<PathRequestEventPayload>(Events.PathRequest, OnPathRequest);
        }

        private void OnPathRequest(Event<PathRequestEventPayload> eventArg){
            foreach(var weeble in weebles){
                var searchSpace = weeble.searchSpace;
                if (searchSpace == null ) continue;
                
                PathRequestEventPayload request = eventArg.EventData;
                if (request.gameObject != searchSpace.gameObject) continue; // request not for us
                weeble.ResetSearch(request.destination);
            }
        }
    }
}

