using UnityEngine;
using System.Collections;

public class clock : MonoBehaviour {
	public int minutes = 0;
	public int hours = 0;
	public float speed = 1;
	GameObject needleSeconds;
	GameObject needleMinutes;
	GameObject needleHours;
	float secInterval = 0;
	int seconds = 0;

	//just a script to simulate a clock in the solar system mechanism
	void Start(){
		needleSeconds = transform.Find("seconds").gameObject;
		needleMinutes = transform.Find("minutes").gameObject;
		needleHours = transform.Find("hours").gameObject;
	}

	void Update(){
		secInterval += Time.deltaTime * speed;
		if(secInterval >= 1){
			secInterval -= 1;
			seconds++;
			if(seconds >= 60){
				seconds = 0;
				minutes++;
				if(minutes > 60){
					minutes = 0;
					hours++;
					if(hours >= 24){
						hours = 0;
					}
				}
			}
		}
		float rotationSeconds = (360 / 60) * seconds;
		float rotationMinutes = (360 / 60) * minutes;
		float rotationHours = ((360 / 12) * hours) + ((360 / (60 * 12)) * minutes);
		needleSeconds.transform.localEulerAngles = transform.up * rotationSeconds;
		needleMinutes.transform.localEulerAngles = transform.up * rotationMinutes;
		needleHours.transform.localEulerAngles = transform.up * rotationHours;
	}
}