

using UnityEngine;
using System.Collections.Generic;
namespace Thot.GameAI{
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

    // Add to the component menu.
    [AddComponentMenu("Scripts/Managers/Universal Path Manager")]

    public sealed class UniversalPathManager : MonoBehaviour
    {
        private static UniversalPathManager _instance;

        /// <summary>
        /// Gets the accessor for the <see cref="UniversalPathManager"/> singleton instance.
        /// </summary>
        public static UniversalPathManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Awake()
        {
            if (_instance != null)
            {
                Debug.Log("Multiple instances of UniversalPathManager!");
            }
            
            _instance = this;
        }

        GameObject[] weebles;
        List<SearchSpace> searchSpaces;
        /* Used to control path obstruction -unimplemented */
        public bool[] ignoreObstructions;
        /* Used for manual debugging */
        public bool[] requestPaths;
        /* Used to show marker */
        public bool showPath = true;

        public void Start()
        {
            weebles =  GameObject.FindGameObjectsWithTag("Weeble");
            searchSpaces = new List<SearchSpace>();
            foreach(var weeble in weebles){
                SearchSpace searchSpace = GetSearchSpace(weeble);
                searchSpaces.Add(searchSpace);
            }
            requestPaths = new bool[weebles.Length];
            ignoreObstructions = new bool[weebles.Length];
        }
        
        

        public void Update()
        {
            for(int i=0; i<weebles.Length; i++){
                var searchSpace = searchSpaces[i];
                if (searchSpace == null || !searchSpace.enabled) continue;
                var requestPath = requestPaths[i];
                // This is for manual testing via the Inspector
                if (requestPath)
                {
                    PathRequestEventPayload request = 
                        new PathRequestEventPayload(searchSpace.gameObject, searchSpace.GetRandomEntityPosition());
                    EventManager.Instance.Enqueue<PathRequestEventPayload>(Events.PathRequest, request);
                    requestPaths[i] = false;
                }
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
            for(int i=0; i<weebles.Length; i++){
                var searchSpace = searchSpaces[i];
                if (searchSpace == null ) continue;
                
                PathRequestEventPayload request = eventArg.EventData;
                if (request.gameObject != searchSpace.gameObject) continue; // request not for us
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
                        continue; // screwed
                    }
                }
                
                int target = searchSpace.GetClosestNodeToPosition(request.destination);
                if (target == SearchSpace.NO_CLOSEST_NODE_FOUND)
                {
                    ////TODO: should we instead move the target to closest valid node??
                    continue;
                }
                
                var currentSearch = new AStarSearch(searchSpace.Graph, source, target);
                
                var path = new Path(
                        searchSpace.gameObject,
                        requestorPosition2D, 
                        request.destination, 
                        currentSearch.GetPathToTarget(),
                        searchSpace.Graph);
                
                PathReadyEventPayload result =
                    new PathReadyEventPayload(request.gameObject, path);
                EventManager.Instance.Enqueue<PathReadyEventPayload>(Events.PathReady, result); 
            }
        }

        private SearchSpace GetSearchSpace(GameObject weeble)
        {
            foreach (SearchSpace ss in weeble.GetComponents<SearchSpace>())
            {
                if (ss.enabled)
                {
                    return ss;
                }
            }
            Debug.LogWarning("no search-space found :(");
            return null;
        }
    }
}

