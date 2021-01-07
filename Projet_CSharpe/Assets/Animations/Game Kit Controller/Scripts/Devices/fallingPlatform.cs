using UnityEngine;
using System.Collections;
public class fallingPlatform : MonoBehaviour {
	public float movementSpeed;
	public bool fallInTouch;
	public bool fallInTime;
	public float fallDelay;
	public bool resetDelayInExit;
	public float timeToBackInPosition;
	public float extraForceInFall;
	bool inside;
	bool platformFallen;
	bool movePlatformToPosition;
	float timeOnPlatform;
	float fallenTime;
	Vector3 originalPosition;
	Quaternion originalRotation;
	Rigidbody mainRigidbody;

	void Start () {
		mainRigidbody = GetComponent<Rigidbody> ();
		originalPosition = transform.position;
		originalRotation = transform.rotation;
	}
	void Update () {
		if (!movePlatformToPosition) {
			if (platformFallen && mainRigidbody.velocity.magnitude<1) {
				fallenTime += Time.deltaTime;
				if (fallenTime > timeToBackInPosition) {
					StartCoroutine (moveToOriginalPosition ());

				}
			} else {
				if (inside) {
					if (fallInTouch) {
						mainRigidbody.isKinematic = false;
						mainRigidbody.AddForce (-transform.up * extraForceInFall);
						platformFallen = true;
						fallenTime = 0;
						inside = false;
					}
					if (fallInTime) {
						timeOnPlatform += Time.deltaTime;
						if (timeOnPlatform > fallDelay) {
							mainRigidbody.isKinematic = false;
							mainRigidbody.AddForce (-transform.up * extraForceInFall);
							platformFallen = true;
							fallenTime = 0;
							inside = false;
						}
					}
				}
			}
		}
	}
	void OnCollisionEnter(Collision col){
		if (col.gameObject.tag == "Player" && !inside && !platformFallen) {
			inside = true;
			timeOnPlatform = 0;
		}
	}
	void OnCollisionExit(Collision col){
		if (col.gameObject.tag == "Player" && inside) {
			inside = false;
			if (resetDelayInExit) {
				timeOnPlatform = 0;
			}
		}
	}
	IEnumerator moveToOriginalPosition(){
		platformFallen = false;
		mainRigidbody.isKinematic = true;
		movePlatformToPosition = true;
		while (Vector3.Distance (transform.position, originalPosition) > .01f) {
			transform.position = Vector3.MoveTowards (transform.position, originalPosition, Time.deltaTime * movementSpeed);
			transform.rotation = Quaternion.Slerp (transform.rotation, originalRotation, Time.deltaTime * movementSpeed);
			yield return null;
		}
		movePlatformToPosition = false;
	}
}