using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class footStepManager : MonoBehaviour {
	public bool soundsEnabled;
	public characterType character;
	[Range(0,1)] public float feetVolume=1;
	public float stepInterval;
	public footStepType typeOfFootStep;
	public LayerMask layer;
	public GameObject rightFootPrint;
	public GameObject leftFootPrint;
	public bool useFootPrints;
	public bool useFootPrintMaxAmount;
	public int footPrintMaxAmount;
	public float timeToRemoveFootPrints;
	public float maxFootPrintDistance;
	public bool removeFootPrintsInTime;
	public bool vanishFootPrints;
	public float vanishSpeed;
	public GameObject footParticles;
	public bool useFootParticles;
	public footStepsLayer[] footSteps;
	int surfaceIndex;
	float lastFootstepTime = 0;
	GameObject leftFoot;
	GameObject rightFoot;
	GameObject currentSurface;
	RaycastHit hit;
	playerController playerManager;
	bool usingAnimator = true;
	AudioSource cameraAudioSource;
	GameObject footPrintsParent;
	public enum footStepType{
		triggers, raycast
	}
	public enum characterType{
		Player, NPC
	}
	List<GameObject> footPrints =new List<GameObject> ();
	int i;
	float destroyFootPrintsTimer;

	void Start () {
		if (character == characterType.Player) {
			leftFoot = GetComponent<ragdollActivator> ().leftFoot;
			rightFoot = GetComponent<ragdollActivator> ().rightFoot;
			playerManager = GetComponent<playerController> ();
		}
		if (character == characterType.NPC) {
			leftFoot = GetComponent<Animator> ().GetBoneTransform (HumanBodyBones.LeftFoot).GetComponentInChildren<AudioSource> ().gameObject;
			rightFoot = GetComponent<Animator> ().GetBoneTransform (HumanBodyBones.RightFoot).GetComponentInChildren<AudioSource> ().gameObject;
		}
		leftFoot.GetComponent<AudioSource> ().volume = feetVolume;
		rightFoot.GetComponent<AudioSource> ().volume = feetVolume;
		cameraAudioSource = Camera.main.GetComponent<AudioSource> ();
		if (useFootPrints || useFootParticles) {
			footPrintsParent = new GameObject ();
			footPrintsParent.name = "footPrintsParent";
		}
	}
	void Update(){
		//if the player doesn't use the animator when the first person view is enabled, the footsteps in the feet of the player are disabled
		//so checkif the player is moving, and then play the steps sounds according to the stepInterval and the surface detected with a raycast under the player
		if (character == characterType.Player && !usingAnimator && soundsEnabled && playerManager.onGround && playerManager.isMoving) {
			if (Physics.Raycast (transform.position + transform.up * .1f, -transform.up, out hit, .5f, layer)) {
				//get the gameObject under the player's feet
				currentSurface=hit.collider.gameObject;
				//check the footstep frequency
				if (Time.time > lastFootstepTime + stepInterval/playerManager.animSpeedMultiplier ) {
					//get the audio clip according to the type of surface, mesh or terrain
					AudioClip soundEffect = getSound(LayerMask.LayerToName(currentSurface.layer).ToString(),transform.position,currentSurface,footStep.footType.center);
					if(soundEffect){
						//play one shot of the audio
						cameraAudioSource.PlayOneShot( soundEffect,Random.Range (0.8f, 1.2f));
						lastFootstepTime = Time.time;
					}
				}
			}
		}
		if (useFootPrints && removeFootPrintsInTime) {
			if (footPrints.Count > 0) {
				destroyFootPrintsTimer += Time.deltaTime;
				if (destroyFootPrintsTimer > timeToRemoveFootPrints) {
					for (i=0; i<footPrints.Count; i++) {
						if (footPrints [i]) {
							Destroy (footPrints [i]);
						}
					}
					footPrints.Clear ();
					destroyFootPrintsTimer = 0;
				}
			}
		}
	}
	public void changeFootStespType(bool state){
		if (typeOfFootStep == footStepType.raycast) {
			state=false;
		}
		if (!leftFoot || !rightFoot) {
			leftFoot = GetComponent<ragdollActivator> ().leftFoot;
			rightFoot = GetComponent<ragdollActivator> ().rightFoot;
		}
		leftFoot.SetActive(state);
		rightFoot.SetActive(state);
		usingAnimator=state;
	}

	public int GetMainTexture(Vector3 playerPos,Terrain terrain) {
		//get the index of the current texture of the terrain where the player is walking
		TerrainData terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;
		//calculate which splat map cell the playerPos falls within
		int mapX = (int)(((playerPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((playerPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		//get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
		float[,,] splatmapData = terrainData.GetAlphamaps(mapX,mapZ,1,1);
		//change the 3D array data to a 1D array:
		float[] cellMix = new float[splatmapData.GetUpperBound(2)+1];
		for (int n=0; n<cellMix.Length; n++){
			cellMix[n] = splatmapData[0,0,n];    
		}
		float maxMix = 0;
		int maxIndex = 0;
		//loop through each mix value and find the maximum
		for (int n=0; n<cellMix.Length; n++){
			if (cellMix[n] > maxMix){
				maxIndex = n;
				maxMix = cellMix[n];
			}
		}
		return maxIndex;
	}
	//get the audio clip, according to the layer of the object under the player, the position of the player, and the ground itself
	public AudioClip getSound(string layerName,Vector3 pos,GameObject ground,footStep.footType footSide){
		//if the player is in a terrain
		if (ground.GetComponent<Terrain> ()) {
			//get the current texture index of the terrain under the player.
			surfaceIndex = GetMainTexture (pos,ground.GetComponent<Terrain> ());
			for (int i=0;i<footSteps.Length;i++){
				//check if that terrain texture has a sound
				if(footSteps[i].checkTerrain && surfaceIndex==footSteps[i].terrainTextureIndex){
					int index=-1;
					if(footSteps[i].randomPool){
						//get a random sound
						index=randomStep(footSteps[i].poolSounds);
					}
					else{
						//get the next sound in the list
						footSteps[i].poolIndex++;
						if(footSteps[i].poolIndex>footSteps[i].poolSounds.Length-1){
							footSteps[i].poolIndex=0;
						}
						index=footSteps[i].poolIndex;
					}
					placeFootPrint(footSide);
					createParticles (footSide);
					//return the audio selected
					return footSteps[i].poolSounds[index];
				}
			}
		} 
		//else, the player is above a mesh
		else {
			surfaceIndex=-1;
			for (int i=0;i<footSteps.Length;i++){
				//check if the layer of the mesh has a sound 
				if(footSteps[i].checkLayer && layerName==footSteps[i].layerName){
					int index=-1;
					if(footSteps[i].randomPool){
						//get a random sound
						index=randomStep(footSteps[i].poolSounds);
					}
					else{
						//get the next sound in the list
						footSteps[i].poolIndex++;
						if(footSteps[i].poolIndex>footSteps[i].poolSounds.Length-1){
							footSteps[i].poolIndex=0;
						}
						index=footSteps[i].poolIndex;
					}
					placeFootPrint(footSide);
					createParticles (footSide);
					//return the audio selected
					return footSteps[i].poolSounds[index];
				}
			}
		}
		return null;
	}
	//get a random index of the pool of sounds
	int randomStep(AudioClip[] pool){
		int random = Random.Range (0, pool.Length);
		return random;
	}
	public void placeFootPrint(footStep.footType footSide){
		if (useFootPrints) {
			Vector3 footPrintPosition = Vector3.zero;
			bool isLeftFoot = false;
			if (footSide == footStep.footType.left) {
				footPrintPosition = leftFoot.transform.position;
				isLeftFoot = true;
			} else {
				footPrintPosition = rightFoot.transform.position;
			}
			if (Physics.Raycast (footPrintPosition, -transform.up, out hit, 5, layer)) {
				if (hit.distance < maxFootPrintDistance) {
					Vector3 placePosition = hit.point + transform.up * 0.013f;
					if (isLeftFoot) {
						createFootPrint (leftFootPrint, placePosition, transform.rotation, hit.normal);
					} else {
						createFootPrint (rightFootPrint, placePosition, transform.rotation, hit.normal);
					}
				}
			}
		}
	}
	public void createFootPrint(GameObject foot, Vector3 position, Quaternion rotation, Vector3 normal){
		GameObject newFootPrint = (GameObject)Instantiate (foot, position, rotation);
		newFootPrint.transform.SetParent (footPrintsParent.transform);
		Vector3 myForward = Vector3.Cross (newFootPrint.transform.right, normal);
		Quaternion dstRot = Quaternion.LookRotation (myForward, normal);
		newFootPrint.transform.rotation = dstRot;
		footPrints.Add (newFootPrint);
		if (vanishFootPrints) {
			newFootPrint.AddComponent<fadeObject> ().activeVanish (vanishSpeed);
		}
		if (useFootPrintMaxAmount && footPrintMaxAmount>0 && footPrints.Count > footPrintMaxAmount) {
			GameObject footPrintToRemove = footPrints [0];
			footPrints.RemoveAt (0);
			Destroy (footPrintToRemove);
		}
	}
	public void createParticles(footStep.footType footSide){
		if (useFootParticles) {
			Vector3 footPrintPosition = Vector3.zero;
			if (footSide == footStep.footType.left) {
				footPrintPosition = leftFoot.transform.position;
			} else {
				footPrintPosition = rightFoot.transform.position;
			}
			GameObject newFootParticle = (GameObject) Instantiate (footParticles, footPrintPosition, transform.rotation);
			newFootParticle.transform.SetParent (footPrintsParent.transform);
		}
	}
	public void enableOrDisableFootSteps(bool state){
		leftFoot.GetComponent<Collider> ().enabled = rightFoot.GetComponent<Collider> ().enabled = state;
	}
	//class to create every type of surface
	//selecting layerName and checkLayer to set that type of step in a mesh
	//if the current step is for a terrain, then set a terrainTextureName, checkTerrain and terrainTextureIndex according to the order in the terrain textures
	//set to true randomPool to play the sounds in a random order, else the sounds are played in the same order
	[System.Serializable]
	public class footStepsLayer{
		public string Name;
		public AudioClip[] poolSounds;
		public string layerName;
		public bool checkLayer;
		public string terrainTextureName;
		public bool checkTerrain;
		public int terrainTextureIndex;
		public bool randomPool;
		[HideInInspector] public int poolIndex;
	}
}