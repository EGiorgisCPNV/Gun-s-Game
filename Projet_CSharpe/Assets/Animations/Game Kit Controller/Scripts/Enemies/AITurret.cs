using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AITurret : MonoBehaviour {
	public LayerMask layer;
	public LayerMask layerForGravity;
	public bool onSpotted;
	public GameObject bullet;
	public GameObject bulletShell;
	public weaponType currentWeapon;
	public float rotationSpeed = 10;
	public float laserDamage=0.2f;
	public float machineGunDamage=1;
	public float cannonDamage=1;
	public tagsToShoot tagToShoot;
	public Shader transparent;
	public AudioClip locatedEnemy;
	public AudioClip machineGunSound;
	public AudioClip laserSound;
	public AudioClip cannonSound;
	public bool randomWeaponAtStart;
	[HideInInspector] public bool dead;
	GameObject cannonShellPosition;
	GameObject machineGunShellPosition;
	GameObject laserShootPosition;
	GameObject cannonShootPosition;
	GameObject machineGunShootPosition;
	GameObject laserBeam;
	GameObject rayCastPosition;
	GameObject shootPosition;
	GameObject enemyToShoot;
	GameObject shellPosition;
	GameObject rotateCylinder1;
	GameObject fieldOfView;
	GameObject posibleThreat;
	GameObject head;
	GameObject rotor;
	GameObject machineGun;
	GameObject cannon;
	GameObject aimMachineGun;
	GameObject aimCannon;
	GameObject viewTrigger;
	GameObject hackDevice;
	RaycastHit hit;
	int typeWeaponChoosed;
	List<GameObject> bulletShells = new List<GameObject> ();
	List<GameObject> enemies = new List<GameObject> ();
	List<Renderer> rendererParts=new List<Renderer>();
	bool cannonDeployed;
	bool machineGunDeployed;
	bool checkingThreat;
	bool hacking;
	bool hackFailed;
	bool kinematicActive;
	bool paused;
	float timer;
	float shootTimerLimit;
	float destroyShellsTimer=0;
	float timeToCheck=0;
	float originalFOVRaduis;
	float orignalRotationSpeed;
	float speedMultiplier=1;
	health enemyHealth;
	Rigidbody mainRigidbody;
	Animation machineGunAnim;
	Animation cannonAnim;
	AudioSource audioSource;

	public enum weaponType{
		//type of current weapon that the turret is using, you can change it in run time
		cannon = 0, laser = 1 ,machineGun = 2
	}
	//the tags that the turret will check and take as threat
	public enum tagsToShoot{
		Player, enemy
	}
	void Start () {
		//get all the important parts of the turret, by searching in its children by name
		setTurretParts ();
		machineGunAnim = machineGun.GetComponent<Animation> ();
		cannonAnim = cannon.GetComponent<Animation> ();
		//check if the turret is an enemy or an ally from the beginning of the game
		if (tagToShoot==tagsToShoot.Player) {
			tag = "enemy";
		} else {
			tag="friend";
			hackDevice.SetActive(false);
		}
		//set the selected weapon
		setWeapon ();
		//set the parameters of the turret laser
		laserBeam.GetComponent<enemyLaser> ().bulletOwner=gameObject;
		laserBeam.SetActive(false);
		laserBeam.GetComponent<enemyLaser> ().laserDamage = laserDamage;
		originalFOVRaduis=fieldOfView.GetComponent<SphereCollider> ().radius;
		orignalRotationSpeed = rotationSpeed;
		aimMachineGun.SetActive(false);
		aimCannon.SetActive(false);
		mainRigidbody = GetComponent<Rigidbody> ();
		audioSource = GetComponent<AudioSource> ();
		if (randomWeaponAtStart) {
			setRandomWeapon ();
		}
	}
	void Update () {
		if (dead) {
			//if the turrets is destroyed, set it to transparent smoothly to disable it from the scene
			for (int i=0;i<rendererParts.Count;i++){
				Color alpha =rendererParts[i].material.color;
				alpha.a -=Time.deltaTime/5;
				rendererParts[i].material.color = alpha;
				if(alpha.a<=0){
					Destroy (gameObject);
				}
			}
		}
		//if the turret is not destroyed, or being hacked, or paused by a black hole, then
		if (!hacking && !dead && !paused) {
			//look at the closest enemy
			closestTarget ();
			//if the number of targets is equal or higher than 1
			if (onSpotted) {
				followTarget (enemyHealth.placeToShoot);
				//if the current weapon is the machine gun or the cannon, check with a ray if the player is in front of the turret
				//if the cannon is selected, the time to shoot is 1 second, the machine gun shoots every 0.1 seconds
				if ((typeWeaponChoosed == 0 || typeWeaponChoosed == 2) && !machineGunAnim.IsPlaying ("activateMachineGun") && !cannonAnim.IsPlaying ("activateCannon")) {
					if (Physics.Raycast (rayCastPosition.transform.position, rayCastPosition.transform.forward, out hit, Mathf.Infinity, layer)) {
						Debug.DrawLine (rayCastPosition.transform.position, hit.point, Color.red, 200,true);
						if (hit.collider.gameObject == enemyToShoot || hit.collider.gameObject.transform.IsChildOf(enemyToShoot.transform)) {
							timer += Time.deltaTime*speedMultiplier;
						}
					}
					//check the current weapon selected, to aim it in the direction of the closest enemy
					if (typeWeaponChoosed == 0) {
						Vector3 targetDir= enemyHealth.placeToShoot.position - aimCannon.transform.position;
						Quaternion qTo = Quaternion.LookRotation(targetDir);
						aimCannon.transform.rotation =  Quaternion.Slerp (aimCannon.transform.rotation, qTo, rotationSpeed * Time.deltaTime);
					}
					if (typeWeaponChoosed == 2) {
						Vector3 targetDir= enemyHealth.placeToShoot.position - aimMachineGun.transform.position;
						Quaternion qTo = Quaternion.LookRotation(targetDir);
						aimMachineGun.transform.rotation =  Quaternion.Slerp (aimMachineGun.transform.rotation, qTo, rotationSpeed * Time.deltaTime);
						rotor.transform.Rotate (0, 0, 800 * Time.deltaTime*speedMultiplier);
					}
					//if the timer ends, shoot
					if (timer >= shootTimerLimit) {
						timer = 0;
						destroyShellsTimer = 0;
						//create the projectile in the position of the current weapon
						GameObject bulletClone = (GameObject)Instantiate (bullet, shootPosition.transform.position, shootPosition.transform.rotation);
						enemyBullet enemyBulletClone = bulletClone.GetComponent<enemyBullet> ();
						enemyBulletClone.enemy = enemyToShoot;
						enemyBulletClone.bulletOwner=gameObject;
						enemyBulletClone.speedMultiplier=speedMultiplier;
						//configure the fired projectile
						if (typeWeaponChoosed == 0) {
							enemyBulletClone.damage = cannonDamage;
							enemyBulletClone.missile=true;
							aimCannon.GetComponent<Animation>().Play("cannonRecoil");
							audioSource.Play ();
						} 
						if (typeWeaponChoosed == 2){
							enemyBulletClone.damage = machineGunDamage;
							enemyBulletClone.bullet=true;
							audioSource.Play ();
						}
						//create the shell bullet
						GameObject bulletShellClone = (GameObject)Instantiate (bulletShell, shellPosition.transform.position, shellPosition.transform.rotation);
						bulletShellClone.GetComponent<Rigidbody> ().velocity = shellPosition.transform.forward * 2;  
						bulletShells.Add (bulletShellClone);
						//the shells are removed from the scene
						if (bulletShells.Count > 15) {
							GameObject shellToRemove = bulletShells [0];
							bulletShells.RemoveAt (0);
							Destroy (shellToRemove);
						}
					}
				}
				//if the laser is selected, activate it
				if (typeWeaponChoosed == 1) {
					if (!laserBeam.activeSelf) {
						laserBeam.SetActive(true);
					}
				}
				//if in run time the weapon is changed, set the new weapon
				if (typeWeaponChoosed != (int)currentWeapon) {
					activateWeapon ();
				}
			}
			//if there are no enemies in the field of view, rotate in Y local axis to check new targets
			else if (!checkingThreat) {
				rotateCylinder1.transform.Rotate (0, rotationSpeed * Time.deltaTime*3 , 0);
			}
			//if the turret detects a target, it will check if it is an enemy, and this will take 2 seconds, while the enemy choose to leave or stay in the place
			else if (checkingThreat){
				if(!enemyHealth){
					//every object with a health component, has a place to be shoot, to avoid that a enemy shoots the player in his foot, so to center the shoot
					//it is used the gameObject placetoshoot in the health script
					Component component=posibleThreat.GetComponent<health>();
					//get the position of the enemy to shoot
					enemyHealth=component as health;
					if(enemyHealth.dead){
						cancelCheckSuspect(posibleThreat);
						return;
					}
				}
				//look at the target position
				followTarget (enemyHealth.placeToShoot);
				//Debug.DrawRay (rayCastPosition.transform.position, rayCastPosition.transform.forward, Color.red, 100);
				//uses a raycast to check the posible threat
				if (Physics.Raycast (rayCastPosition.transform.position, rayCastPosition.transform.forward, out hit, Mathf.Infinity, layer)) {
					if (hit.collider.gameObject == posibleThreat || hit.collider.gameObject.transform.IsChildOf(posibleThreat.transform)) {
						timeToCheck += Time.deltaTime*speedMultiplier;
					}
					//when the turret look at the target for a while, it will open fire 
					if (timeToCheck > 2) {
						timer = 1;
						timeToCheck = 0;
						checkingThreat = false;
						addEnemy(posibleThreat);
						posibleThreat=null;
					}
				}
			}
			//disable the machine gun and the cannon renderers while they are not used
			if(!machineGunAnim.IsPlaying ("activateMachineGun") && !cannonAnim.IsPlaying ("activateCannon") && !onSpotted){
				if(aimMachineGun.activeSelf){
					aimMachineGun.SetActive(false);
				}
				if(aimCannon.activeSelf){
					aimCannon.SetActive(false);
				}
			}
		}
		//remove the shells of the bullet when the turret is not shooting
		if (bulletShells.Count > 0) {
			destroyShellsTimer += Time.deltaTime;
			if (destroyShellsTimer > 3) {
				for (int i=0; i<bulletShells.Count; i++) {
					Destroy (bulletShells [i]);
				}
				bulletShells.Clear ();
			}
		}
		//if the turret has been hacked, the player can grab it, so when he drops it, the turret will be set in the first surface that will touch
		//also checking if the gravity of the turret has been modified
		if (tag == "Untagged" && !mainRigidbody.isKinematic && !mainRigidbody.freezeRotation) {
			mainRigidbody.freezeRotation=true;
			StartCoroutine(rotateElement(gameObject));
		}
		if (tag != "Untagged" && mainRigidbody.freezeRotation) {
			mainRigidbody.freezeRotation=false;
			kinematicActive=true;
		}
		//when the kinematicActive has been enabled, the turret has a regular gravity again, so the first ground surface that will find, will be its new ground
		//enabling the kinematic rigidbody of the turret
		if (kinematicActive) {
			if (Physics.Raycast (transform.position, -Vector3.up, out hit, 1.2f, layerForGravity)) {
				if(!mainRigidbody.isKinematic && kinematicActive && !GetComponent<artificialObjectGravity>() && !hit.collider.isTrigger){
					StartCoroutine (rotateToSurface (hit));
				}
			}
		}
	}
	//the gravity of the turret is regular again
	void dropCharacter(bool state){
		kinematicActive = state;
	}
	//when the turret detects a ground surface, will rotate according to the surface normal 
	IEnumerator rotateToSurface(RaycastHit hit){
		//it works like the player gravity
		kinematicActive = false;
		mainRigidbody.useGravity=true;
		mainRigidbody.isKinematic = true;
		Quaternion rot = transform.rotation;
		Vector3 myForward = Vector3.Cross (transform.right, hit.normal);
		Quaternion dstRot = Quaternion.LookRotation (myForward, hit.normal);
		Vector3 pos = hit.point;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3;
			transform.rotation = Quaternion.Slerp (rot, dstRot, t);
			//set also the position of the turret to the hit point
			transform.position = Vector3.MoveTowards (transform.position, pos + transform.up * 0.5f, t);
			yield return null;
		}
		gameObject.layer=0;
	}
	//check if the object which has collided with the viewTrigger (the capsule collider in the head of the turret) is an enemy checking the tag of that object
	void checkSuspect(GameObject col){
		if (((tagToShoot==tagsToShoot.Player && col.gameObject.tag=="friend") || col.gameObject.tag==tagToShoot.ToString()) && !onSpotted && !posibleThreat) {
			posibleThreat=col.gameObject;
			checkingThreat=true;
			hacking = false;
		}
	}
	//in the object exits from the viewTrigger, the turret rotates again to search more enemies
	void cancelCheckSuspect(GameObject col){
		if (((tagToShoot==tagsToShoot.Player && col.gameObject.tag=="friend") || col.gameObject.tag==tagToShoot.ToString()) && !onSpotted && posibleThreat) {
			enemyHealth=null;
			posibleThreat=null;
			timeToCheck=0;
			checkingThreat=false;
			StartCoroutine(rotateElement(head));
		}
	}
	//the sphere collider with the trigger of the turret has detected an enemy, so it is added to the list of enemies
	void enemyDetected(GameObject col){
		if((tagToShoot==tagsToShoot.Player && col.gameObject.tag=="friend") || col.gameObject.tag==tagToShoot.ToString()){
			addEnemy(col.gameObject);
		}
	}
	//one of the enemies has left, so it is removed from the enemies list
	void enemyLost(GameObject col){
		if(((tagToShoot==tagsToShoot.Player && col.gameObject.tag=="friend") || col.gameObject.tag==tagToShoot.ToString()) && onSpotted){
			removeEnemy(col.gameObject);
		}
	}
	//if anyone shoot the turret, increase its field of view to search any enemy close to it
	void checkShootOrigin(GameObject bulletOwner){
		if (!onSpotted) {
			enemyDetected (bulletOwner);
		}
	}
	//add an enemy to the list, checking that that enemy is not already in the list
	void addEnemy(GameObject enemy){
		bool included=false;
		for(int i=0;i<enemies.Count;i++){
			if(enemy==enemies[i]){
				included=true;
			}
		}
		if (!included) {
			enemies.Add(enemy);
		}
	}
	//remove an enemy from the list
	void removeEnemy(GameObject enemy){
		enemies.Remove(enemy);
	}
	//when there is one enemy or more, check which is the closest to shoot it. 
	void closestTarget(){
		if (enemies.Count > 0) {
			float min = Mathf.Infinity;
			int index = -1;
			for (int i=0; i<enemies.Count; i++) {
				if (enemies [i]) {
					if (Vector3.Distance (enemies [i].transform.position, transform.position) < min) {
						min = Vector3.Distance (enemies [i].transform.position, transform.position);
						index = i;
					}
				}
			}
			enemyToShoot = enemies [index];
			Component component=enemyToShoot.GetComponent<health>();
			enemyHealth=component as health;
			if(enemyHealth.dead){
				removeEnemy(enemyToShoot);
				return;
			}
			if(!onSpotted){
				//the player can hack the turrets, but for that he has to crouch, so he can reach the back of the turret and activate the panel
				// if the player fails in the hacking or he gets up, the turret will detect the player and will start to fire him
				if(tagToShoot==tagsToShoot.Player){
					//check if the player fails or get up
					if(enemyToShoot.GetComponent<playerController>()){
						if(!enemyToShoot.GetComponent<playerController>().crouch || hackFailed){
							hacking = false;
							shootTarget();
						}
					}
					//else, the target is a friend of the player, so shoot him
					else{
						shootTarget();
					}
				}
				else{
					shootTarget();
				}
			}
		} 
		//if there are no enemies, the turret will set to pasive mode
		else {
			if(onSpotted){
				StartCoroutine(rotateElement(head));
				enemyHealth=null;
				enemyToShoot=null;
				onSpotted=false;
				fieldOfView.GetComponent<SphereCollider>().radius=originalFOVRaduis;
				viewTrigger.SetActive(true);
				deactivateWeapon();
				hackFailed=false;
			}
		}
	}
	//active the fire mode
	void shootTarget(){
		onSpotted=true;
		fieldOfView.GetComponent<SphereCollider>().radius=Vector3.Distance(enemyToShoot.transform.position,transform.position)+2;
		viewTrigger.SetActive(false);
		activateWeapon();
	}
	//the turret is been hacked
	void activateHack(){
		hacking=true;
	}
	//check the result of the hacking, true the turret now is an ally, else, the turret detects the player
	void hackResult(bool state){
		hacking = false;
		if (state) {
			hackDevice.GetComponent<enemyHackPanel>().disablePanelHack ();
			tag="friend";
			tagToShoot=tagsToShoot.enemy;
			enemies.Clear();
			//if the turret becomes an ally, change its icon color in the radar
			if(GetComponent<mapObjectInformation>()){
				GetComponent<mapObjectInformation>().addMapObject("Friend");
			}
			//set in the health slider the new name and slider color
			rotateCylinder1.GetComponent<health>().hacked();
		} else {
			hackFailed=true;
		}
	}
	//follow the enemy position, to rotate torwards his direction
	void followTarget(Transform objective){
		//there are two parts in the turret that move, the head and the middle body
		Vector3 targetDir = objective.position - rotateCylinder1.transform.position;
		targetDir = targetDir - transform.up * transform.InverseTransformDirection (targetDir).y;
		targetDir = targetDir.normalized;
		Quaternion targetRotation = Quaternion.LookRotation (targetDir,transform.up);
		rotateCylinder1.transform.rotation = Quaternion.Slerp (rotateCylinder1.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		Vector3 targetDir2= objective.position - head.transform.position;
		Quaternion targetRotation2 = Quaternion.LookRotation (targetDir2,transform.up);
		head.transform.rotation = Quaternion.Slerp (head.transform.rotation, targetRotation2, rotationSpeed * Time.deltaTime);
	}
	//return the head of the turret to its original rotation
	IEnumerator rotateElement(GameObject element){
		Quaternion rot = element.transform.localRotation;
		Vector3 myForward = Vector3.Cross (element.transform.right, Vector3.up);
		Quaternion dstRot= Quaternion.LookRotation (myForward, Vector3.up);
		dstRot.y=0;
		for (float t = 0; t < 1;) {
			t += Time.deltaTime * 3*speedMultiplier;
			element.transform.localRotation = Quaternion.Slerp (rot,dstRot, t);
			yield return null;
		}
	}
	//if one enemy or more are inside of the turret's trigger, activate the weapon selected in the inspector: machine gun, laser or cannon
	void activateWeapon(){
		audioSource.PlayOneShot( locatedEnemy,Random.Range (0.8f, 1.2f));
		typeWeaponChoosed = (int)currentWeapon;
		//cannon
		if(typeWeaponChoosed == 0){
			aimCannon.SetActive(true);
			setWeapon();
			//the turret has an animation to activate and deactivate the machinge gun and the cannon
			//also if the machine gun is activated, and the cannon was activate before, this is disabled
			if (machineGunDeployed) {
				machineGunAnim["activateMachineGun"].speed = -1; 
				machineGunAnim["activateMachineGun"].time=machineGunAnim["activateMachineGun"].length;
				machineGunAnim.Play("activateMachineGun");
				machineGunDeployed=false;
			}
			laserBeam.SetActive(false);
			cannonAnim["activateCannon"].speed =1; 
			cannonAnim.Play("activateCannon");
			cannonDeployed=true;
			audioSource.clip = cannonSound;
		}
		//laser
		if(typeWeaponChoosed == 1){
			setWeapon();
			deactivateWeapon();
			laserBeam.SetActive(true);
			audioSource.clip = laserSound;
			audioSource.loop = true;
			audioSource.Play ();
		}
		//machine gun
		if(typeWeaponChoosed == 2){
			aimMachineGun.SetActive(true);
			setWeapon();
			if(cannonDeployed){
				cannonAnim["activateCannon"].speed = -1; 
				cannonAnim["activateCannon"].time=cannonAnim["activateCannon"].length;
				cannonAnim.Play("activateCannon");
				cannonDeployed=false;
			}
			laserBeam.SetActive(false);
			machineGunAnim["activateMachineGun"].speed =1; 
			machineGunAnim.Play("activateMachineGun");
			machineGunDeployed=true;
			audioSource.clip = machineGunSound;
		}
	}
	//if all the enemies in the trigger of the turret are gone, deactivate the weapons
	void deactivateWeapon(){
		audioSource.loop = false;
		if(cannonDeployed){
			cannonAnim["activateCannon"].speed = -1; 
			cannonAnim["activateCannon"].time=cannonAnim["activateCannon"].length;
			cannonAnim.Play("activateCannon");
			cannonDeployed=false;
		}
		if(machineGunDeployed) {
			machineGunAnim ["activateMachineGun"].speed = -1; 
			machineGunAnim ["activateMachineGun"].time = machineGunAnim["activateMachineGun"].length;
			machineGunAnim.Play ("activateMachineGun");
			machineGunDeployed=false;
		}
		laserBeam.SetActive(false);
	}
	//at the start, set the rate of shooting, the position where the bullet are shooted and the position where the bullet shells are released
	void setWeapon(){
		typeWeaponChoosed = (int)currentWeapon;
		if (typeWeaponChoosed == 0) {
			shootTimerLimit=0.7f;
			shootPosition=cannonShootPosition;
			shellPosition=cannonShellPosition;
		} else if (typeWeaponChoosed == 1) {
			shootPosition=laserShootPosition;
		} else {
			shootTimerLimit=0.1f;
			shootPosition=machineGunShootPosition;
			shellPosition=machineGunShellPosition;
		}
	}
	//the turret is destroyed, so disable all the triggers, the AI, and add a rigidbody to every object with a render, and add force to them
	void death(Vector3 pos){
		audioSource.loop = false;
//		if (GetComponentInChildren<dropPickUpSystem> ()) {
//			BroadcastMessage ("createObjects");
//		}
		dead = true;
		laserBeam.SetActive(false);
		Component[] components2=GetComponentsInChildren(typeof(Transform));
		foreach (Component c in components2)	{
			if(c.GetComponent<Renderer>() && c.gameObject.layer!=LayerMask.NameToLayer("Scanner") && !c.gameObject.transform.IsChildOf(GetComponent<damageInScreen>().numbersParent)
				&& (!c.GetComponent<ParticleRenderer>() && !c.GetComponent<ParticleAnimator>() )){
				rendererParts.Add(c.GetComponent<Renderer>());
				c.GetComponent<Renderer>().material.shader=transparent;
				c.transform.parent=transform;
				c.gameObject.layer=LayerMask.NameToLayer ("Ignore Raycast");
				if(!c.gameObject.GetComponent<Rigidbody>()){
					c.gameObject.AddComponent<Rigidbody>();
				}
				c.gameObject.AddComponent<BoxCollider>();
				c.gameObject.GetComponent<Rigidbody>().AddExplosionForce(500, pos, 50, 3);
			}
			else{
				if(c.gameObject.GetComponent<Collider>()){
					c.gameObject.GetComponent<Collider>().enabled=false;
				}
			}
		}
	}
	//if the player uses the power of slow down, reduces the rotation speed of the turret, the rate fire and the projectile velocity
	void reduceVelocity(float speedMultiplierValue){
		rotationSpeed = speedMultiplierValue;
		speedMultiplier = speedMultiplierValue;
	}
	//set the turret speed to its normal state
	void normalVelocity(){
		rotationSpeed = orignalRotationSpeed;
		speedMultiplier = 1;
	}
	//the turret is in a black hole, so its behaviour is paused
	void pauseAI(bool state){
		paused = state;
	}
	//just a way to assign the necessary objects and keep the inspector clean due to all the parts inside the turret
	void setTurretParts(){
		Component[] components=GetComponentsInChildren(typeof(Transform));
		foreach (Component child in components)	{
			if (child.name == "rotateCylinder1"){
				rotateCylinder1=child.gameObject;
			}
			if (child.name == "head"){
				head=child.gameObject;
			}
			if (child.name == "rayCastPosition"){
				rayCastPosition=child.gameObject;
			}
			if (child.name == "laserShootPosition"){
				laserShootPosition=child.gameObject;
			}
			if (child.name == "cannonShootPosition"){
				cannonShootPosition=child.gameObject;
			}
			if (child.name == "machineGunShootPosition"){
				machineGunShootPosition=child.gameObject;
			}
			if (child.name == "cannonShellPosition"){
				cannonShellPosition=child.gameObject;
			}
			if (child.name == "machineGunShellPosition"){
				machineGunShellPosition=child.gameObject;
			}
			if (child.name == "enemyLaserBeam"){
				laserBeam=child.gameObject;
			}
			if (child.name == "rotor"){
				rotor=child.gameObject;
			}
			if (child.name == "aimMachineGun"){
				aimMachineGun=child.gameObject;
			}
			if (child.name == "aimCannon"){
				aimCannon=child.gameObject;
			}
			if (child.name == "machineGun"){
				machineGun=child.gameObject;
			}
			if (child.name == "cannon"){
				cannon=child.gameObject;
			}
			if (child.name == "fieldOfView"){
				fieldOfView=child.gameObject;
			}
			if (child.name == "viewTrigger"){
				viewTrigger=child.gameObject;
			}
			if (child.name == "hackDevice"){
				hackDevice=child.gameObject;
			}
		}
	}
	public void setRandomWeapon(){
		int random = Random.Range (0, 2);
		switch (random) {
		case 0:
			currentWeapon = weaponType.cannon;
			break;
		case 1:
			currentWeapon = weaponType.laser;
			break;
		case 2:
			currentWeapon = weaponType.machineGun;
			break;
		}
		setWeapon ();
	}
}