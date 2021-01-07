using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class health : MonoBehaviour {
	public float healthAmount = 100;
	public float regenerateSpeed = 0;
	public bool invincible = false;
	public bool dead  = false;
	public GameObject damagePrefab ;
	public Transform placeToShoot;
	public GameObject scorchMarkPrefab  = null;
	public string damageFunction;
	public string deadFuncion;
	public bool useExtraDeadFunctions;
	public List<string> extraDeadFunctionList=new List<string>();
	public enemySettings settings = new enemySettings();
	public advancedSettingsClass advancedSettings = new advancedSettingsClass ();
	List<GameObject> projectilesReceived=new List<GameObject>();
	List<characterDamageReceiver> damageReceiverList = new List<characterDamageReceiver> ();
	bool enemyLocated;
	GameObject scorchMark;
	GameObject slider;
	GameObject hudAndMenus;
	float lastDamageTime  = 0;
	public float maxhealthAmount;
	RaycastHit hit;
	damageInScreen damageInScreenManager;
	Vector3 originalPlaceToShootPosition;
	Camera mainCamera;

	void Start () {
		//get the initial health assigned
		maxhealthAmount = healthAmount;
		if (damagePrefab) {
			//if damage prefab has been assigned, instantiate the damage effect
			GameObject effect = (GameObject)Instantiate (damagePrefab, Vector3.zero, Quaternion.identity);
			effect.transform.SetParent(transform);
			effect.transform.localPosition = Vector3.zero;
			damageEffect = effect.GetComponent<ParticleEmitter>();
		}
		if (scorchMarkPrefab) {
			scorchMarkPrefab.SetActive(false);
		}
		//instantiate a health slider in the UI, used for the enemies and allies
		if (settings.enemyHealthSlider) {
			hudAndMenus = GameObject.Find ("enemySliders");
			slider = (GameObject)Instantiate (settings.enemyHealthSlider, Vector3.zero, Quaternion.identity);
			slider.transform.SetParent (hudAndMenus.transform);
			slider.SetActive (true);
			slider.GetComponent<Slider> ().maxValue = maxhealthAmount;
			slider.GetComponent<Slider> ().value = healthAmount;
			slider.GetComponent<Slider> ().interactable = false;
			slider.transform.localScale = Vector3.one;
			//set the info in the health slider, enemy or ally, changing also the slider color
			if (tag == "enemy") {
				setSliderInfo (settings.enemyName, Color.red);
			} else {
				setSliderInfo (settings.allyName, Color.green);
			}
		}
		//adjust the initial player health amount at the current value
		if (tag == "Player") {
			GetComponent<otherPowers>().settings.healthBar.maxValue=healthAmount;
			GetComponent<otherPowers>().settings.healthBar.value=healthAmount;
		}
		damageInScreenManager = GetComponent<damageInScreen> ();

		//get all the damage receivers in the vehicle
		Component[] damageReceivers=GetComponentsInChildren(typeof(characterDamageReceiver));
		if (damageReceivers.Length > 0) {
			foreach (Component c in damageReceivers) {
				characterDamageReceiver newReceiver = c.GetComponent<characterDamageReceiver> ();
				newReceiver.character = gameObject;
				damageReceiverList.Add (newReceiver);
			}
		} else {
			gameObject.AddComponent <characterDamageReceiver> ().character = gameObject;
		}
		if (placeToShoot) {
			originalPlaceToShootPosition = placeToShoot.transform.localPosition;
		}
		mainCamera = Camera.main;
	}
	void Update(){
		//clear the list which contains the projectiles received by the vehicle
		if (Time.time > lastDamageTime + 3) {
			projectilesReceived.Clear ();
		}
		//if the object can regenerate, add health after a while with no damage
		if (regenerateSpeed > 0 && healthAmount < maxhealthAmount && !dead) {
			if (Time.time > lastDamageTime + 3) {
				healthAmount += regenerateSpeed * Time.deltaTime;
				if (healthAmount >= maxhealthAmount) {
					healthAmount = maxhealthAmount;
				}
			}
		}
		//if the health slider has been created, set its position in the screen, so the slider follows the object position
		//to make the slider visible, the player has to see directly the object
		//also, the slider is disabled, when the object is not visible in the screen
		if (slider) {
			Vector3 screenPoint = mainCamera.WorldToScreenPoint (transform.position + transform.up * settings.sliderOffset);
			if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height && enemyLocated) {
				slider.transform.position = screenPoint;
				//set the direction of the raycast
				Vector3 direction = transform.position + transform.up - Camera.main.transform.position;
				direction = direction / direction.magnitude;
				float distance = Vector3.Distance (transform.position, Camera.main.transform.position);
				bool activeIcon = false;
				//if the raycast find an obstacle between the enemy and the camera, disable the icon
				//if the distance from the camera to the enemy is higher than 100, disable the raycast and the icon
				if (distance < 100) {
					if (Physics.Raycast (transform.position + transform.up, -direction, out hit, distance, settings.layer)) {
						Debug.DrawRay (transform.position + transform.up, -direction * hit.distance, Color.red);
						activeIcon = false;
					}
					//else, the raycast reachs the camera, so enable the pick up icon
					else {
						Debug.DrawRay (transform.position + transform.up, -direction * distance, Color.green);
						activeIcon = true;
					}
				}
				slider.SetActive (activeIcon);
			} else {
				slider.SetActive (false);
			}
			//if the slider is not visible yet, check the camera position
			if (!enemyLocated) {
				//when the player looks at the enemy position, enable his slider health bar
				if (Physics.Raycast (Camera.main.transform.position, mainCamera.transform.forward, out hit, 200, settings.layer)) {
					if (hit.collider.gameObject == gameObject || hit.collider.gameObject.transform.IsChildOf (gameObject.transform)) {
						enemyLocated = true;
					}
				}
			}
		}
	}
	//receive a certain amount of damage
	public void setDamage (float amount, Vector3 fromDirection, Vector3 damagePos,GameObject bulletOwner, GameObject projectile,bool damageConstant) {
		if (!damageConstant) {
			//if the projectile is not a laser, store it in a list
			//this is done like this because you can add as many colliders (box or mesh) as you want (according to the vehicle meshes), 
			//which are used to check the damage received by every character, so like this the damage detection is really accurated. 
			//For example, if you shoot a grenade to a character, every collider will receive the explosion, but the character will only be damaged once, with the correct amount.
			//in this case the projectile has not produced damage yet, so it is stored in the list and in the below code the damage is applied. 
			//This is used for bullets for example, which make damage only in one position
			if (!projectilesReceived.Contains (projectile)) {
				projectilesReceived.Add (projectile);
			} 
			//in this case the projectile has been added to the list previously, it means that the projectile has already applied damage to the vehicle, 
			//so it can't damaged the vehicle twice. This is used for grenades for example, which make a damage inside a radius
			else {
				return;
			}
		}
		//if any elememnt in the list of current projectiles received is not longer exits, remove it from the list
		for (int i = 0; i < projectilesReceived.Count; i++) {
			if (!projectilesReceived [i]) {
				projectilesReceived.RemoveAt (i);
			}
		}
		//if the objects is not dead, invincible or its health is zero, exit
		if (invincible || dead || amount <= 0) {
			return;
		}
		if (gameObject.tag == "Player" && gameObject.GetComponent<playerController> ().driving) {
			return;
		}
		if (advancedSettings.useWeakSpots) {
			int weakSpotIndex = getClosesWeakSpotIndex (damagePos);
			if (advancedSettings.weakSpots [weakSpotIndex].killedWithOneShoot) {
				if (advancedSettings.weakSpots [weakSpotIndex].needMinValueToBeKilled) {
					if (advancedSettings.weakSpots [weakSpotIndex].minValueToBeKilled < amount) {
						amount = healthAmount;
					}
				} else {
					amount = healthAmount;
				}
			}
			if (!advancedSettings.notHuman) {
				amount *= advancedSettings.weakSpots [weakSpotIndex].damageMultiplier;
			}
		}
		if (amount > healthAmount) {
			amount = healthAmount;
		}
		//active the damage prefab, substract the health amount, and set the value in the slider
		healthAmount -= amount;
		if (slider) {
			slider.GetComponent<Slider> ().value = healthAmount;
		}
		if (damageFunction != "") {
			//call a function when the object receives damage
			SendMessage (damageFunction, bulletOwner);
		}
		lastDamageTime = Time.time;
		if (damageEffect) {
			//set the position of the damage in the position where the projectile hitted the object with the health component
			damageEffect.transform.position = damagePos;
			damageEffect.transform.rotation = Quaternion.LookRotation (fromDirection, Vector3.up);
			damageEffect.Emit ();
		}
		if (damageInScreenManager) {
			damageInScreenManager.showScreenInfo (amount, true, fromDirection);
		}
		//if the health reachs 0, call the dead function
		if (healthAmount <= 0) {
			healthAmount = 0;
			if (slider) {
				Destroy (slider);
			}
			dead = true;
			if (deadFuncion != "") {
				//enable the ragdoll of the player
				SendMessage ("deathDirection", -fromDirection, SendMessageOptions.DontRequireReceiver);
				SendMessage (deadFuncion, damagePos);
				if (useExtraDeadFunctions) {
					for (int i = 0; i < extraDeadFunctionList.Count; i++) {
						BroadcastMessage (extraDeadFunctionList [i], SendMessageOptions.DontRequireReceiver);
					}
				}
				if (tag != "Player" && GetComponent<mapObjectInformation> ()) {
					GetComponent<mapObjectInformation> ().removeMapObject ();
				}
			}
			if (scorchMarkPrefab) {
				//if the object is an enemy, set an scorch below the enemy, using a raycast
				scorchMarkPrefab.SetActive (true);
				scorchMarkPrefab.transform.SetParent (null);
				RaycastHit hit;
				if (Physics.Raycast (transform.position, transform.up * (-1), out hit, 200, settings.layer)) {
					if (hit.collider.gameObject.layer != 2) {
						Vector3 scorchPosition = hit.point;
						scorchMarkPrefab.transform.position = scorchPosition + hit.normal * 0.03f;
					}
				}
			}
		} else {
			if (advancedSettings.haveRagdoll) {
				if (amount >= advancedSettings.minDamageToEnableRagdoll) {
					SendMessage (advancedSettings.functionToRagdoll);
				}
			}
		}
	}
	public void getHealth(float amount){
		if (damageInScreenManager) {
			damageInScreenManager.showScreenInfo (amount,false, Vector3.zero);
		}
	}
	//if an enemy becomes an ally, set its name and its slider color
	public void setSliderInfo(string name,Color color){
		slider.transform.GetChild (0).GetComponent<Text> ().text = name;
		GameObject redSlider = slider.transform.GetChild (2).gameObject;
		redSlider.transform.GetChild (0).GetComponent<Image> ().color = color;
	}
	public void hacked(){
		setSliderInfo (settings.allyName, Color.green);
	}
	//restart the health component of the object
	public void resurrect(){
		healthAmount = maxhealthAmount;
		dead = false;
	}
	public int getClosesWeakSpotIndex(Vector3 collisionPosition){
		float distance = Mathf.Infinity;
		int index = -1;
		for (int i = 0; i < advancedSettings.weakSpots.Count; i++) {
			float currentDistance = Vector3.Distance (collisionPosition, advancedSettings.weakSpots [i].spotTransform.position);
			if (currentDistance < distance) {
				distance = currentDistance;
				index = i;
			}
		}
		if (index > -1){
			if (advancedSettings.showGizmo) {
				//print (advancedSettings.weakSpots [index].name);
			}

		}
		return index;
	}
	public float getMaxHealthAmount(){
		return maxhealthAmount;
	}
	public void killByButton(){
		setDamage (healthAmount, transform.forward, transform.position + transform.up * 1.5f, gameObject, gameObject, false);
	}
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		// && !Application.isPlaying
		if (advancedSettings.showGizmo) {
			for (int i = 0; i < advancedSettings.weakSpots.Count; i++) {
				if (advancedSettings.weakSpots [i].spotTransform) {
					float rValue = 0;
					float gValue = 0;
					float bValue = 0;
					if (!advancedSettings.weakSpots [i].killedWithOneShoot) {
						if (advancedSettings.weakSpots [i].damageMultiplier < 1) {
							bValue = advancedSettings.weakSpots [i].damageMultiplier / 0.1f;
						} else {
							rValue = advancedSettings.weakSpots [i].damageMultiplier / 20;
						}
					} else {
						rValue = 1;
						gValue = 1;
					}
					Color gizmoColor = new Vector4 (rValue, gValue, bValue, advancedSettings.alphaColor);
					Gizmos.color = gizmoColor;
					Gizmos.DrawSphere (advancedSettings.weakSpots [i].spotTransform.position, advancedSettings.gizmoRadius);
					if (advancedSettings.notHuman) {
						advancedSettings.weakSpots [i].spotTransform.GetComponent<characterDamageReceiver> ().damageMultiplier = advancedSettings.weakSpots [i].damageMultiplier;
					}
				}
			}
		}
	}
	public void changePlaceToShootPosition(bool state){
		if (state) {
			placeToShoot.transform.localPosition = originalPlaceToShootPosition - placeToShoot.transform.up;
		} else {
			placeToShoot.transform.localPosition = originalPlaceToShootPosition;
		}
	}
	public void updateDamageReceivers(){
		for (int i = 0; i < advancedSettings.weakSpots.Count; i++) {
			if (advancedSettings.weakSpots [i].spotTransform && advancedSettings.weakSpots [i].spotTransform.GetComponent<characterDamageReceiver> ()) {
				#if UNITY_EDITOR
				EditorUtility.SetDirty (advancedSettings.weakSpots [i].spotTransform.GetComponent<characterDamageReceiver> ());
				#endif
			}
		}
	}
	[System.Serializable]
	public class enemySettings{
		public GameObject enemyHealthSlider;
		public float sliderOffset;
		public LayerMask layer;
		public string enemyName;
		public string allyName;
	}
	[System.Serializable]
	public class advancedSettingsClass{
		public bool notHuman;
		public bool useWeakSpots;
		public List<weakSpot> weakSpots=new List<weakSpot>();
		public bool haveRagdoll;
		public float minDamageToEnableRagdoll;
		public string functionToRagdoll;
		public bool showGizmo;
		public Color gizmoLabelColor;
		[Range(0,1)] public float alphaColor;
		[Range(0,1)] public float gizmoRadius;
	}
	[System.Serializable]
	public class weakSpot{
		public string name;
		public Transform spotTransform;
		[Range(0.1f,20)] public float damageMultiplier;
		public bool killedWithOneShoot;
		public bool needMinValueToBeKilled;
		public float minValueToBeKilled;
	}
}