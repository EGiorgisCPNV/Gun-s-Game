using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class jetpackSystem : MonoBehaviour {
	public bool jetPackEquiped;
	public bool usingJetpack;
	public IKJetpackInfo IKInfo;
	public List<ParticleSystem> thrustsParticles=new List<ParticleSystem>();
	public GameObject jetpack;
	public string animationName;
	public GameObject jetpackHUDInfo;
	public Slider jetpackSlider;
	public Text fuelAmountText;
	public float jetpackForce;
	public float jetpackAirSpeed;
	public float jetpackAirControl;
	public float jetpackFuelAmount;
	public float jetpackFuelRate;
	public float regenerativeSpeed;
	public float timeToRegenerate;
	public bool showGizmo;
	inputManager input;
	playerController playerManager;
	int i;
	Animation jetPackAnimation;
	bool hudEnabled;
	float lastTimeUsed;

	void Start () {
		input = transform.parent.GetComponent<inputManager> ();
		playerManager = GetComponent<playerController> ();
		changeThrustsParticlesState (false);
		jetPackAnimation = jetpack.GetComponent<Animation> ();
		if (jetpackFuelAmount > 0) {
			jetpackSlider.maxValue = jetpackFuelAmount;
			jetpackSlider.value = jetpackFuelAmount;
			hudEnabled = true;
			fuelAmountText.text = jetpackSlider.maxValue.ToString("0") + " / " + jetpackSlider.value.ToString("0");
		}
	}
	void Update () {
		if(jetPackEquiped){
			if (!playerManager.aiming && !playerManager.driving) {
				if (input.checkInputButton ("Jump", inputManager.buttonType.getKeyDown)) {
					if (canUseJetpack ()) {
						startOrStopJetpack (true);
					}
				}
				if (input.checkInputButton ("Jump", inputManager.buttonType.getKeyUp)) {
					if (canUseJetpack ()) {
						startOrStopJetpack (false);
					}
				}
			}
			if (usingJetpack) {
				playerManager.onGround = false;
				if (hudEnabled) {
					jetpackSlider.value -= Time.deltaTime * jetpackFuelRate;
					fuelAmountText.text = jetpackSlider.maxValue.ToString("0") + " / " + jetpackSlider.value.ToString("0");
					if (jetpackSlider.value <= 0) {
						startOrStopJetpack (false);
					}
				}
			}
			else if (regenerativeSpeed > 0 && lastTimeUsed != 0) {
				if (Time.time > lastTimeUsed + timeToRegenerate) {
					jetpackSlider.value += regenerativeSpeed * Time.deltaTime;
					if (jetpackSlider.value >= jetpackSlider.maxValue) {
						jetpackSlider.value = jetpackSlider.maxValue;
						lastTimeUsed = 0;
					}
					fuelAmountText.text = jetpackSlider.maxValue.ToString("0") + " / " + jetpackSlider.value.ToString("0");
				}
			} 
		}
	}
	public bool canUseJetpack(){
		bool value = false;
		if (jetpackSlider.value > 0) {
			value = true;
		}
		return value;
	}
	public void startOrStopJetpack(bool state){
		if (usingJetpack != state) {
			usingJetpack = state;
			if (usingJetpack) {
				jetPackAnimation [animationName].speed = 1; 
				jetPackAnimation.Play (animationName);
			} else {
				jetPackAnimation [animationName].speed = -1; 
				jetPackAnimation [animationName].time = jetPackAnimation [animationName].length;
				jetPackAnimation.Play (animationName);
				lastTimeUsed = Time.time;
			}
			playerManager.usingJetpack = state;
			changeThrustsParticlesState (state);
			GetComponent<IKSystem> ().jetpackState (state, IKInfo);
		}
	}
	public void changeThrustsParticlesState(bool state){
		for (i = 0; i < thrustsParticles.Count; i++) {
			if (state) {
				if (!thrustsParticles [i].isPlaying) {
					thrustsParticles [i].gameObject.SetActive (true);
					thrustsParticles [i].Play ();
					thrustsParticles [i].loop = true;
				}
			} else {
				thrustsParticles [i].loop = false;
			}
		}
	}
	public void enableOrDisableJetpack(bool state){
		jetPackEquiped = state;
		playerManager.equipJetpack (state,jetpackForce,jetpackAirControl,jetpackAirSpeed);
		if (jetPackEquiped) {
			if (hudEnabled) {
				jetpackHUDInfo.SetActive (jetPackEquiped);
			}
		} else {
			jetpackHUDInfo.SetActive (jetPackEquiped);
		}
	}
	public void getJetpackFuel(float amount){
		float newValue = amount + jetpackSlider.value;
		if (newValue > jetpackSlider.maxValue) {
			jetpackSlider.maxValue = newValue;
		}
		jetpackSlider.value = newValue;
		fuelAmountText.text = jetpackSlider.maxValue.ToString("0") + " / " + jetpackSlider.value.ToString("0");
	}
	public void enableOrDisableJetPackMesh(bool state){
		jetpack.SetActive (state);
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos(){
		if (showGizmo) {
			for (i = 0; i < IKInfo.IKGoals.Count; i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere (IKInfo.IKGoals [i].position.position, 0.05f);
			}
			for (i = 0; i < IKInfo.IKHints.Count; i++) {
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere (IKInfo.IKHints [i].position.position, 0.05f);
			}
		}
	}
	[System.Serializable]
	public class IKJetpackInfo{
		public List<IKGoalsJetpackPositions> IKGoals=new List<IKGoalsJetpackPositions>();
		public List<IKHintsJetpackPositions> IKHints=new List<IKHintsJetpackPositions>();
	}
	[System.Serializable]
	public class IKGoalsJetpackPositions{
		public string Name;
		public AvatarIKGoal limb;
		public Transform position;
	}
	[System.Serializable]
	public class IKHintsJetpackPositions{
		public string Name;
		public AvatarIKHint limb;
		public Transform position;
	}
}