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

using Thot.GameAI;

using UnityEngine;

public struct ArrivalEventPayload
{
    public GameObject gameObject;
    public Vector2 destination;

    public ArrivalEventPayload(GameObject gameObject, Vector2 destination)
    {
        this.gameObject = gameObject;
        this.destination = destination;
    }
}

public abstract class Steering : MonoBehaviour 
{	
	public bool isOn = true;
	
	// Time between ticks
	public float tickLapse = 0;
	
	// Optional target object. If specified, the target position will get set to the target object's position.
    public GameObject targetObject;

    // The position to seek to.
    public Vector3? targetPosition;
	
	public float satisfactionRadius = 1;
	
	// Comment out the next line to see distance to target in the inspector view.
    [NonSerialized]
    public float distanceToTarget;
	
	// Regulates frequency of steering updates
	protected TickManager tickManager;
	
	public bool IsKinematic { get; protected set; }
	
	public float MaximumSpeed
	{
		get
		{
			if (MovingEntity != null && MovingEntity.enabled)
			{
				return MovingEntity.MaximumSpeed;
			}
			else if (Motor != null && Motor.enabled)
			{
				return Motor.maximumSpeed;
			}
			else
			{
				return 0;
			}
		}
	}
	
	public float MaximumAcceleration
	{
		get
		{
			if (MovingEntity != null && MovingEntity.enabled)
			{
				return MovingEntity.MaximumAcceleration;
			}
			else if (Motor != null && Motor.enabled)
			{
				return Motor.maximumAcceleration;
			}
			else
			{
				Debug.Log("NO AC");
				return 0;
			}
		}
	}
	
	public Vector3 LinearVelocity
	{
		get
		{
			if (MovingEntity != null && MovingEntity.enabled)
			{
				return MovingEntity.LinearVelocity;
			}
			else if (Motor != null && Motor.enabled)
			{
				return Motor.LinearVelocity;
			}
			else
			{
				return Vector3.zero;
			}
		}
	}
	
    public GameObject TargetObject
    {
        get
        {
            return targetObject;
        }

        set
        {
            targetObject = value;
            
            if (targetObject != null)
            {
                targetPosition = targetObject.transform.position;
            }
        }
    }
	
    public Vector3? TargetPosition
    {
        get
        {
            return targetPosition;
        }

        set
        {
            targetPosition = value;
            targetObject = null;
        }
    }
	
	protected GameObject movingObject;
	
	// Game object that this steering behaviour will influence
	public GameObject MovingObject
	{
		get 
		{ 
			return movingObject; 
		}
	}
	
	protected MovingEntity movingEntity;
	
	// Entity that this steering behaviour will influence
	public MovingEntity MovingEntity 
	{
		get 
		{ 
			return movingEntity; 
		}
	}
	
	protected Motor motor;
	public Motor Motor 
	{
		get 
		{ 
			return motor; 
		}
	}
	
	[SerializeField]
	protected float weight = 1;
	
	// Weight assigned to this steering behaviour
	public float Weight 
	{
		get 
		{
			return weight;
		}
		set 
		{
			weight = value;
		}
	}
	
	[SerializeField]
	protected float priority = 1;
	
	// Priority assigned to this steering behaviour
	public float Priority 
	{
		get 
		{
			return priority;
		}
		set 
		{
			priority = value;
		}
	}

	protected virtual void Awake()
	{
		movingObject = gameObject;
		movingEntity = GetComponent<MovingEntity>();
		motor = GetComponent<Motor>();
		tickManager = new TickManager();
	}
	
	public bool CalculateWeightedSteering(out Vector3? weightedLinear, out Vector3? weightAngular)
	{	
		Vector3? linear = null;
		Vector3? angular = null;
		
		if (tickManager.ShouldTick(tickLapse))
		{	
			CalculateSteering(out linear, out angular);	
		}
		
		if (linear.HasValue)
		{
			weightedLinear = linear.Value * Weight;
		}
		else
		{
			weightedLinear = null;
		}
		
		if (angular.HasValue)
		{
			weightAngular = angular.Value * Weight;
		}
		else
		{
			weightAngular = null;
		}
		
		return linear.HasValue || angular.HasValue;	
	}
	
	public bool CalculateUnweightedSteering(out Vector3? linear, out Vector3? angular)
	{	
		linear = null;
		angular = null;
		
		if (tickManager.ShouldTick(tickLapse))
		{	
			return CalculateSteering(out linear, out angular);	
		}
		
		return false;
	}
	
	protected abstract bool CalculateSteering(out Vector3? linear, out Vector3? angular);
}
