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

using UnityEngine;

// Add to the component menu.
[AddComponentMenu("Scripts/Entities/AI Moving Entity")]

public class AiMovingEntity : MovingEntity
{
	public bool useFixedUpdate = false;
	public SummingMethods summingMethod = SummingMethods.Blended;
	
	private int lastDitheredSteeringIndex = -1;
	
	public enum SummingMethods { Blended, Prioritized, Dithered };
	
	public void FixedUpdate(){
		if (useFixedUpdate){
			ApplySteering();
		}
	}
	
	public override void Update(){
		base.Update();
		
		if (!useFixedUpdate){
			ApplySteering();
		}
	}
	
	private void ApplySteering(){
		switch (summingMethod){
			case SummingMethods.Blended:
				ApplyBlendedSteering();
				break;
			
			case SummingMethods.Prioritized:
				ApplyPrioritizedSteering();
				break;
			
			case SummingMethods.Dithered:
				ApplyDitheredSteering();
				break;
		}
	}
	
	private void ApplyDitheredSteering(){
		for (int count = 0; count < Steerings.Length; count++){
			lastDitheredSteeringIndex = (lastDitheredSteeringIndex + 1) % Steerings.Length;
			Steering steering = Steerings[lastDitheredSteeringIndex];
			if (steering.enabled && steering.isOn){
				if (steering.CalculateUnweightedSteering(out motor.desiredLinear, out motor.desiredAngular)){
					break;
				}
			}
		}
	}
	
	private void ApplyPrioritizedSteering(){
		foreach (Steering steering in Steerings){
			if (steering.enabled && steering.isOn){
				if (steering.CalculateUnweightedSteering(out motor.desiredLinear, out motor.desiredAngular)){
					break;
				}
			}
		}
	}
	
	private void ApplyBlendedSteering(){
		Vector3? totalWeightedLinear = null;
		Vector3? totalWeightedAngular = null;
		float totalWeightForLinear = 0;
		float totalWeightForAngular = 0;
		
		foreach (Steering steering in Steerings){
			if (steering.enabled && steering.isOn){
				Vector3? weightedLinear;
				Vector3? weightedAngular;
				steering.CalculateWeightedSteering(out weightedLinear, out weightedAngular);
				
				if (weightedLinear.HasValue){
					if (totalWeightedLinear.HasValue){
						totalWeightedLinear += weightedLinear.Value;
					}
					else {
						totalWeightedLinear = weightedLinear.Value;
					}
					
					totalWeightForLinear += steering.Weight;
				}
				
				if (weightedAngular.HasValue){
					if (totalWeightedAngular.HasValue){
						totalWeightedAngular += weightedAngular.Value;
					}
					else {
						totalWeightedAngular = weightedAngular.Value;
					}
					
					totalWeightForAngular += steering.Weight;
				}
			}
		}

		if (totalWeightedLinear.HasValue){
			motor.desiredLinear = totalWeightedLinear.Value / totalWeightForLinear;
		}
		
		if (totalWeightedAngular.HasValue){
			motor.desiredAngular = totalWeightedAngular.Value / totalWeightForAngular;
		}
	}
}