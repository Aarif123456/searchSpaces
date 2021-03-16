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

using System;
using System.Collections;
using System.Collections.Generic;

using Thot.GameAI;

using UnityEngine;

// Add to the component menu.
[AddComponentMenu("Scripts/Experiments/GoalGenerator")]

public class GoalGenerator : MonoBehaviour
{
	public bool chooseRandomly = true;
	
	private readonly Queue<Vector2> destinationQueue = new Queue<Vector2>();
	
	private SearchSpace searchSpace;
	private Vector2? currentDestination;
	
	public void Awake()
	{
		searchSpace = GetComponent<SearchSpace>();
	}
	
	public IEnumerator Start()
	{
		while ( searchSpace == null ||
		        searchSpace.Graph == null)
		{
			yield return 0;
		}
		
		FindPathToNextDestination();
	}
	
	private void OnEnable()
	{
		EventManager.Instance.Subscribe<FollowCompletedEventPayload>(
			Events.FollowCompleted, 
		    OnFollowCompleted);
		
		EventManager.Instance.Subscribe<FollowFailedEventPayload>(
			Events.FollowFailed, 
		    OnFollowFailed);
	}
	
	private void OnDisable()
	{
		EventManager.Instance.Unsubscribe<FollowCompletedEventPayload>(
			Events.FollowCompleted, 
		    OnFollowCompleted);
		
		EventManager.Instance.Unsubscribe<FollowFailedEventPayload>(
			Events.FollowFailed, 
		    OnFollowFailed);
	}
	
	/// <summary>
    /// Called to determine the next target either randomly or from the destination queue.
    /// This posts a path-finding request for the target.
    /// </summary>
    private void FindPathToNextDestination()
    {
		currentDestination = null;
		
		if ( searchSpace == null ||
		     searchSpace.Graph == null)
		{
			return;
		}
		
        if (chooseRandomly)
        {
			currentDestination = searchSpace.GetRandomEntityPosition();
		}
		else if (destinationQueue.Count > 0)
        {
            currentDestination = destinationQueue.Dequeue();
        }
		
		if (currentDestination.HasValue)
		{
			EventManager.Instance.Enqueue<PathRequestEventPayload>(
				Events.PathRequest,
			    new PathRequestEventPayload(searchSpace.gameObject, currentDestination.Value));
		}
	}
	
	/// <summary>
    /// This is called when the path follower successful completes following a path.
    /// </summary>
    protected void OnFollowCompleted(Event<FollowCompletedEventPayload> eventArg)
    {
		FollowCompletedEventPayload payload = eventArg.EventData;
		
        if (payload.gameObject != gameObject) // event not for us
        {
            return;
        }

        FindPathToNextDestination();
    }

    /// <summary>
    /// This is called when the path follower failed to complete following a path.
    /// </summary>
    protected void OnFollowFailed(Event<FollowFailedEventPayload> eventArg)
    {
		FollowFailedEventPayload payload = eventArg.EventData;
			
        if (payload.gameObject != gameObject) // event not for us
        {
            return;
        }
		
		Debug.Log("Failed " + payload.path);
		
//		if (currentDestination.HasValue)
//		{
//	        // re-plan.
//	        ////TODO: be smarter here. Otherwise could keep failing for the same reason.
////	        EventManager.Instance.Enqueue<PathRequestEventPayload>(
////	            Events.PathRequest,
////	            new PathRequestEventPayload(payload.gameObject, currentDestination.Value));
//		}
//		else
//		{
//			//FindPathToNextDestination();
//		}
    }
}
