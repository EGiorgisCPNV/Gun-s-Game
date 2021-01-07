using UnityEngine;
using System.Collections;
public class slowObject : MonoBehaviour {
	public GameObject objectToCallFunction;

	void Start () {
		if (!objectToCallFunction) {
			objectToCallFunction = gameObject;
		}
	}
}