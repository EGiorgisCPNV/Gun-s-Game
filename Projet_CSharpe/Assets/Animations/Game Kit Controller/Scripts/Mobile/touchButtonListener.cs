using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using System;
[System.Serializable]
//I made this script to use touch buttons with the UI of unity 4.6, instead of the event triggers, to check if the button is being pressing, holding or released
public class touchButtonListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler{
	public bool pressedDown = false;
	public bool pressedUp = false;
	public bool pressed;
	public bool changeColorOnPress;
	public Color releaseColor;
	public Color pressColor;
	public float colorChangeSpeed;
	Coroutine colorTransition;
	RawImage buttonIcon;

	void Start(){
		if (changeColorOnPress) {
			buttonIcon = GetComponent<RawImage> ();
		}
	}
	//if you press the button
	public void OnPointerDown(PointerEventData eventData){
		pressedDown = true;
		pressed = true;
		if (changeColorOnPress) {
			changeColor (true);
		}
		StartCoroutine (disableDown ());
	}
	//if you release the button
	public void OnPointerUp(PointerEventData eventData){
		pressedUp = true;
		pressed = false;
		if (changeColorOnPress) {
			changeColor (false);
		}
		StartCoroutine (disableUp ());
	}
	//disable the booleans parameters after press them
	IEnumerator disableDown(){
		yield return new WaitForSeconds(0.001f);
		pressedDown = false;
	}
	IEnumerator disableUp(){
		yield return new WaitForSeconds(0.001f);
		pressedUp = false;
	}
	//if the button is disabled, reset the button
	void OnDisable(){
		pressedDown = false;
		pressedUp = false;
		pressed = false;
	}
	void changeColor(bool state){
		if (colorTransition != null) {
			StopCoroutine (colorTransition);
		}
		colorTransition = StartCoroutine (changeColorCoroutine (state));
	}
	IEnumerator changeColorCoroutine(bool state){
		Color targetColor = Color.white;
		if (state) {
			targetColor = pressColor;
		} else {
			targetColor = releaseColor;
		}
		while (buttonIcon.color != targetColor) {
			buttonIcon.color = Color.Lerp (buttonIcon.color, targetColor, Time.deltaTime * colorChangeSpeed);
			yield return null;
		}
	}
}