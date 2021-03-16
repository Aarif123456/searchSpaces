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

using UnityEngine;

// Require a character controller to be attached to the parent game object.
[RequireComponent(typeof(CharacterController))]

public abstract class KinematicWeebleMotor : KinematicMotor 
{ 
	public float pushPower = 2.0f;
	
	// The last collision flags returned from controller.Move
    [NonSerialized]
    public CollisionFlags collisionFlags;

    [NonSerialized]
    public Vector3 hitPoint = Vector3.zero;

    [NonSerialized]
    public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
	
	[NonSerialized]
    public Vector3 groundNormal = Vector3.zero;

    private Vector3 lastGroundNormal = Vector3.zero;
	
	protected CharacterController characterController;
	
	public override bool IsGrounded
	{
		get {
			return characterController.isGrounded;
		}
	}

	public override Vector3 ActualVelocity
	{
		get {
			return characterController.velocity;
		}
	}
			
	protected override void Awake(){
		base.Awake();
		
		characterController = GetComponent<CharacterController>();
	}
	
	protected override void IntegrateVelocity(){
		groundNormal = Vector3.zero;
		
		collisionFlags = characterController.Move(linearVelocity * Time.deltaTime);
		
		lastHitPoint = hitPoint;
        lastGroundNormal = groundNormal;
	}
	
	protected override void IntegrateAngularVelocity(){
		transform.Rotate(angularVelocity * Time.deltaTime);
	}
	
	protected virtual void OnControllerColliderHit(ControllerColliderHit hit){
        if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0){
            if ((hit.point - lastHitPoint).sqrMagnitude > 0.001f || lastGroundNormal == Vector3.zero){
                groundNormal = hit.normal;
            }
            else {
                groundNormal = lastGroundNormal;
            }
        }

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic){
            return;
        }

        if (hit.moveDirection.y < -0.3f){
            return;
        }

        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        body.velocity = pushDirection * pushPower; // TODO: apply force instead
    }
}