using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class footStep : MonoBehaviour {
	public LayerMask layer;
	public footType footSide;
	bool touching;
	footStepManager soundManager;
	GameObject currentSurface;
	AudioSource audioSource;

	public enum footType{
		left, right, center
	}
	void Start(){
		soundManager = GetComponentInParent<footStepManager> ();
		audioSource = GetComponent<AudioSource> ();
	}
	//check when the trigger hits a surface, and play one shoot of the audio clip according to the layer of the hitted collider
	void OnTriggerEnter(Collider col){
		//compare if the layer of the hitted object is not in the layer configured in the inspector
		if(soundManager.soundsEnabled && (1<<col.gameObject.layer & layer.value)==1<<col.gameObject.layer && col.gameObject.tag != "Player") {
			touching=true;
			//get the gameObject touched by the foot trigger
			currentSurface=col.gameObject;
			//check the footstep frequency
			if (touching) {
				//get the audio clip according to the type of surface, mesh or terrain
				AudioClip soundEffect = soundManager.getSound(LayerMask.LayerToName(currentSurface.layer).ToString(),transform.position,col.gameObject,footSide);
				if(soundEffect){
					playSound( soundEffect);
				}
			}
		}
	}
	//play one shot of the audio
	void playSound(AudioClip clip){
		audioSource.PlayOneShot( clip,Random.Range (0.8f, 1.2f));
	}
}