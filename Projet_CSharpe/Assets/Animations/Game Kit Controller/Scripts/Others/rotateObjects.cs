using UnityEngine;
using System.Collections;

public class rotateObjects : MonoBehaviour {
	public Vector3 direccion;
	public float speed = 1;
	public bool rotationEnabled=true;
	void Start () {
	
	}

	void Update () {
		if (rotationEnabled) {
			transform.Rotate (speed * direccion * Time.deltaTime);
		}
	}

	public void enableOrDisableRotation(){
		rotationEnabled=!rotationEnabled;
	}

	public void increaseRotationSpeedTenPercentage(){
		speed += speed * 0.1f;
	}
}
