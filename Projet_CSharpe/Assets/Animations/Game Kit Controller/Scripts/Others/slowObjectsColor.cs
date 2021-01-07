using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class slowObjectsColor : MonoBehaviour {
	public Color slowColor;
	public bool changeToSlow = true;
	public bool changeToNormal;
	public float lerpSpeed=6;
	public float t=0;
	float slowValue;
	Transform[] parts;
	public List<Renderer> rendererParts=new List<Renderer>();
	public List<Color> originalColor = new List<Color> ();
	public List<Color> transistionColor=new List<Color>();
	int i=0;
	int j=0;

	//this script is attached to an object with the tag slowing, changing the colors of all the renderers inside of it
	void Start () {	
		
	}
	void Update () {
		//change the color smoothly from the original, to the other
		t += Time.deltaTime;
		for (i=0;i<rendererParts.Count;i++){
			if(rendererParts[i]){
				for (j=0;j<rendererParts[i].materials.Length;j++){
					rendererParts[i].materials[j].color = Color.Lerp (rendererParts[i].materials[j].color, transistionColor[i], t);
				}
			}
		}
		//after the 80% of the time has passed, the color will change from the slowObjectsColor, to the original
		if (t>=lerpSpeed*0.8 && changeToSlow) {
			//set the transition color to the original
			changeToSlow=false;
			changeToNormal=true;
			transistionColor=originalColor;
			t=0;
		}
		//when the time is over, set the color and remove the script
		if (t>=lerpSpeed*0.2f && !changeToSlow && changeToNormal) {
			for (i=0;i<rendererParts.Count;i++){
				if(rendererParts[i]){
					for (j=0;j<rendererParts[i].materials.Length;j++){
						rendererParts[i].materials[j].color = transistionColor[i];
					}
				}
			}
			GetComponent<slowObject>().objectToCallFunction.BroadcastMessage ("normalVelocity",SendMessageOptions.DontRequireReceiver);
			Destroy(gameObject.GetComponent<slowObjectsColor>());
		}
	}

	public void startSlowObject(Color newSlowColor, float newSlowValue){
		slowColor = newSlowColor;
		slowValue = newSlowValue;
		//send a message to slow down the object
		GetComponent<slowObject>().objectToCallFunction.BroadcastMessage ("reduceVelocity", slowValue, SendMessageOptions.DontRequireReceiver);
		//get all the renderers inside of it, to change their color with the slowObjectsColor from otherpowers
		Component[] components=GetComponentsInChildren(typeof(Renderer));
		foreach (Renderer child in components){
			if (child.GetComponent<Renderer>().material.HasProperty ("_Color")) {
				for (j=0;j<child.materials.Length;j++){
					rendererParts.Add (child);
					originalColor.Add (child.GetComponent<Renderer>().materials[j].color);
					transistionColor.Add(slowColor);
				}
			}
		}
	}
}