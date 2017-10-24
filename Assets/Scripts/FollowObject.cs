using UnityEngine;
using System.Collections;
//This script is very simple. It just tells the GameObject it is attached to to follow
//another object every frame.
public class FollowObject : MonoBehaviour {

	public Transform followObject; //this will be assigned by the game manager script, which knows about the player car
	public float offset; //we want it to sit a little behind the followObject. We'll set this in the inspector

	// FixedUpdate is called at fixed intervals, when the physics engine is updated.
	// Since that's when we're moving the player object, it's a great time to move the camera too
	// So they stay in sync
	void FixedUpdate () {
		//we aren't allowed to just change a single component of a vector, so we need to 
		//store this object's current position in a temporary variable
		Vector3 currentPosition = transform.position;

		//Mathf.Lerp is a function for blending two values some amount
		//if I give it the parameters (a,b,0.5f) then it will return a 50% blend of a and b
		//if I give it the parameter (a,b,0.1f) then it will be 10% a and 90% b
		//here we use it to make the camera go 50% of the way to the followObject's position every frame.
		currentPosition.y = Mathf.Lerp (currentPosition.y, followObject.position.y + offset, 0.5f);

		//now that we've modified the position, we can copy the value back to the transform
		transform.position = currentPosition;
	}
}
