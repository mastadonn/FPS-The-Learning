using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// C# translation from http://answers.unity3d.com/questions/155907/basic-movement-walking-on-walls.html
/// Author: UA @aldonaletto 
/// </summary>

// Prequisites: create an empty GameObject, attach to it a Rigidbody w/ UseGravity unchecked
// To empty GO also add BoxCollider and this script. Makes this the parent of the Player
// Size BoxCollider to fit around Player model.

[RequireComponent(typeof(Rigidbody))]
public class QM_CharController : MonoBehaviour {
	public float   moveSpeed     = 6;  // move speed
	public float   sprintSpeed   = 1.2f;  // move sprintSpeed multiplyer
	public float   turnSpeed     = 90; // turning speed [degrees/second]
	public float   lerpSpeed     = 15; // smoothing speed
	public float   gravity       = 7; // gravity acceleration
	private bool    isGrounded;
	public float   deltaGround   = 0.2f; // character is grounded up to this distance
	public float   alignGround   = 1.5f; // character will align at this distance
	public float   jumpSpeed     = 10;   // vertical jump initial speed
	public float   jumpRange     = 2;   // range to detect target wall
	public float   jumpRotationSpeed     = 1.5f;   // how fast it takes to jump to a wall, Default was 1
	private Vector3 surfaceNormal; // current surface normal
	private Vector3 myNormal;      // character normal
	private float   distGround;    // distance from character position to ground
	private bool    jumping       = false; // flag &quot;I'm jumping to wall&quot;
	// private float   vertSpeed     = 0;     // vertical jump current speed

	private Transform   myTransform;
	private Rigidbody   rb;
	public  BoxCollider boxCollider; // drag BoxCollider ref in editor

	Vector3 moveAmount;
	Vector3 smoothMoveVelocity;

	private void Start() {
		myNormal = transform.up; // normal starts as character up direction
		myTransform = transform;
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true; // disable physics rotation
		// distance from transform.position to ground
		distGround = boxCollider.size.y - boxCollider.center.y;
	}

	private void FixedUpdate() {
		// apply constant weight force according to character normal:
		rb.AddForce(-gravity*rb.mass*myNormal);


		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rb.MovePosition(rb.position + localMove);
		
	

	

	}



	private void Update() {
		// jump code - jump to wall or simple jump
		if (jumping) return; // abort Update while jumping to a wall

		Ray ray;
		RaycastHit hit;

//		if (Input.GetButtonDown("Jump")) { // jump pressed:
//			ray = new Ray(myTransform.position, myTransform.forward);
//			if (Physics.Raycast(ray, out hit, jumpRange)) { // wall ahead?
//				JumpToWall(hit.point, hit.normal); // yes: jump to the wall
//			} else if (isGrounded) { // no: if grounded, jump up
//				rb.velocity += jumpSpeed * myNormal;
//			}
//		}

		if (Input.GetButtonDown("Jump")) { // jump pressed:
			ray = new Ray(myTransform.position, myTransform.forward);
			if (isGrounded) { // no: if grounded, jump up
				rb.velocity += jumpSpeed * myNormal;

			}
		}


		if (Input.GetButtonDown("Climb")) { // jump pressed:
			
			ray = new Ray(myTransform.position, myTransform.forward);
			if (Physics.Raycast(ray, out hit, jumpRange)) { // wall ahead?
				JumpToWall(hit.point, hit.normal); // yes: jump to the wall
//			} else if (isGrounded) { // no: if grounded, jump up
//				rb.velocity += jumpSpeed * myNormal;
				Debug.Log ("Climbing");
			}
		}


		// movement code - turn left/right with Horizontal axis:
//		myTransform.Rotate(0, Input.GetAxis("Horizontal")*turnSpeed*Time.deltaTime, 0);
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");

		Vector3 moveDir = new Vector3(inputX,0, inputY).normalized;

		if (Input.GetButton ("Sprint")) {
			Vector3 targetMoveAmount = moveDir * moveSpeed * sprintSpeed;
			moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);

		}
			else{
				Vector3 targetMoveAmount = moveDir * moveSpeed;
			 	moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);

			}




		// update surface normal and isGrounded:
		ray = new Ray(myTransform.position, -myNormal); // cast ray downwards
		if (Physics.Raycast(ray, out hit )) { // use it to update myNormal and isGrounded
			isGrounded = hit.distance <= distGround + deltaGround;
			if(hit.distance <= alignGround){
//				Debug.Log ("aligned to ground");
				surfaceNormal = hit.normal;
			}

		} else {
			isGrounded = false;
			// assume usual ground normal to avoid "falling forever"
			surfaceNormal = Vector3.up;
		}
		myNormal = Vector3.Lerp(myNormal, surfaceNormal, lerpSpeed*Time.deltaTime);
		// find forward direction with new myNormal:
		Vector3 myForward = Vector3.Cross(myTransform.right, myNormal);
		// align character to the new myNormal while keeping the forward direction:
		Quaternion targetRot = Quaternion.LookRotation(myForward, myNormal);
		myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRot, lerpSpeed*Time.deltaTime);







	}

	private void JumpToWall(Vector3 point, Vector3 normal) {
		// jump to wall
		jumping = true; // signal it's jumping to wall
		rb.isKinematic = true; // disable physics while jumping
		Vector3 orgPos = myTransform.position;
		Quaternion orgRot = myTransform.rotation;
		Vector3 dstPos = point + normal * (distGround - 1f); // will jump to 0.5 above wall
		Vector3 myForward = Vector3.Cross(myTransform.right, normal);
		Quaternion dstRot = Quaternion.LookRotation(myForward, normal);
		Debug.Log (dstPos);

		StartCoroutine (jumpTime (orgPos, orgRot, dstPos, dstRot, normal));
		//jumptime
	}

	private IEnumerator jumpTime(Vector3 orgPos, Quaternion orgRot, Vector3 dstPos, Quaternion dstRot, Vector3 normal) {
		for (float t = 0.0f; t < 1f; ) {
			t += Time.deltaTime*jumpRotationSpeed;
			myTransform.position = Vector3.Lerp(orgPos, dstPos, t);
			myTransform.rotation = Quaternion.Slerp(orgRot, dstRot, t);
			yield return null; // return here next frame
		}
		myNormal = normal; // update myNormal
		rb.isKinematic = false; // enable physics
		jumping = false; // jumping to wall finished
	}
}