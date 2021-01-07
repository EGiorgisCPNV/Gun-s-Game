using UnityEngine;
using System.Collections;
public class simpleLamp : MonoBehaviour {
	public GameObject lampLight;
	public bool hasLamp;
	public AudioClip switchSound;
	AudioSource audioSource;

	void Start () {
		audioSource = GetComponent<AudioSource> ();
	}

	void Update () {
	
	}
	public void lampPlaced(){
		hasLamp = true;
	}
	public void activateDevice(){
		if (hasLamp) {
			audioSource.PlayOneShot (switchSound);
			lampLight.SetActive (!lampLight.activeSelf);
		}
	}
}
