using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class timeBullet : MonoBehaviour{
	public bool timeBulletEnabled;
	public float timeBulletTimeSpeed = 0.1f;
	public bool affectAudioPitch;
	public bool timeBulletActivated;
	public bool previouslyActivated;
	float timeBulletTime = 1;
	inputManager input;
	AudioSource[] audios;
	Coroutine timeCoroutine;

	void Start(){
		input = GetComponent<inputManager> ();
	}
	void Update(){
		if (input.checkInputButton ("Time Bullet", inputManager.buttonType.getKeyDown)) {
			activateTime();
		}
		if (timeBulletActivated) {
			timeBulletTime=timeBulletTimeSpeed;
			Time.timeScale = timeBulletTime ;
			Time.fixedDeltaTime = timeBulletTime * 0.02f;
		}
	}
	public void activateTime(){
		//check that the player is not using a device, the game is not paused and that this feature is enabled
		if (Time.deltaTime != 0 && timeBulletEnabled) {
			timeBulletActivated = ! timeBulletActivated;
			if (timeBulletActivated) {
				timeBulletTime = timeBulletTimeSpeed;
			} else {
				timeBulletTime = 1;
				Time.timeScale = timeBulletTime;
				Time.fixedDeltaTime = timeBulletTime * 0.02f;
			}
			changeAudioPitch ();
		}
	}

	public void disableTimeBullet(){
		if (timeBulletActivated) {
			activateTime ();
			previouslyActivated = true;
		}
	}

	public void reActivateTime(){
		if (previouslyActivated) {
			if (timeCoroutine != null) {
				StopCoroutine (timeCoroutine);
			}
			timeCoroutine = StartCoroutine(reActivateTimeCoroutine());
			previouslyActivated = false;
		}
	}
	IEnumerator reActivateTimeCoroutine(){
		yield return new WaitForSeconds(0.001f);
		activateTime ();
	}
	public void changeAudioPitch(){
		if (affectAudioPitch) {
			audios = FindObjectsOfType (typeof(AudioSource)) as AudioSource[];
			for (int i = 0; i < audios.Length; i++) {
				audios [i].pitch = timeBulletTime;
			}
		}
	}
}