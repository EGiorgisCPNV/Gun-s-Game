using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class openStructure : MonoBehaviour {
	public List<GameObject> objectsToFade=new List<GameObject>();
	public Shader transparent;
	public Shader normalShader;
	GameObject[] walls;
	public bool activate;
	public bool changed;
	public int option=1;
	int i;
	//just a simple script to disable and enable part of the structure, using a transparent shader, so the player can go outside
	void Start () {
		walls = GameObject.FindGameObjectsWithTag ("fade");
		for (i=0; i<walls.Length; i++) {
			objectsToFade.Add(walls[i]);
		}
	}

	void Update () {
		if (activate) {
			if(!changed){
				changeShader();
			}
			for (i=0; i<objectsToFade.Count; i++) {
				Color alpha = objectsToFade [i].GetComponent<Renderer>().material.color;
				alpha.a +=( Time.deltaTime / 2)*option;
				objectsToFade [i].GetComponent<Renderer>().material.color = alpha;
				if (alpha.a <= 0 || alpha.a >= 1) {
					activate=false;
					disableObjects();
				}
			}
		}
	}
	void changeStructureState(){
		option *= (-1);
		activate=true;
	}
	void disableObjects(){
		for (int i=0; i<objectsToFade.Count; i++) {
			if(option==-1){
				objectsToFade[i].GetComponent<Collider>().enabled=false;
			}
			else{
				objectsToFade[i].GetComponent<Collider>().enabled=true;
				objectsToFade [i].GetComponent<Renderer>().material.shader=normalShader; 
				objectsToFade [i].GetComponent<Renderer>().material.color=Color.white;
			}
		}
		if (option == 1) {
			changed=false;
		}
	}
	void changeShader(){
		for (int i=0; i<objectsToFade.Count; i++) {
			if(option==-1){
				objectsToFade [i].GetComponent<Renderer>().material.shader=transparent; 
			}
		}
		changed = true;
	}
}
