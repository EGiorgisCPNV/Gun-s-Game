using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class damageScreenSystem : MonoBehaviour {
	public bool damageScreenEnabled;
	public GameObject damageScreen;
	public GameObject damageDirectionIcon;
	public GameObject damagePositionIcon;
	public Color damageColor;
	public Color damageDirectionColor;
	public Color damagePositionColor;
	public float maxAlphaDamage=0.6f;
	public float fadeToDamageColorSpeed;
	public float fadeToTransparentSpeed;
	public float timeToStartToHeal;
	public bool showDamageDirection;
	public bool showDamagePositionWhenEnemyVisible;
	public bool showAllDamageDirections;
	public List<damageInfo> enemiesDamageList = new List<damageInfo> ();	
	bool wounding;
	bool healWounds;
	RawImage damageImage;
	otherPowers powersManager;
	int i,j;

	void Start () {
		powersManager = GetComponent<otherPowers> ();
		//set the size of the damage screen, being the same size that the screen
		damageScreen.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 2, Screen.height * 2);
		damageImage = damageScreen.GetComponent<RawImage> ();
		damageImage.color = damageColor;
		damageDirectionIcon.GetComponentInChildren<RawImage> ().color = damageDirectionColor;
		damagePositionIcon.GetComponent<RawImage> ().color = damagePositionColor;
	}
	void Update () {
		if (damageScreenEnabled) {
			//if the player is wounded, then activate the icon that aims to the enemy position, so the player can see the origin of the damage
			//also, the screen color changes to red, setting the alpha value of a panel in the hud
			if (wounding) {
				Color alpha = damageImage.color;
				if (alpha.a < maxAlphaDamage) {
					float alphaValue = 1 - powersManager.settings.healthBar.value / powersManager.settings.healthBar.maxValue;
					alpha.a = Mathf.Lerp(alpha.a,alphaValue,Time.deltaTime*fadeToDamageColorSpeed);
				} else {
					alpha.a = maxAlphaDamage;
				}
				damageImage.color = alpha;
				if (showDamageDirection) {
					for (i = 0; i < enemiesDamageList.Count; i++) {
						if (enemiesDamageList [i].enemy != gameObject) {
							//get the target position from global to local in the screen
							Vector3 screenPoint = Camera.main.WorldToScreenPoint (enemiesDamageList [i].enemy.transform.position);
							//if the target is visible in the screen, disable the arrow
							if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
								if (enemiesDamageList [i].damageDirection.activeSelf) {
									enemiesDamageList [i].damageDirection.SetActive (false);
								}
								if (showDamagePositionWhenEnemyVisible) {
									if (!enemiesDamageList [i].damagePosition.activeSelf) {
										enemiesDamageList [i].damagePosition.SetActive (true);
									}
									enemiesDamageList [i].damagePosition.transform.position = screenPoint;
								}
							} 
						//if the target is off screen, rotate the arrow to the target direction
						else {
								if (!enemiesDamageList [i].damageDirection.activeSelf) {
									enemiesDamageList [i].damageDirection.SetActive (true);
									enemiesDamageList [i].damagePosition.SetActive (false);
								}
								if (screenPoint.z < 0) {
									screenPoint *= -1;
								}
								Vector3 screenCenter = new Vector3 (Screen.width, Screen.height, 0) / 2;
								screenPoint -= screenCenter;
								float angle = Mathf.Atan2 (screenPoint.y, screenPoint.x);
								angle -= 90 * Mathf.Deg2Rad;
								enemiesDamageList [i].damageDirection.transform.rotation = Quaternion.Euler (0, 0, angle * Mathf.Rad2Deg);
							}
						}
						//if the player is not damaged for a while, disable the arrow
						if (Time.time > enemiesDamageList [i].woundTime + timeToStartToHeal) {
							Destroy (enemiesDamageList [i].damageDirection);
							Destroy (enemiesDamageList [i].damagePosition);
							enemiesDamageList.RemoveAt (i);
						}
					}
				}
			} 
			if (wounding && enemiesDamageList.Count == 0) {
				healWounds = true;
				wounding = false;
			}
			//if the player is not reciving damage for a while, then set alpha of the red color of the background to 0
			if (healWounds || (wounding && enemiesDamageList.Count==0)) {
				Color alpha = damageImage.color;
				alpha.a -= Time.deltaTime * fadeToTransparentSpeed;
				damageImage.color = alpha;
				if (alpha.a <= 0) {
					damageScreen.SetActive (false);
					healWounds = false;
				}
			}
		}
	}
	//set the direction of the damage arrow to see the enemy that injured the player
	public void setDamageDir(GameObject enemy){
		if (showAllDamageDirections) {
			bool enemyFound = false;
			int index = -1;
			for (j = 0; j < enemiesDamageList.Count; j++) {
				if (enemiesDamageList [j].enemy == enemy) {
					index = j;
					enemyFound = true;
				}
			}
			if (!enemyFound) {
				damageInfo newEnemy = new damageInfo ();
				newEnemy.enemy = enemy;
				GameObject newDirection = (GameObject)Instantiate (damageDirectionIcon, Vector3.zero, Quaternion.identity);
				newDirection.transform.SetParent (damageScreen.transform);
				newDirection.transform.localScale = Vector3.one;
				newDirection.transform.localPosition = Vector3.zero;
				newEnemy.damageDirection = newDirection;
				GameObject newPosition = (GameObject)Instantiate (damagePositionIcon, Vector3.zero, Quaternion.identity);
				newPosition.transform.SetParent (damageScreen.transform);
				newPosition.transform.localScale = Vector3.one;
				newEnemy.damagePosition = newPosition;
				newEnemy.woundTime = Time.time;
				enemiesDamageList.Add (newEnemy);
			} else {
				if (index != -1) {
					enemiesDamageList [index].woundTime = Time.time;
				}
			}
		}
		wounding = true;
		damageScreen.SetActive (true);
	}
	[System.Serializable]
	public class damageInfo{
		public GameObject enemy;
		public GameObject damageDirection;
		public GameObject damagePosition;
		public float woundTime;
	}
}