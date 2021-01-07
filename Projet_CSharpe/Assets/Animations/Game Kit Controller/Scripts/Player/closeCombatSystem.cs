using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class closeCombatSystem : MonoBehaviour {
	public bool combatSystemEnabled;
	public float handsDamage;
	public float legsDamage;
	public float addForceMultiplier;
	public List<GameObject> legColliders=new List<GameObject>();
	public List<GameObject> handColliders=new List<GameObject>();
	public List<GameObject> handTrails=new List<GameObject>();
	public List<GameObject> legTrails=new List<GameObject>();
	[HideInInspector] public bool currentPlayerMode;
	float timerCombat=0;
	int npunch=0;
	int nkick=0;
	playerController character;
	otherPowers powers;
	Animator animator;
	float trailTimer;
	int i;
	bool fighting;
	float currentAnimLenght;
	inputManager input;
	menuPause pauseManager;
	powersListManager powersManager;
	changeGravity gravity;

	//this is a simple close combat system, I want to improve it in future updates, but for now, it worsk fine
	//just set your combat animatios in the combat layer of the animator
	//you can make combos of kick and punch, joining two kicks and a punch for example
	void Start () {
		for (i = 0; i < legColliders.Count; i++) {
			legColliders [i].GetComponent<hitCombat> ().hitDamage = legsDamage;
			legColliders [i].GetComponent<hitCombat> ().addForceMultiplier = addForceMultiplier;
		}
		for (i = 0; i < handColliders.Count; i++) {
			handColliders [i].GetComponent<hitCombat> ().hitDamage = handsDamage;
			handColliders [i].GetComponent<hitCombat> ().addForceMultiplier = addForceMultiplier;
		}
		animator = GetComponent<Animator>();
		character = gameObject.GetComponent<playerController> ();
		powers = GetComponent<otherPowers> ();
		input = transform.parent.GetComponent<inputManager> ();
		pauseManager= transform.parent.GetComponent<menuPause> ();
		powersManager = transform.parent.GetComponent<powersListManager> ();
		gravity = GetComponent<changeGravity> ();
	}
	void Update () {
		//check if the mouse buttons are pressed
		if (input.checkInputButton ("Shoot", inputManager.buttonType.getKeyDown)) {
			punch ();
		}
		if (input.checkInputButton ("Secondary Button", inputManager.buttonType.getKeyDown)) {
			kick ();
		}
		//get the current state of the animator in the combat layer
		AnimatorClipInfo[] ainfo = animator.GetCurrentAnimatorClipInfo(4);
		if (ainfo.Length != 0) {
			//get the current animation info
			for (int idx=0; idx<ainfo.Length; idx++) {
				currentAnimLenght=ainfo[idx].clip.length;
			}
			//enable the fight mode
			if(!fighting){
				fighting = true;
			}
		} else {
			//disable the fight mode
			if(fighting){
				fighting=false;
				disableCombat();
			}
		}
		//if the timer of the current animation is over, end the combat mode
		if(timerCombat>0){
			trailTimer=0;
			timerCombat-=Time.deltaTime;
			if(timerCombat<0){
				disableCombat();
			}
		}
		//configurate the trails renderer in the hands and foot of the player to disable smoothly
		if (trailTimer>0 && timerCombat==0) {
			trailTimer-=Time.deltaTime;
			if(trailTimer<0){
				trailTimer=0;
				//set the state of the triggers in the hands and foot of the player
				changeCollidersState(false,2,true);
			}
			for (i=0; i<handColliders.Count; i++) {
				if(handTrails [i].GetComponent<TrailRenderer> ().time>0){
					handTrails [i].GetComponent<TrailRenderer> ().time -=Time.deltaTime;
					legTrails [i].GetComponent<TrailRenderer> ().time -=Time.deltaTime;
				}
			}
		}
	}
	//the player has pressed the punch button
	public void punch(){
		//check the state of the player
		if (canUseCombat()) {
			//the current combat has only a combo of three punchs
			if (npunch < 3 ) {
				nkick=0;
				npunch++;
				//in the first punch, set the timer to a value, because the combat layer of the animator it cannot be check yet
				if (npunch == 1) {
					timerCombat = 0.33f;
					changeCollidersState(true,1,true);
				}
				//else, add to the timer the time of the animation
				else{
					timerCombat += currentAnimLenght;
				}
				//set the parameters of the animator
				animator.SetInteger ("punchNumber", npunch);
				animator.SetInteger ("kickNumber", nkick);
			}
		}
	}
	//the player has pressed the kick button
	public void kick(){
		//check the state of the player
		if (canUseCombat()) {
			//the current combat has only a combo of four kicks
			if (nkick < 4) {
				npunch=0;
				nkick++;
				//if the first kick, set the timer to a value, because the combat layer of the animator it cannot be check yet 
				if (nkick == 1) {
					timerCombat = 0.96f;
					changeCollidersState(true,0,true);
				}
				//else, add to the timer the time of the animation
				else {
					timerCombat += currentAnimLenght;
				}
				//set the parameters of the animator
				animator.SetInteger ("punchNumber", npunch);
				animator.SetInteger ("kickNumber", nkick);
			}
		}
	}
	bool canUseCombat(){
		bool value = false;
		if (currentPlayerMode && !character.powerActive && character.onGround && !powers.aim && !powers.usingWeapons && !pauseManager.usingDevice 
			&& combatSystemEnabled && !powersManager.editingPowers && !powersManager.selectingPower && !gravity.dead) {
			value = true;
		}
		return value;
	}
	//the combo has finished, so disable the combat mode
	void disableCombat(){
		npunch=0;
		nkick=0;
		timerCombat=0;
		animator.SetInteger ("punchNumber", npunch);
		animator.SetInteger ("kickNumber", nkick);
		changeCollidersState(false,2,false);
		trailTimer=1;
	}
	//disable or enable the triggers in the hands and foot of the player, to damage the enemy when they touch it
	void changeCollidersState(bool state,int type,bool trail){
		//check what colliders have to be activated or deactivated, the hands or the foot, to damage the enemy with 
		//the correct triggers according to the type of combo, kicks or punchs
		if (type == 0 || type == 2) {
			for (i=0; i<legColliders.Count; i++) {
				legColliders [i].SetActive(state);
				if(trail){
					legTrails[i].GetComponent<TrailRenderer>().enabled=state;
					legTrails [i].GetComponent<TrailRenderer> ().time = 1;
				}
			}
		}
		if (type == 1 || type == 2) {
			for (i=0; i<handColliders.Count; i++) {
				handColliders [i].SetActive(state);
				if(trail){
					handTrails[i].GetComponent<TrailRenderer>().enabled=state;
					handTrails [i].GetComponent<TrailRenderer> ().time = 1;
				}
			}
		}
	}
}