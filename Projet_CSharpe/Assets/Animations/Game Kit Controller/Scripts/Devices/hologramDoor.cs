using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class hologramDoor : MonoBehaviour {
	public string unlockedText;
	public string lockedText;
	public string openText;
	public string hologramIdle;
	public string hologramInside;
	public AudioClip enterSound;
	public AudioClip exitSound;
	public AudioClip lockedSound;
	public AudioClip openSound;
	public float openDelay;
	public Color lockedColor;
	public GameObject doorToOpen;
	public List<Text> hologramText = new List<Text> ();
	public List<GameObject> holograms = new List<GameObject> ();
	public List<GameObject> hologramCentralRing = new List<GameObject> ();
	List<Image> otherHologramParts = new List<Image> ();
	List<RawImage> hologramParts = new List<RawImage> ();
	List<Color> originalColors = new List<Color> ();
	List<Color> lockedColors = new List<Color> ();
	List<Animation> hologramsAnimations = new List<Animation> ();
	bool insidePlayed;
	bool doorLocked;
	bool inside;
	bool openingDoor;
	bool hologramOccupied;
	int i;
	string regularStateText;
	doorSystem doorManager;
	Color newColor;
	AudioSource audioSource;

	void Start () {
		//get all the raw images components in the hologram
		for (i = 0; i < holograms.Count; i++) {
			Component[] hologramsParts = holograms[i].GetComponentsInChildren (typeof(RawImage));
			foreach (Component c in hologramsParts) {
				//store the raw images
				hologramParts.Add (c.GetComponent<RawImage> ());
				//store the original color of every raw image
				originalColors.Add (c.GetComponent<RawImage> ().color);
				//for every color, add a locked color
				lockedColors.Add (lockedColor);
			}
			//store every animation component
			hologramsAnimations.Add (holograms [i].GetComponent<Animation> ());
		}
		for (i = 0; i < holograms.Count; i++) {
			//get the image components in the hologram
			Component[] hologramsParts = holograms [i].GetComponentsInChildren (typeof(Image));
			foreach (Component c in hologramsParts) {
				//store every component in the correct list
				otherHologramParts.Add (c.GetComponent<Image> ());
				originalColors.Add (c.GetComponent<Image> ().color);
				lockedColors.Add (lockedColor);
			}
		}
		//check if the door that uses the hologram is locked or not, to set the text info in the door
		string newText = "";
		if (doorToOpen.GetComponent<doorSystem> ().locked) {
			doorLocked = true;
			newText = lockedText;
			for (i = 0; i < hologramParts.Count; i++) {
				hologramParts [i].color = lockedColors[i];
			}
		} else {
			newText = unlockedText;
		}
		regularStateText = newText;
		//set the text in the hologram
		setHologramText (regularStateText);
		//get the door system component of the door
		doorManager = doorToOpen.GetComponent<doorSystem> ();
		audioSource = GetComponent<AudioSource> ();
	}

	void Update () {
		//if the player is not inside, play the normal rotating animation
		if (!inside) {
			for (i = 0; i < hologramsAnimations.Count; i++) {
				if (!hologramsAnimations[i].IsPlaying (hologramIdle)) {
					hologramsAnimations[i].Play (hologramIdle);
				}
			}
		} 
		//if the player is inside the trigger, play the open? animation of the hologram and stop the rotating animation
		if(inside && !insidePlayed){
			for (i = 0; i < hologramsAnimations.Count; i++) {
				hologramsAnimations [i].Stop ();
			}
			for (i = 0; i < hologramCentralRing.Count; i++) {
				hologramCentralRing [i].GetComponent<Animation> () [hologramInside].speed = 1;
				hologramCentralRing [i].GetComponent<Animation> ().Play (hologramInside);
			}
			insidePlayed = true;
		}
		//if the door has been opened, and now it is closed and the player is not inside the trigger, set the alpha color of the hologram to its regular state
		if (openingDoor && doorManager.doorState == doorSystem.doorCurrentState.closed  && !doorManager.moving && !inside) {
			openingDoor = false;
			StartCoroutine (changeTransparency (false));
		}
	}
	//if the player is inside the trigger and press the activate device button, check that the door is not locked and it is closed
	public void activateDevice(){
		if (!doorLocked && doorManager.doorState == doorSystem.doorCurrentState.closed  && !doorManager.moving && !hologramOccupied) {
			//fade the hologram colors and open the door
			audioSource.PlayOneShot (openSound);
			StartCoroutine (changeTransparency (true));
			StartCoroutine (openDoor ());
		}
	}
	//this fades and turns back the alpha value of the colors in the hologram, according to if the door is opening or closing
	IEnumerator changeTransparency(bool state){
		hologramOccupied = true;
		int mult = 1;
		if (state) {
			mult = -1;
		} 
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			for (i = 0; i < hologramParts.Count; i++) {
				Color alpha = hologramParts [i].color;
				alpha.a += Time.deltaTime*mult*3;
				hologramParts [i].color = alpha;
			}
			for (i = 0; i < hologramText.Count; i++) {
				Color alpha = hologramText [i].color;
				alpha.a += Time.deltaTime*mult*3;
				hologramText [i].color = alpha;
			}
			for (i = 0; i < otherHologramParts.Count; i++) {
				Color alpha = otherHologramParts [i].color;
				alpha.a += Time.deltaTime*mult*3;
				otherHologramParts [i].color = alpha;
			}
			yield return null;
		}
		hologramOccupied = false;
	}
	//if the door is unlocked with a pass device or other way, change the locked colors in the hologram for the original unlocked colors
	IEnumerator setHologramColors(List<Color> newColors){
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			for (i = 0; i < hologramParts.Count; i++) {
				hologramParts [i].color = Color.Lerp (hologramParts [i].color, newColors[i], t);
			}
			yield return null;
		}
	}
	//the door was locked and now it has been unlocked, to change the hologram colors
	public void unlockHologram(){
		doorLocked = false;
		regularStateText = unlockedText;
		setHologramText (regularStateText);
		StartCoroutine (setHologramColors (originalColors));
	}
	//the door was locked and now it has been unlocked, to change the hologram colors
	public void lockHologram(){
		doorLocked = true;
		regularStateText = lockedText;
		setHologramText (regularStateText);
		StartCoroutine (setHologramColors (lockedColors));
	}
	//wait a delay and then open the door
	IEnumerator openDoor(){
		yield return new WaitForSeconds (openDelay);
		doorManager.changeDoorsStateByButton ();
		openingDoor = true;
	}
	//chane the current text showed in the door, according to it is locked, unlocked or can be opened
	void setHologramText(string newState){
		for (i = 0; i < hologramText.Count; i++) {
			hologramText [i].text = newState;
		}
	}
	void OnTriggerEnter(Collider col){
		//if the player is inside the hologram trigger
		if(col.GetComponent<Collider>().tag == "Player"){
			//if the door is unlocked, set the open? text in the hologram
			if (!doorLocked) {
				setHologramText (openText);
			}
			inside = true;
			//set an audio when the player enters in the hologram trigger
			if (!openingDoor && doorManager.doorState == doorSystem.doorCurrentState.closed && !doorManager.moving) {
				if (doorLocked) {
					audioSource.PlayOneShot (lockedSound);
				} else {
					audioSource.PlayOneShot (enterSound);
				}
			}
		}
	}
	void OnTriggerExit(Collider col){
		//if the player exits the hologram trigger
		if(col.GetComponent<Collider>().tag == "Player"){
			//set the current state text in the hologram
			setHologramText (regularStateText);
			inside = false;
			//stop the central ring animation and play it reverse and start the rotating animation again
			if (insidePlayed) {
				for (i = 0; i < hologramCentralRing.Count; i++) {
					hologramCentralRing[i].GetComponent<Animation> ()[hologramInside].speed = -1; 
					hologramCentralRing[i].GetComponent<Animation> ()[hologramInside].time=hologramCentralRing[i].GetComponent<Animation> ()[hologramInside].length;
					hologramCentralRing[i].GetComponent<Animation> ().Play(hologramInside);
				}
				for (i = 0; i < hologramsAnimations.Count; i++) {
					hologramsAnimations[i][hologramIdle].time=hologramsAnimations[i][hologramIdle].length;
					hologramsAnimations [i].Play (hologramIdle);
				}
				insidePlayed = false;
			}
			if (!openingDoor) {
				audioSource.PlayOneShot (exitSound);
			}
		}
	}
}
