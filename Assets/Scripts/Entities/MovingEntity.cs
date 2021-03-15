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

using System.Linq;

using Thot.GameAI;

using UnityEngine;

// Add to the component menu.
[AddComponentMenu("Scripts/Entities/Moving Entity")]

public class MovingEntity : Entity
{
	public Motor motor;
	public CharacterController characterController;
	
	private Steering[] steerings;
	
	// The entity's center in the transform
	[SerializeField]
	private Vector3 center;
	public Vector3 Center
	{
		get
		{
			return center;
		}
		
		set
		{
			center = value;
		}
	}
	
	[SerializeField]
	// Internally-assigned Mass for the entity. If the entity has a rigidbody, this value will be disregarded and
	// the rigidbody's mass value will be used instead.
	private float internalMass = 1;
	
	// If the entity has a rigidbody, its mass will be updated whenever this property is set.
	public float Mass 
	{
		get
		{
			return GetComponent<Rigidbody>() != null ? GetComponent<Rigidbody>().mass : internalMass;
		}
		
		set
		{
			if (GetComponent<Rigidbody>() != null )
			{
				GetComponent<Rigidbody>().mass = value;
			}
			else
			{
				internalMass = value;
			}
		}
	}
	
	public bool isAiControlled
	{
		get
		{
			return motor != null && motor.isAiControlled;
		}
		
		set
		{
			if (motor != null)
			{
				motor.isAiControlled = value;
			}
		}
	}
	
	// Entity's position. The entity's position is the transform's position displaced 
	// by the entity center.
	public Vector3 Position 
	{
		get 
		{
			return transform.position + center;
		}
	}
	
	public Vector2 Position2D
	{
		get
		{
			return Position.To2D();
		}
	}
	
	// The entity's radius.
	[SerializeField]
	private float radius = 1;
	public float Radius 
	{
		get 
		{
			return radius;
		}
		
		set 
		{
			radius = Mathf.Clamp(value, 0.01f, float.MaxValue);		
		}
	}
	
	// The entity's height.
	[SerializeField]
	[HideInInspector]
	private float height = 1;
	public float Height 
	{
		get 
		{
			return height;
		}
		
		set 
		{
			height = Mathf.Clamp(value, 0.01f, float.MaxValue);		
		}
	}
	
	public float MaximumSpeed
	{
		get
		{
			return motor.maximumSpeed;
		}
		
		set
		{
			motor.maximumSpeed = Mathf.Clamp(value, 0, float.MaxValue);
		}
	}
	
	public float MaximumAngularSpeed
	{
		get
		{
			return motor.maximumAngularSpeed;
		}
		
		set
		{
			motor.maximumAngularSpeed = Mathf.Clamp(value, 0, float.MaxValue);
		}
	}
	
	public float MaximumAcceleration
	{
		get
		{
			return motor.maximumAcceleration;
		}
		
		set
		{
			motor.maximumAcceleration = Mathf.Clamp(value, 0, float.MaxValue);
		}
	}
	
	public float MaximumAngularAcceleration
	{
		get
		{
			return motor.maximumAngularAcceleration;
		}
		
		set
		{
			motor.maximumAngularAcceleration = Mathf.Clamp(value, 0, float.MaxValue);
		}
	}
	
	public Vector3 LinearVelocity
	{
		get
		{
			return motor.LinearVelocity;
		}
	}
	
	public Vector3 AngularVelocity
	{
		get
		{
			return motor.AngularVelocity;
		}
	}
	
	// Array of steering behaviors
	public Steering[] Steerings 
	{
		get 
		{
			return steerings;
		}
	}
	
	public PathManager PathManager { get; set; }
	
	public override void Awake()
    {
		base.Awake();
		
		motor = GetComponent<Motor>();
		characterController = GetComponent<CharacterController>();
		if (characterController != null)
		{
			radius = characterController.radius;
			height = characterController.height;
			center = characterController.center;
		}
		steerings = GetComponents<Steering>().OrderByDescending(s => s.Priority).ToArray();
		
		PathManager = GetComponent<PathManager>();
    }
	
	public Vector3 PositionAt(Vector2 point)
	{
		return PositionAt(point, 0);
	}
	
	public Vector3 PositionAt(Vector2 point, float heightOffset)
	{
		float entityHeightOffset = Center.y + Height / 2;
		float groundHeightOffset = World.Instance.GroundHeightAt(point);
		return new Vector3(point.x, groundHeightOffset + entityHeightOffset + heightOffset, point.y);
	}
	
	public bool IsEntityInObstacle(Vector2 point)
	{
		Vector3 groundPosition = World.Instance.GroundPositionAt(point);
		
		foreach (Collider collider in Physics.OverlapSphere(groundPosition, Radius))
		{
	        if (collider != null)
			{
				Entity entity = collider.gameObject.GetComponent<Entity>();
				if (entity != null && entity.isObstacle)
				{
					return true;
				}
			}
		}
		
		return false;
	}
}