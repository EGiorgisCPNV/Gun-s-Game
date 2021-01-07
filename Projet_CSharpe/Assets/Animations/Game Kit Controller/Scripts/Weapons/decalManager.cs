using UnityEngine;
using System.Collections;
public class decalManager : MonoBehaviour {
	public bool fadeDecals;
	public float fadeSpeed;
	public static Transform decalParent;
	public static bool fadeDecalsValue;
	public static float fadeSpeedValue;

	void Start () {
		if (!decalParent) {
			decalParent = new GameObject ().transform;
			decalParent.name = "DecalParent";
		}
		fadeDecalsValue = fadeDecals;
		fadeSpeedValue = fadeSpeed;
	}
	void Update () {

	}

	public static void setScorch(Quaternion rotation, GameObject scorch, RaycastHit hit, GameObject collision){
		//set the position of the scorch according to the hit point
		if (!collision.GetComponent<characterDamageReceiver> ()) {
			GameObject newScorch = Instantiate (scorch);
			newScorch.transform.rotation = rotation;
			newScorch.transform.position = hit.point + hit.normal * 0.03f;
			//get the surface normal to rotate the scorch to that angle
			Vector3 myForward = Vector3.Cross (newScorch.transform.right, hit.normal);
			Quaternion dstRot = Quaternion.LookRotation (myForward, hit.normal);
			newScorch.transform.rotation = dstRot;
			if (collision.GetComponent<Rigidbody> ()) {
				newScorch.transform.SetParent (collision.transform);
			} else if (collision.GetComponent<vehicleDamageReceiver> ()) {
				newScorch.transform.SetParent (collision.GetComponent<vehicleDamageReceiver> ().vehicle.transform);
			} else {
				newScorch.transform.SetParent (decalParent);
			}
			if (fadeDecalsValue) {
				newScorch.AddComponent<fadeObject> ().activeVanish (fadeSpeedValue);
			}
		}
	}
}