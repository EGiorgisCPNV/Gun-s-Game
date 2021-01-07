using UnityEngine;
using System.Collections;
public class AINavMesh : MonoBehaviour {
	public Transform target;
	public float targetChangeTolerance = 1;
	public float minDistance=3;
	public float minDistanceToMoveBack;
	public bool navMeshPaused;
	public bool patrolling;
	float navSpeed=1;
	Vector3 targetPos;
	Color c = Color.white;
	int i;
	UnityEngine.AI.NavMeshAgent agent;
	UnityEngine.AI.OffMeshLinkData _currLink;
	bool lookingPathAfterJump;
	bool followTarget;
	AIMoveInfo AIMoveInput=new AIMoveInfo();
	LineRenderer lineRenderer;
	Transform partner;
	public float patrolSpeed;
	Vector3 targetOffset;
	public bool runFromTarget;

	void Start () {
		agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
		if(!GetComponent<LineRenderer> ()){
			gameObject.AddComponent<LineRenderer> ();
			lineRenderer = GetComponent<LineRenderer> ();
			lineRenderer.material = new Material (Shader.Find ("Sprites/Default")) { color = c };
			lineRenderer.SetWidth (0.5f, 0.5f);
			lineRenderer.SetColors (c, c);
		}
	}
	void Update () {
		if (!navMeshPaused) {
			if (target) {
				float distance = Vector3.Distance (target.position, transform.position);
				if (patrolling) {
					targetPos = target.position;
					agent.SetDestination (targetPos);
					agent.transform.position = transform.position;
					followTarget = true;
					navSpeed = patrolSpeed;
				} else {
					if (runFromTarget) {
						Vector3 direction = transform.position - target.position;
						targetOffset = direction / distance;
						targetPos = target.position + targetOffset * distance * 10;
						agent.SetDestination (targetPos);
						// update the agents posiiton 
						agent.transform.position = transform.position;
						// use the values to move the character
						navSpeed = 20 / distance;
						navSpeed = Mathf.Clamp (navSpeed, 0.1f, 1);
						followTarget = true;
					} else {
						if (distance > minDistance) {
							// update the progress if the character has made it to the previous target
							if ((target.position - targetPos).magnitude > targetChangeTolerance) {
								targetPos = target.position;
								agent.SetDestination (targetPos);
							}
							// update the agents posiiton 
							agent.transform.position = transform.position;
							// use the values to move the character
							navSpeed = distance / 20;
							navSpeed = Mathf.Clamp (navSpeed, 0.1f, 1);
							followTarget = true;
						} else if (distance < minDistanceToMoveBack) {
							// update the progress if the character has made it to the previous target
							Vector3 direction = transform.position - partner.position;
							targetOffset = direction / distance;
							targetPos = target.position + targetOffset * (minDistanceToMoveBack + 1);
							agent.SetDestination (targetPos);
							// update the agents posiiton 
							agent.transform.position = transform.position;
							// use the values to move the character
							navSpeed = distance / 20;
							navSpeed = Mathf.Clamp (navSpeed, 0.1f, 1);
							followTarget = true;
						} else {
							// We still need to call the character's move function, but we send zeroed input as the move param.
							moveNavMesh (Vector3.zero, false, false, target.position);
							followTarget = false;
						}
					}
				}
				if (followTarget) {
					UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath ();
					bool hasFoundPath = agent.CalculatePath (targetPos, path);
					if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) {
						//print ("Can reach");
						moveNavMesh (agent.desiredVelocity * navSpeed, false, false, targetPos);
						c = Color.white;
					} else if (path.status == UnityEngine.AI.NavMeshPathStatus.PathPartial) {
						c = Color.yellow;
						if(Vector3.Distance(transform.position,path.corners[path.corners.Length-1])>2){
							moveNavMesh (agent.desiredVelocity * navSpeed, false, false, path.corners[path.corners.Length-1]);
						}	
						else{
							moveNavMesh (Vector3.zero, false, false, Vector3.zero);
						}
						//print ("Can get close");
					} else if (path.status == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
						c = Color.red;
						//print ("Can't reach");
					}
					if (agent.isOnOffMeshLink && !lookingPathAfterJump) {
						_currLink = agent.currentOffMeshLinkData;
						lookingPathAfterJump = true;
						moveNavMesh (agent.desiredVelocity * navSpeed, false, true, targetPos);
					} 
					//else {
					//			Move (agent.desiredVelocity * navSpeed, false, false, targetPos);
					//		}
					//		
					//		NavMeshHit navMeshHit;
					//		if(NavMesh.SamplePosition(agent.transform.position, out navMeshHit, 1f, NavMesh.AllAreas)) {
					//			print (navMeshHit.mask );
					//		}
					lineRenderer.enabled = true;
					lineRenderer.SetColors (c, c);
					lineRenderer.SetVertexCount (path.corners.Length);
					for (i = 0; i < path.corners.Length; i++) {
						lineRenderer.SetPosition (i, path.corners [i]);
					}
				} else {
					lineRenderer.enabled = false;
				}
			} else {
				moveNavMesh (Vector3.zero, false, false, Vector3.zero);
			}
		}
	}
	public void moveNavMesh(Vector3 move, bool crouch, bool jump, Vector3 lookPos){
		AIMoveInput.moveInput = move;
		AIMoveInput.crouchInput = crouch;
		AIMoveInput.jumpInput = jump;
		AIMoveInput.currentLookPosition = lookPos;
		SendMessage ("Move", AIMoveInput);
	}

	public void pauseAI(bool state){
		navMeshPaused = state;
		if (navMeshPaused) {
			agent.Stop ();
		} else {
			_currLink = agent.currentOffMeshLinkData;
			agent.CompleteOffMeshLink();
			agent.Resume ();
		}
		SendMessage ("pauseAction", navMeshPaused);
		lineRenderer.enabled = !state;
	}
	public void recalculatePath(){
		agent.Resume();
	}
	public void jumpEnded(){
		agent.CompleteOffMeshLink();
		//Resume normal navmesh behaviour
		agent.Resume();
		lookingPathAfterJump=false;
	}
	public void setTarget(Transform currentTarget){
		target = currentTarget;
	}
	public void avoidTarget(Transform targetToAvoid){
		target = targetToAvoid;
	}
	public void setAvoidTargetState(bool state){
		runFromTarget = state;
	}
//	public void setTargetOffset(Vector3 offset){
//		targetOffset = offset;
//	}
	public void removeTarget(){
		target = null;
	}
	public void partnerFound(Transform currentPartner){
		partner = currentPartner;
		target = partner;
		if (patrolling) {
			patrolling = false;
			SendMessage ("pauseOrPlayPatrol", true);
		}
		if (target.GetComponent<friendListManager> ()) {
			target.GetComponent<friendListManager> ().addFriend (gameObject);
		}
	}
	public void removeFromPartnerList(){
		if (partner) {
			if (partner.GetComponent<friendListManager> ()) {
				partner.GetComponent<friendListManager> ().removeFriend (transform);
			}
		}
	}
	public void lookAtTaget(bool state){
		AIMoveInput.lookAtTarget = state;
	}
	public void setPatrolState(bool state){
		patrolling = state;
	}
	public void setPatrolSpeed(float value){
		patrolSpeed = value;
	}
	public void attack(Transform currentTarget){
		setTarget (currentTarget);
	}
	public void follow(Transform currentTarget){
		setTarget (currentTarget);
	}
	public void wait(Transform currentTarget){
		removeTarget ();
	}
	public void hide(Transform currentTarget){
		setTarget (currentTarget);
	}
	[System.Serializable]
	public class AIMoveInfo{
		public Vector3 moveInput;
		public bool crouchInput;
		public bool jumpInput;
		public Vector3 currentLookPosition;
		public bool lookAtTarget;
	}
}