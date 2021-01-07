using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class ragdollBuilder : MonoBehaviour {
	public Animator animator;
	public Transform pelvis;
	public Transform leftHips;
	public Transform leftKnee;
	public Transform leftFoot;
	public Transform rightHips;
	public Transform rightKnee;
	public Transform rightFoot;
	public Transform leftArm;
	public Transform leftElbow;
	public Transform rightArm;
	public Transform rightElbow;
	public Transform middleSpine;
	public Transform head;
	public float totalMass;
	public bonesConfiguration bonesSettings;
	[HideInInspector] public bool ragdollAdded;
	Vector3 vectorRight = Vector3.right;
	Vector3 vectorUp = Vector3.up;
	Vector3 vectorForward = Vector3.forward;
	Vector3 worldRight = Vector3.right;
	Vector3 worldUp = Vector3.up;
	Vector3 worldForward = Vector3.forward;   
	List<boneParts> bones=new List<boneParts>();
	List<Transform> bonesTransforms = new List<Transform> ();
	boneParts pelvisBone;
	bool canBeCreated;
	int i;
	float distance;
	int direction;

	public bool createRagdoll (){  
		bool created = false;
		distance = 0;
		direction = 0;
		canBeCreated =true;
		bonesTransforms.Clear ();
		//get the correct bones
		if (animator) {		
			pelvis = animator.GetBoneTransform (HumanBodyBones.Hips);
			leftHips = animator.GetBoneTransform (HumanBodyBones.LeftUpperLeg);
			leftKnee = animator.GetBoneTransform (HumanBodyBones.LeftLowerLeg);
			leftFoot = animator.GetBoneTransform (HumanBodyBones.LeftFoot);
			rightHips = animator.GetBoneTransform (HumanBodyBones.RightUpperLeg);
			rightKnee = animator.GetBoneTransform (HumanBodyBones.RightLowerLeg);
			rightFoot = animator.GetBoneTransform (HumanBodyBones.RightFoot);
			leftArm = animator.GetBoneTransform (HumanBodyBones.LeftUpperArm);
			leftElbow = animator.GetBoneTransform (HumanBodyBones.LeftLowerArm);
			rightArm = animator.GetBoneTransform (HumanBodyBones.RightUpperArm);
			rightElbow = animator.GetBoneTransform (HumanBodyBones.RightLowerArm);
			middleSpine = animator.GetBoneTransform (HumanBodyBones.Chest);
			head = animator.GetBoneTransform (HumanBodyBones.Head);
			//add every bone to a list
			bonesTransforms.Add (pelvis);
			bonesTransforms.Add (leftHips);
			bonesTransforms.Add (leftKnee);
			bonesTransforms.Add (leftFoot);
			bonesTransforms.Add (rightHips);
			bonesTransforms.Add (rightKnee);
			bonesTransforms.Add (rightFoot);
			bonesTransforms.Add (leftArm);
			bonesTransforms.Add (leftElbow);
			bonesTransforms.Add (rightArm);
			bonesTransforms.Add (rightElbow);
			bonesTransforms.Add (middleSpine);
			bonesTransforms.Add (head);
			//check that every bone has been found, else the ragdoll is not built
			for (int i = 0; i < bonesTransforms.Count; i++) {
				if (!bonesTransforms [i]) {
					canBeCreated = false;
					print ("Some bone is not placed in the inspector");
					return false;
				}
			}
			//calculate axis of the model
			vectorUp = getDirectionAxis (pelvis.InverseTransformPoint (head.position));
			Vector3 pelvisDir = pelvis.InverseTransformPoint (rightElbow.position);
			Vector3 newVector = vectorUp.normalized * Vector3.Dot(pelvisDir, vectorUp.normalized);
			vectorRight = getDirectionAxis (pelvisDir - newVector);
			vectorForward = Vector3.Cross (vectorRight, vectorUp);
			//set the world axis directions
			worldRight = pelvis.TransformDirection (vectorRight);
			worldUp = pelvis.TransformDirection (vectorUp);
			worldForward = pelvis.TransformDirection (vectorForward);
			//create the bones list
			bones.Clear ();
			//create the pelvis bone first which is the main bone
			pelvisBone = new boneParts ();
			pelvisBone.name = "pelvis";
			pelvisBone.boneTransform = pelvis;
			pelvisBone.parent = null;
			pelvisBone.mass = bonesSettings.hipsMassPercentage;
			bones.Add (pelvisBone);
			//then create the rest of limbs
			createDoubleLimb ("hips", leftHips, rightHips, "pelvis", "capsule", worldRight, worldForward, new Vector2 (-20, 70), 30, bonesSettings.upperLegsScale, bonesSettings.upperLegsMassPercentage);
			createDoubleLimb ("knee", leftKnee, rightKnee, "hips", "capsule", worldRight, worldForward, new Vector2 (-80, 0), 0, bonesSettings.lowerLegsScale, bonesSettings.lowerLegsMassPercentage);
			createLimb ("middle Spine", middleSpine, "pelvis", "", worldRight, worldForward, new Vector2 (-20, 20), 10, bonesSettings.spineScale, bonesSettings.spineMassPercentage);
			createDoubleLimb ("arm", leftArm, rightArm, "middle Spine", "capsule", worldUp, worldForward, new Vector2 (-70, 10), 50, bonesSettings.upperArmsScale, bonesSettings.upperArmsMassPercentage);
			createDoubleLimb ("elbow", leftElbow, rightElbow, "arm", "capsule", worldForward, worldUp, new Vector2 (-90, 0), 0, bonesSettings.lowerArmsScale, bonesSettings.lowerArmsMassPercentage);
			createLimb ("head", head, "middle Spine", "",worldRight, worldForward, new Vector2 (-40, 25), 25, bonesSettings.headScale, bonesSettings.headMassPercentage);
			//if every part has been correctly configured, build the ragdoll
			if (canBeCreated) {
				buildRagdollComponents ();
				created = true;
			}
		}
		return created;
	}
	void createDoubleLimb (string name, Transform leftTransform, Transform rightTransform, string parent, string colliderType,
		Vector3 axis, Vector3 swingAxis, Vector2 limits, float swingLimit, float scale, float mass){
		createLimb ("left " + name, leftTransform, parent, colliderType, axis, swingAxis, limits, swingLimit, scale, mass);
		createLimb ("right " + name, rightTransform, parent, colliderType, axis, swingAxis, limits, swingLimit, scale, mass);
	}	
	void createLimb (string name, Transform boneTransform, string parent, string colliderType, Vector3 axis, Vector3 swingAxis,
		Vector2 limits, float swingLimit, float scale, float mass){
		boneParts bone = new boneParts();
		bone.name = name;
		bone.boneTransform = boneTransform;
		if (getBoneParent (parent) != null) {
			bone.parent = getBoneParent (parent);
		} else if (name.StartsWith ("left")) {
			bone.parent = getBoneParent ("left " + parent);
		} else if (name.StartsWith ("right")) {
			bone.parent = getBoneParent ("right " + parent);
		}	
		bone.colliderType = colliderType;
		bone.axis = axis;
		bone.swingAxis = swingAxis;
		bone.limits.x = limits.x;
		bone.limits.y = limits.y;
		bone.swingLimit = swingLimit;
		bone.scale = scale;
		bone.mass = mass;
		bone.parent.children.Add(bone);
		bones.Add (bone);
	}
	boneParts getBoneParent (string name){
		for (int i = 0; i < bones.Count; i++) {
			if (bones[i].name == name) {
				return bones[i];
			}
		}
		return null;
	}
	void buildRagdollComponents (){	
		Vector3 center = Vector3.zero;
		removeRagdoll ();
		for (int i = 0; i < bones.Count; i++) {
			//create capsule colliders
			if (bones[i].colliderType == "capsule") {
				if (bones[i].children.Count == 1) {
					boneParts childBone = (boneParts)bones[i].children [0];
					Vector3 endPosition = childBone.boneTransform.position;
					getOrientation (bones[i].boneTransform.InverseTransformPoint (endPosition));
				} else {
					Vector3 endPosition = (bones[i].boneTransform.position - bones[i].parent.boneTransform.position) + bones[i].boneTransform.position;
					getOrientation (bones[i].boneTransform.InverseTransformPoint (endPosition));
					if (bones[i].boneTransform.GetComponentsInChildren (typeof(Transform)).Length > 1) {
						Bounds bounds = new Bounds ();
						foreach (Transform child in bones[i].boneTransform.GetComponentsInChildren(typeof(Transform))) {
							bounds.Encapsulate (bones[i].boneTransform.InverseTransformPoint (child.position));
						}
						if (distance > 0) {
							distance = bounds.max [direction];
						} else {
							distance = bounds.min [direction];
						}
					}
				}
				CapsuleCollider collider = bones[i].boneTransform.gameObject.AddComponent <CapsuleCollider> ();
				collider.direction = direction;
				center = Vector3.zero;
				center [direction] = distance * 0.5f;
				collider.center = center;
				collider.height = Mathf.Abs (distance);
				collider.radius = Mathf.Abs (distance * bones[i].scale);
			}
			//add rigidbodies
			bones[i].boneTransform.gameObject.AddComponent<Rigidbody>();
			bones[i].boneTransform.GetComponent<Rigidbody>().mass = bones[i].mass;
			//build joints
			if (bones[i].parent!=null) {
				CharacterJoint joint = bones[i].boneTransform.gameObject.AddComponent <CharacterJoint> ();
				bones[i].joint = joint;
				//configure joint connections
				joint.axis = getDirectionAxis (bones[i].boneTransform.InverseTransformDirection (bones[i].axis));
				joint.swingAxis = getDirectionAxis (bones[i].boneTransform.InverseTransformDirection (bones[i].swingAxis));
				joint.anchor = Vector3.zero;
				joint.connectedBody = bones[i].parent.boneTransform.GetComponent<Rigidbody> ();
				//configure jount limits			
				SoftJointLimit limit = new SoftJointLimit ();
				limit.limit = bones[i].limits.x;
				joint.lowTwistLimit = limit;
				limit.limit = bones[i].limits.y;
				joint.highTwistLimit = limit;
				limit.limit = bones[i].swingLimit;
				joint.swing1Limit = limit;
				limit.limit = 0;
				joint.swing2Limit = limit;
				joint.enableProjection = true;
			}
		}
		//add box colliders to the hips and spine
		Bounds boxColliderInfo;
		BoxCollider boxCollider;
		boxColliderInfo = adjustColliderScale (getColliderSize (pelvis), pelvis, middleSpine, false);
		boxCollider = pelvis.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = boxColliderInfo.center;
		boxCollider.size = boxColliderInfo.size;
		boxColliderInfo = adjustColliderScale (getColliderSize (middleSpine), middleSpine, middleSpine, true);
		boxCollider = middleSpine.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = boxColliderInfo.center;
		boxCollider.size = boxColliderInfo.size;
		//add head collider
		float radius =(Vector3.Distance(rightArm.transform.position ,leftArm.transform.position))/4;
		SphereCollider sphere = head.gameObject.AddComponent <SphereCollider>();
		sphere.radius = radius;
		center = Vector3.zero;
		getOrientation (head.InverseTransformPoint(pelvis.position));
		if (distance > 0) {
			center [direction] = -radius;
		} else {
			center [direction] = radius;
		}
		sphere.center = center;
		//set the mass in the children of the pelvis
		for (int i = 0; i < bones.Count; i++) {
			bones[i].boneTransform.GetComponent<Rigidbody> ().mass = (totalMass*bones[i].mass)/100;
			bones [i].mass = bones [i].boneTransform.GetComponent<Rigidbody> ().mass;
		}
		ragdollAdded = true;
	}
	//adjust the box colliders for hips and spine
	Bounds adjustColliderScale (Bounds bounds, Transform relativeTo, Transform clipTransform, bool below){
		int axis = getCorrectAxis(bounds.size,true);
		if (Vector3.Dot (worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot (worldUp, relativeTo.TransformPoint(bounds.min)) == below){
			Vector3 min = bounds.min;
			min[axis] = relativeTo.InverseTransformPoint (clipTransform.position)[axis];
			bounds.min = min;
		}
		else{
			Vector3 max = bounds.max;
			max[axis] = relativeTo.InverseTransformPoint (clipTransform.position)[axis];
			bounds.max = max;
		}
		return bounds;
	}
	//get the size of the box collider for hips and spine
	Bounds getColliderSize (Transform relativeTo){
		Bounds bounds = new Bounds ();
		bounds.Encapsulate (relativeTo.InverseTransformPoint (leftHips.position));
		bounds.Encapsulate (relativeTo.InverseTransformPoint (rightHips.position));
		bounds.Encapsulate (relativeTo.InverseTransformPoint (leftArm.position));
		bounds.Encapsulate (relativeTo.InverseTransformPoint (rightArm.position));
		Vector3 size = bounds.size;
		size[getCorrectAxis (bounds.size,false)] = size[getCorrectAxis (bounds.size,true)] / 2;
		bounds.size = size;
		return bounds;		
	}
	public bool removeRagdoll(){
		bool removed = false;
		if (animator != null) {		
			for (int i = 0; i < bones.Count; i++) {
				//remove colliders, joints or rigidbodies
				if (bones[i].boneTransform) {
					if (bones[i].boneTransform.GetComponent<Joint> ()) {
						DestroyImmediate (bones[i].boneTransform.GetComponent<Joint> ());
					}
					if (bones[i].boneTransform.GetComponent<Rigidbody> ()) {
						DestroyImmediate (bones[i].boneTransform.GetComponent<Rigidbody> ());
					}
					if (bones[i].boneTransform.GetComponent<Collider> ()) {
						if (bones[i].boneTransform != leftFoot.transform && bones[i].boneTransform != rightFoot) {
							DestroyImmediate (bones[i].boneTransform.GetComponent<Collider> ());
						}
					}
				}		
			}
			removed = true;
			if (ragdollAdded) {
				ragdollAdded = false;
			}
		}
		return removed;
	}
	public void disableRagdollColliders(){
		if (animator != null) {		
			for (int i = 0; i < bones.Count; i++) {
				//disalbe colliders in the ragdoll
				if (bones[i].boneTransform) {
					if (bones[i].boneTransform.GetComponent<Collider> ()) {
						if (bones[i].boneTransform != leftFoot.transform && bones[i].boneTransform != rightFoot) {
							bones[i].boneTransform.GetComponent<Collider> ().enabled = false;
						}
					}
				}		
			}
		}
	}
	void getOrientation (Vector3 point){
		//get the axis with the longest value
		direction = 0;
		if (Mathf.Abs (point.y) > Mathf.Abs (point.x)) {
			direction = 1;
		}
		if (Mathf.Abs (point.z) > Mathf.Abs (point [direction])) {
			direction = 2;
		}
		distance = point[direction];
	}
	Vector3 getDirectionAxis (Vector3 point){
		getOrientation (point);
		Vector3 axis = Vector3.zero;
		if (distance > 0) {
			axis [direction] = 1;
		} else {
			axis [direction] = -1;
		}
		return axis;
	}
	int getCorrectAxis (Vector3 point, bool biggest){
		//get the bigger or lower axis to set the scale of the box collider
		direction = 0;
		if (biggest) {
			if (Mathf.Abs (point.y) > Mathf.Abs (point.x)) {
				direction = 1;
			}
			if (Mathf.Abs (point.z) > Mathf.Abs (point [direction])) {
				direction = 2;
			}
		} else {
			if (Mathf.Abs (point.y) < Mathf.Abs (point.x)) {
				direction = 1;
			}
			if (Mathf.Abs (point.z) < Mathf.Abs (point [direction])) {
				direction = 2;
			}
		}
		return direction;
	}
	public void getAnimator(Animator anim){
		animator = anim;
	}
	[System.Serializable]
	public class boneParts{
		public string name;
		public Transform boneTransform;
		public CharacterJoint joint;
		public boneParts parent;
		public string colliderType;
		public Vector3 axis;
		public Vector3 swingAxis;
		public Vector2 limits;
		public float swingLimit;
		public float scale;
		public float mass;
		public float childrenMass;
		public ArrayList children = new ArrayList();
	}
	[System.Serializable]
	public class bonesConfiguration{
		public float hipsScale=1;
		public float upperLegsScale=0.2f;
		public float lowerLegsScale=0.2f;
		public float upperArmsScale=0.25f;
		public float lowerArmsScale=0.2f;
		public float spineScale=1;
		public float headScale=1;
		public float hipsMassPercentage=20;
		public float upperLegsMassPercentage=9;
		public float lowerLegsMassPercentage=8;
		public float upperArmsMassPercentage=7;
		public float lowerArmsMassPercentage=6;
		public float spineMassPercentage=10;
		public float headMassPercentage=10;
	}
}