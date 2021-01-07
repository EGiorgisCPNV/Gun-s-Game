using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class solarSystem : MonoBehaviour {

	public GameObject sun;
	public float speed;
	public bool useDistance;
	public List<GameObject> planets = new List<GameObject> ();
	//a script to make a simple solar system, where every planet rotates according to the distance to the sun
	void Start () {
		Component[] components=GetComponentsInChildren(typeof(Transform));
		//get all the planets inside the solar system
		foreach (Component c in components) {
			if(c.tag=="moving" && c.tag!="Untagged"){
				planets.Add(c.gameObject);
			}
		}
	}

	void Update () {
		for (int i=0; i<planets.Count; i++) {
			float distance=Vector3.Distance(sun.transform.position,planets[i].transform.position);
			if(useDistance){
				planets[i].transform.RotateAround(sun.transform.position,planets[i].transform.up,(speed/(distance/20))*Time.deltaTime);
			}
			else{
				planets[i].transform.RotateAround(sun.transform.position,planets[i].transform.up,speed*Time.deltaTime);
			}
		}
	}
}