using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class vehicleHUDManager : MonoBehaviour {
	public float healthAmount;
	public float boostAmount;
	public float regenerateHealthSpeed;
	public float regenerateBoostSpeed;
	public float boostUseRate;
	public bool invincible;
	public bool dead;
	public AudioClip destroyedSound;
	public AudioSource destroyedSource;
	public LayerMask layer;
	public float leftGetOffDistance;
	public float rightGetOffDistance;
	public float getOffHeight;
	public float getOffForward;
	public getOffSide getOffPlace;
	public GameObject damageParticles;
	public GameObject destroyedParticles;
	public float healthPercentageDamageParticles;
	public float extraGrabDistance;
	public Transform placeToShoot;
	public float timeToFadePieces=3;
	public advancedSettingsClass advancedSettings= new advancedSettingsClass();
	[Range(1,100)] public float damageMultiplierOnCollision = 1;
	public bool useWeakSpots;
	[HideInInspector] public bool driving;
	[HideInInspector] public Slider vehicleHealth;
	[HideInInspector] public Slider vehicleBoost;
	[HideInInspector] public Slider vehicleAmmo;
	[HideInInspector] public IKDrivingSystem IKDrivingManager;
	[HideInInspector] public float auxHealthAmount;
	[HideInInspector] public float auxPowerAmount;
	public enum getOffSide{
		left,right
	};
	List<Material> rendererParts=new List<Material>();
	List<GameObject> projectilesReceived=new List<GameObject>();
	List<ParticleSystem> fireParticles=new List<ParticleSystem>();
	float lastDamageTime  = 0;
	[HideInInspector] public float maxhealthAmount;
	float lastBoostTime  = 0;
	float maxBoostAmount;
	bool vehicleDisabled;
	Text ammoAmountText;
	Text weaponNameText;
	Text currentSpeed;
	vehicleWeaponSystem weaponsManager;
	Rigidbody mainRigidbody;
	damageInScreen damageInScreenManager;
	inputActionManager actionManager;
	mapObjectInformation mapInformationManager;
	vehicleCameraController vehicleCameraManager;

	void Start () {
		//get the max amount of health and boost
		maxhealthAmount = healthAmount;
		maxBoostAmount = boostAmount;
		//get the ik driving manager component of the parent
		IKDrivingManager = transform.parent.GetComponent<IKDrivingSystem> ();
		//check if the vehicle has a weapon system
		if (GetComponent<vehicleWeaponSystem> ()) {
			if (GetComponentInChildren<vehicleWeaponSystem> ().enabled) {
				weaponsManager = GetComponent<vehicleWeaponSystem> ();
			}
		}
		//get all the damage receivers in the vehicle
		Component[] damageReceivers=GetComponentsInChildren(typeof(vehicleDamageReceiver));
		foreach (Component c in damageReceivers)	{
			c.GetComponent<vehicleDamageReceiver>().vehicle = gameObject;
		}
		//like in the player, store the max amount of health and boost in two auxiliars varaibles, used by the pick ups to check if the vehicle uses one or more of them
		auxPowerAmount = maxBoostAmount;
		auxHealthAmount = maxhealthAmount;
		//get the damage particles of the vehicle
		if (damageParticles) {
			Component[] fireParticlesComponents=damageParticles.GetComponentsInChildren(typeof(ParticleSystem));
			foreach (Component c in fireParticlesComponents)	{
				fireParticles.Add(c.GetComponent<ParticleSystem>());
				c.gameObject.SetActive (false);
			}
		}
		mainRigidbody = GetComponent<Rigidbody> ();
		damageInScreenManager = GetComponent<damageInScreen> ();
		mapInformationManager = GetComponent<mapObjectInformation> ();
		vehicleCameraManager = transform.parent.GetComponentInChildren<vehicleCameraController> ();
	}

	void Update () {
		//get the current values of health and boost of the vehicle, checking if they are regenerative or not
		healthAmount=manageBarInfo (vehicleHealth, healthAmount, maxhealthAmount, lastDamageTime, regenerateHealthSpeed);
		boostAmount=manageBarInfo (vehicleBoost, boostAmount, maxBoostAmount, lastBoostTime, regenerateBoostSpeed);
		//clear the list which contains the projectiles received by the vehicle
		if (Time.time > lastDamageTime + 3) {
			projectilesReceived.Clear ();
		}
		//if the vehicle is being driving set the health and boost values in the HUD
		if (driving) {
			vehicleHealth.value = healthAmount;
			vehicleBoost.value = boostAmount;
			if (actionManager.getActionInput ("Show Controls Menu")) {
				IKDrivingManager.openOrCloseControlsMenu (!IKDrivingManager.controlsMenuOpened);
			}
		}
		//if the vehicle is destroyed, when destroyed time reachs 0, all the renderer parts of the vehicle are vanished, setting their alpha color value to 0
		if (dead && !vehicleDisabled) {
			if (timeToFadePieces > 0) {
				timeToFadePieces -= Time.deltaTime;
			}
			if (timeToFadePieces <= 0) {
				int piecesAmountFade = 0;
				for (int i = 0; i < rendererParts.Count; i++) {
					Color alpha = rendererParts [i].color;
					alpha.a -= Time.deltaTime / 5;
					rendererParts [i].color = alpha;
					if (alpha.a <= 0) {
						piecesAmountFade++;
					}
				}
				if (piecesAmountFade == rendererParts.Count) {
					IKDrivingManager.destroyVehicle ();
					vehicleDisabled = true;
					return;
				}
			}
		}
		//just a button to destroy the vehicle, used for test
		if (Input.GetKeyDown (KeyCode.Y) && driving) {
			setDamage (healthAmount, -transform.forward, transform.position, gameObject,gameObject,false);
		}
	}
	//function called when the player press the use device button
	public void activateDevice(){
		Vector3 nextPlayerPos = Vector3.zero;
		//if the player was driving
		if (driving) {
			bool canGetOffRight = false;
			bool canGetOffLeft = false;
			RaycastHit[] hits;
			Ray ray=new Ray();
			//if the current option is get off at the left side of the vehicle, then
			if (getOffPlace == getOffSide.left) {
				//set the ray origin at the vehicle position with a little offset set in the inspector
				ray.origin = transform.position + transform.up * getOffHeight + transform.forward * getOffForward;
				//set the ray direction to the left
				ray.direction = -transform.right;
				//get all the colliders in that direction where the yellow sphere is placed
				hits = Physics.SphereCastAll (ray, 0.1f, leftGetOffDistance,layer);
				//get the position where the player will be place
				nextPlayerPos = transform.position - transform.right * leftGetOffDistance;
				if (hits.Length == 0) {
					//any obstacle detected, so the player can get off
					canGetOffLeft = true;
				}
				//some obstacles found
				for (int i = 0; i < hits.Length; i++) {
					//check the distance to that obstacles, if they are lower that the leftGetOffDistance, the player can get off
					if (hits [i].distance > leftGetOffDistance) {
						canGetOffLeft = true;
					}
				}
				//if the left side is blocked, then check the right side in the same way that previously
				if (!canGetOffLeft) {
					ray.direction = transform.right;
					hits = Physics.SphereCastAll (ray, 0.1f, rightGetOffDistance,layer);
					nextPlayerPos = transform.position + transform.right * rightGetOffDistance;
					if (hits.Length == 0) {
						canGetOffRight = true;
					}
					for (int i = 0; i < hits.Length; i++) {
						if (hits [i].distance > rightGetOffDistance) {
							canGetOffRight = true;
						}
					}
				}
			} 
			//else, the default side is the right, so check like in the other mode, but first check the right side, and the left in case the right one is blocked
			else {
				ray.origin = transform.position + transform.up * getOffHeight + transform.forward * getOffForward;
				ray.direction = transform.right;
				hits = Physics.SphereCastAll (ray, 0.1f, rightGetOffDistance,layer);
				nextPlayerPos = transform.position + transform.right * rightGetOffDistance;
				if (hits.Length == 0) {
					canGetOffRight = true;
				}
				for (int i = 0; i < hits.Length; i++) {
					if (hits [i].distance > rightGetOffDistance) {
						canGetOffRight = true;
					}
				}
				if (!canGetOffRight) {
					ray.direction = -transform.right;
					hits = Physics.SphereCastAll (ray, 0.1f, leftGetOffDistance,layer);
					nextPlayerPos = transform.position - transform.right * leftGetOffDistance;
					if (hits.Length == 0) {
						canGetOffLeft = true;
					}
					for (int i = 0; i < hits.Length; i++) {
						if (hits [i].distance > leftGetOffDistance) {
							canGetOffLeft = true;
						}
					}
				}
			}
			//if both sides are blocked, exit the function and the player can't get off
			if (!canGetOffRight && !canGetOffLeft) {
				return;	
			}
			//if any side is avaliable then check a ray in down direction, to place the player above the ground
			RaycastHit hit;
			if (Physics.Raycast (nextPlayerPos + transform.up * getOffHeight + transform.forward * getOffForward, -transform.up, out hit, Mathf.Infinity, layer)) {
				Debug.DrawRay (nextPlayerPos + transform.up * getOffHeight + transform.forward * getOffForward, -transform.up*hit.distance, Color.yellow);
				//also, checks that the distance is lower that a certain height, so for example if the car is in the air, the player is place at the side of the vehicle, instead of the ground
				if (hit.distance <= 3) {
					nextPlayerPos = hit.point;
				}
			}
		}
		//change the driving value
		driving = !driving;
		//send the message to the vehicle movement component, like car controller or motorbike controller
		SendMessage ("changeVehicleState", nextPlayerPos);
	}
	//the player has used a pickup while he is driving, so the health is added in the vehicle
	public void getHealth(float amount){
		//increase the health amount
		healthAmount += amount;
		//check that the current health is not higher than the max value
		if (healthAmount >= vehicleHealth.maxValue) {
			healthAmount = vehicleHealth.maxValue;
		}
		//set the value in the slider of the HUD
		vehicleHealth.value = healthAmount;
		auxHealthAmount = healthAmount;
		//check the current health amount to stop or reduce the damage particles
		changeDamageParticlesValue (false,amount);
		if (damageInScreenManager) {
			damageInScreenManager.showScreenInfo (amount,false,Vector3.zero);
		}
	}
	//the player has used a pickup while he is driving, so the boost is added in the vehicle
	public void getEnergy(float amount){
		//increase the boost amount
		boostAmount += amount;
		//check that the current boost is not higher than the max value
		if (boostAmount >= vehicleBoost.maxValue) {
			boostAmount = vehicleBoost.maxValue;
		}
		//set the value in the slider of the HUD
		vehicleBoost.value = boostAmount;
		auxPowerAmount = boostAmount;
	}
	//the player has used a pickup while he is driving, so the ammo is added in the vehicle
	public void getAmmo(string ammoName, int amount){
		if (weaponsManager) {
			weaponsManager.getAmmo (ammoName, amount);
		}
	}
	//get the value of the current speed in the vehicle
	public void getSpeed(float speed, float maxSpeed){
		currentSpeed.text = speed.ToString ("0") + " / " + maxSpeed.ToString ();
	}
	//if the health or the boost are regenerative, increase the values according to the last time damaged or used
	public float manageBarInfo(Slider bar,float barAmount,float maxAmount,float lastTime,float regenerateSpeed){
		if (regenerateSpeed > 0 && barAmount<maxAmount && !dead) {
			if (Time.time > lastTime + 3) {
				barAmount += regenerateSpeed*Time.deltaTime;
				if (barAmount >= maxAmount) {
					barAmount = maxAmount;
				}
			}
		}
		return barAmount;
	}
	//use the boost in the vehicle, checking the current amount of energy in it
	public bool useBoost(bool moving){
		bool canBeUsed = false;
		//the vehicle is moving so 
		if (moving) {
			if (boostAmount > 0) {
				//reduce the boost amount and return a true value
				boostAmount -= Time.deltaTime * boostUseRate;
				vehicleBoost.value = boostAmount;
				auxPowerAmount = boostAmount;
				canBeUsed = true;
				lastBoostTime = Time.time;
			}
		}
		return canBeUsed;
	}
	//when the current weapon is changed for another, get the current name, ammo per clip and clip size of that weapon
	public void setWeaponName(string name,int ammoPerClip,int clipSize){
		weaponNameText.text = name;
		vehicleAmmo.maxValue = ammoPerClip;
		vehicleAmmo.value = clipSize;
	}
	//the player is shooting while he is driving, so use ammo of the vehicle weapon
	public void useAmmo(int clipSize,string remainAmmo){
		ammoAmountText.text = clipSize.ToString () + "/" + remainAmmo;
		vehicleAmmo.value = clipSize;
	}
	//the vehicle is receiving damage, getting the current damage amount, the direction of the projectile, its hit position, the object that fired it and if the damage is applied only 
	//one time, like a bullet, or constantly like a laser
	public void setDamage (float amount, Vector3 fromDirection, Vector3 damagePos,GameObject bulletOwner, GameObject projectile,bool damageConstant) {
		if (!damageConstant) {
			//if the projectile is not a laser, store it in a list
			//this is done like this because you can add as many colliders (box or mesh) as you want (according to the vehicle meshes), 
			//which are used to check the damage received by every vehicle, so like this the damage detection is really accurated. 
			//For example, if you shoot a grenade to a car, every collider will receive the explosion, but the vehicle will only be damaged once, with the correct amount.
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
		//if the object is not dead, invincible or its health is zero, exit
		if (invincible || dead || amount <= 0) {
			return;
		}
		if (vehicleCameraManager.shakeSettings.useDamageShake && driving) {
			vehicleCameraManager.setDamageCameraShake ();
		}
		if (useWeakSpots) {
			int weakSpotIndex = getClosesWeakSpotIndex (damagePos);
			if (advancedSettings.damageReceiverList [weakSpotIndex].killedWithOneShoot) {
				if (advancedSettings.damageReceiverList [weakSpotIndex].needMinValueToBeKilled) {
					if (advancedSettings.damageReceiverList [weakSpotIndex].minValueToBeKilled < amount) {
						amount = healthAmount;
					}
				} else {
					amount = healthAmount;
				}
			}
		}
		if (amount > healthAmount) {
			amount = healthAmount;
		}
		//decrease the health amount
		healthAmount -= amount;
		auxHealthAmount = healthAmount;
		//if the player is driving this vehicle, set the value in the slider
		if (driving) {
			vehicleHealth.value = healthAmount;
		}
		if (damageInScreenManager) {
			damageInScreenManager.showScreenInfo (amount,true,fromDirection);
		}
		//increase the damage particles values
		changeDamageParticlesValue (true,amount);
		//set the last time damage
		lastDamageTime = Time.time;
		//if the health reachs 0, call the dead function
		if (healthAmount <= 0) {
			healthAmount = 0;
			dead = true;
			destroyVehicle (damagePos);
		}
	}
	public int getClosesWeakSpotIndex(Vector3 collisionPosition){
		float distance = Mathf.Infinity;
		int index = -1;
		for (int i = 0; i < advancedSettings.damageReceiverList.Count; i++) {
			float currentDistance = Vector3.Distance (collisionPosition, advancedSettings.damageReceiverList [i].spotTransform.position);
			if (currentDistance < distance) {
				distance = currentDistance;
				index = i;
			}
		}
		if (index > -1){
			if (advancedSettings.showGizmo) {
				print (advancedSettings.damageReceiverList [index].name);
			}

		}
		return index;
	}
	//the vehicle health is 0, so the vehicle is destroyed
	public void destroyVehicle(Vector3 pos){
		//instantiated an explosiotn particles
		GameObject destroyedParticlesClone = (GameObject)Instantiate (destroyedParticles, transform.position, transform.rotation);
		destroyedParticlesClone.transform.SetParent (transform);
		destroyedSource.PlayOneShot (destroyedSound);
		//set the velocity of the vehicle to zero
		mainRigidbody.velocity = Vector3.zero;
		mainRigidbody.isKinematic = true;
		//get every renderer component if the car
		Component[] components=GetComponentsInChildren(typeof(MeshRenderer));
		foreach (Component c in components)	{
			//check that the current renderer is not the player or any object inside him
			if (c.gameObject!=IKDrivingManager.player && !c.gameObject.transform.IsChildOf(IKDrivingManager.player.transform)) {
				if (c.GetComponent<Renderer> () && c.gameObject.layer != LayerMask.NameToLayer ("Scanner")) {
					if (c.GetComponent<Renderer> ().enabled) {
						//for every renderer object, change every shader in it for a transparent shader 
						for (int j = 0; j < c.gameObject.GetComponent<MeshRenderer> ().materials.Length; j++) {
							c.GetComponent<Renderer> ().materials [j].shader = Shader.Find ("Legacy Shaders/Transparent/Diffuse");
							rendererParts.Add (c.GetComponent<MeshRenderer> ().materials [j]);
						}
						//set the layer ignore raycast to them
						c.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
						//add rigidbody and box collider to them
						if (!c.gameObject.GetComponent<Rigidbody> ()) {
							c.gameObject.AddComponent<Rigidbody> ();
						}
						if(!c.gameObject.GetComponent<BoxCollider> ()){
							c.gameObject.AddComponent<BoxCollider> ();
						}
						//apply explosion force
						c.gameObject.GetComponent<Rigidbody> ().AddExplosionForce (500, pos, 50, 3);
						//ignore collisions with the player
						Physics.IgnoreCollision (IKDrivingManager.player.GetComponent<Collider> (), c.gameObject.GetComponent<Collider> ());
					}
				} 
			}
		}
		//any other object with a collider but with out renderer, is disabled
		Component[] colliders = GetComponentsInChildren (typeof(Collider));
		foreach (Component c in colliders) {
			if (c.gameObject != IKDrivingManager.player && !c.gameObject.transform.IsChildOf (IKDrivingManager.player.transform)) {
				if (c.gameObject.GetComponent<Collider> () && !c.GetComponent<Renderer> ()) {
					c.gameObject.GetComponent<Collider> ().enabled = false;
				}
			}
		}
		//stop the IK system in the player
		IKDrivingManager.disableVehicle ();
		if (mapInformationManager) {
			mapInformationManager.removeMapObject ();
		}
	}
	//this function is called when the vehicle receives damage, to enable a fire and smoke particles system to show serious damage in the vehicle
	void changeDamageParticlesValue(bool damaging,float amount){
		//if the vehicle has a damage particles object
		if (damageParticles) {
			bool activate = false;
			bool activeLoop;
			//if the health is 0, disable the damage particles
			if (healthAmount <= 0) {
				for (int i = 0; i < fireParticles.Count; i++) {
					fireParticles [i].gameObject.SetActive (false);
				}
				return;
			}
			//if the current vehicle health is lower than certain %, the damage particles are enabled 
			bool lowHealth = false;
			if (healthAmount <= maxhealthAmount / (100/healthPercentageDamageParticles)) {
				activate = true;
				activeLoop = true;
				lowHealth = true;
			} else {
				activeLoop = false;
			}
			for (int i = 0; i <fireParticles.Count; i++) {
				//enable the particles
				if (activate) {
					if (!fireParticles [i].isPlaying) {
						fireParticles [i].Play ();
					}
					fireParticles [i].gameObject.SetActive (true);
				}
				//enable or disable their loop, if the particles are enabled, and the health is higher that the above %, then the particles loop is disabled, because the car 
				//has a better health
				if (activeLoop) {
					fireParticles [i].loop = true;
				} else {
					fireParticles [i].loop = false;
				}
				//if the health Percentage Damage Particles is reached, then increase or decrease its size according to if the vehicle is being damaged or receiving health
				if (lowHealth) {
					if (damaging) { 
						fireParticles [i].startSize += amount*0.05f;
					} else {
						fireParticles [i].startSize -= amount*0.05f;
					}
				}
			}
		}
	}
	//when the player gets on the vehicle, the IK driving system sends every slider and text component of the vehicles HUD, to update every value and show them to the player
	public void getHUDBars(Slider health, Slider boost, Slider ammo, Text weaponName,Text ammoInfo,GameObject ammoContent, Text speed){
		if (driving) {
			vehicleHealth = health;
			vehicleBoost = boost;
			vehicleAmmo = ammo;
			vehicleHealth.value = healthAmount;
			vehicleBoost.value = boostAmount;
			ammoAmountText = ammoInfo;
			weaponNameText = weaponName;
			currentSpeed = speed;
		}
		//check also if the vehicle has a weapon system attached, to enable or disable it
		//if the vehicle has not a weapon system, the weapon info of the HUD is disabled
		if (weaponsManager) {
			weaponsManager.changeWeaponState (driving);
			ammoContent.SetActive (true);
		} else {
			ammoContent.SetActive (false);
		}
	}
	//use a jump platform
	public void useJumpPlatform(Vector3 direction){
		SendMessage("useVehicleJumpPlatform",direction, SendMessageOptions.DontRequireReceiver);
	}
	public void useJumpPlatformWithKeyButton(bool state, float newJumpPower){
		if (state) {
			SendMessage("setNewJumpPower",newJumpPower, SendMessageOptions.DontRequireReceiver);
		} else {
			SendMessage("setOriginalJumpPower", SendMessageOptions.DontRequireReceiver);
		}
	}
	public void getAllDamageReceivers(){
		advancedSettings.damageReceiverList.Clear ();
		//get all the damage receivers in the vehicle
		Component[] damageReceivers=GetComponentsInChildren(typeof(vehicleDamageReceiver));
		foreach (Component c in damageReceivers)	{
			damageReceiverInfo newInfo = new damageReceiverInfo ();
			newInfo.name = "Spot " + (advancedSettings.damageReceiverList.Count+1).ToString ();
			newInfo.spotTransform = c.gameObject.transform;
			newInfo.damageMultiplier = c.GetComponent<vehicleDamageReceiver> ().damageMultiplier;
			advancedSettings.damageReceiverList.Add (newInfo);
		}
	}
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		if (advancedSettings.showGizmo && !Application.isPlaying) {
			//draw two spheres at both sides of the vehicles, to see where are launched two raycast to  
			//check if that side is not blocking by an object, so the player will get off in the other side, 
			//checking in the same way, so if both sides are blocked, the player won't get off
			//if there is not any obstacle, another ray is used to check the distance to the ground, so the player is placed at the side of the vehicle
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position + transform.right * rightGetOffDistance + transform.up * getOffHeight + transform.forward * getOffForward, 0.1f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position - transform.right * leftGetOffDistance + transform.up * getOffHeight + transform.forward * getOffForward, 0.1f);
			for (int i = 0; i < advancedSettings.damageReceiverList.Count; i++) {
				if (advancedSettings.damageReceiverList[i].spotTransform) {
					float rValue = 0;
					float gValue = 0;
					float bValue = 0;
					if (!advancedSettings.damageReceiverList [i].killedWithOneShoot) {
						rValue = advancedSettings.damageReceiverList [i].damageMultiplier / 10;
					} else {
						rValue = 1;
						gValue = 1;
					}
					Color gizmoColor= new Vector4(rValue,gValue,bValue, advancedSettings.alphaColor);
					Gizmos.color =  gizmoColor;
					Gizmos.DrawSphere (advancedSettings.damageReceiverList [i].spotTransform.position, advancedSettings.gizmoRadius);
					advancedSettings.damageReceiverList [i].spotTransform.GetComponent<vehicleDamageReceiver> ().damageMultiplier = advancedSettings.damageReceiverList [i].damageMultiplier;
				}
			}
		}
	}	
	public void updateDamageReceivers(){
		for (int i = 0; i < advancedSettings.damageReceiverList.Count; i++) {
			if (advancedSettings.damageReceiverList [i].spotTransform) {
				#if UNITY_EDITOR
				EditorUtility.SetDirty (advancedSettings.damageReceiverList [i].spotTransform.GetComponent<vehicleDamageReceiver> ());
				#endif
			}
		}
	}
	//get the input manager component
	public void getInputActionManager(inputActionManager manager){
		actionManager = manager;
	}
	[System.Serializable]
	public class advancedSettingsClass{
		public List<damageReceiverInfo> damageReceiverList = new List<damageReceiverInfo>();
		public bool showGizmo;
		public Color gizmoLabelColor;
		[Range(0,1)] public float alphaColor;
		[Range(0,1)] public float gizmoRadius;
	}
	[System.Serializable]
	public class damageReceiverInfo{
		public string name;
		public Transform spotTransform;
		[Range(1,10)] public float damageMultiplier;
		public bool killedWithOneShoot;
		public bool needMinValueToBeKilled;
		public float minValueToBeKilled;
	}
}