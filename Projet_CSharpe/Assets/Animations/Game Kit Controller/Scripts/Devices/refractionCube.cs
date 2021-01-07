using UnityEngine;
using System.Collections;

public class refractionCube : MonoBehaviour {

	public Color refractionCubeColor;
	//set the color in the cube, so when the laser is reflected, the color is applied to the laser
	void Start () {
		GetComponent<Renderer> ().material.color = refractionCubeColor;
	}
}
