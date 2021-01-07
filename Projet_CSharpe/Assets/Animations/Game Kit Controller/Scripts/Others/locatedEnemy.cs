using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class locatedEnemy : MonoBehaviour {
	public GameObject target;
	public GameObject offScreenIcon;
	public GameObject onScreenIcon;
	GameObject objectiveIcon;
	
	//this script set an icon for the enemies that are locked when the player using the power to launch homing projectiles
	
	void Update () {
		if (target) {
			//get the target position from global to local in the screen
			Vector3 screenPoint = Camera.main.WorldToScreenPoint (target.transform.position);
			//if the target is visible in the screnn, set the icon position and the distance in the text component
			if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
				//change the icon from offscreen to onscreen
				if(objectiveIcon!=onScreenIcon){
					onScreenIcon.SetActive (true);
					offScreenIcon.SetActive (false);
					objectiveIcon=onScreenIcon;
				}
				objectiveIcon.transform.position = screenPoint;
			} 
			//if the target is off screen, change the icon to follow the target position to the target direction
			else {
				//change the icon from onscreen to offscreen
				if(objectiveIcon!=offScreenIcon){
					onScreenIcon.SetActive(false);
					offScreenIcon.SetActive(true);
					objectiveIcon=offScreenIcon;
				}
				if (screenPoint.z < 0) {
					screenPoint *= -1;
				}
				Vector3 screenCenter = new Vector3 (Screen.width, Screen.height, 0) / 2;
				screenPoint -= screenCenter;
				float angle = Mathf.Atan2 (screenPoint.y, screenPoint.x);
				angle -= 90 * Mathf.Deg2Rad;
				float cos = Mathf.Cos (angle);
				float sin = -Mathf.Sin (angle);
				float m = cos / sin;
				Vector3 screenBounds = screenCenter * 0.9f;
				if (cos > 0) {
					screenPoint = new Vector3 (screenBounds.y / m, screenBounds.y, 0);
				} else {
					screenPoint = new Vector3 (-screenBounds.y / m, -screenBounds.y, 0);
				}
				if (screenPoint.x > screenBounds.x) {
					screenPoint = new Vector3 (screenBounds.x, screenBounds.x * m, 0);
				} else if (screenPoint.x < -screenBounds.x) {
					screenPoint = new Vector3 (-screenBounds.x, -screenBounds.x * m, 0);
				}
				//set the position of the icon
				screenPoint += screenCenter;
				objectiveIcon.transform.position = screenPoint;
			}
		}
	}
	//set the enemy to follow
	public void setTarget(GameObject obj){
		target = obj;
	}
	//if the projectile reachs the target, remove the icon
	public void removeTarget(){
		Destroy (gameObject);
	}
}