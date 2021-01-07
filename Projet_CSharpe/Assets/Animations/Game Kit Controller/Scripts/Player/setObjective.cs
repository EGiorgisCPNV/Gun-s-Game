using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class setObjective : MonoBehaviour {
	public Color objectiveColor;
	public GameObject objectiveIcon;
	public float minDefaultDistance=10;
	[Range(0,1)] public float iconOffset;
	[HideInInspector] public List<objectiveInfo> objectiveList = new List<objectiveInfo> ();	
	int i,j,k;
	mapSystem mapManager;

	void Start(){
		mapManager = GameObject.Find ("Character").GetComponent<mapSystem> ();
	}
	void Update () {
		for (i = 0; i < objectiveList.Count; i++) {
			if (objectiveList [i].mapObject && objectiveList[i].iconTransform) {
				float distance = Vector3.Distance (transform.position, objectiveList [i].mapObject.transform.position);
				if (distance < objectiveList[i].closeDistance) {
					removeElmentFromList (objectiveList [i]);
					return;
				}
				//get the target position from global to local in the screen
				Vector3 screenPoint = Camera.main.WorldToScreenPoint (objectiveList [i].mapObject.transform.position);
				//if the target is visible in the screnn, set the icon position and the distance in the text component
				if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height) {
					//change the icon from offscreen to onscreen
					if (!objectiveList [i].onScreenIcon.activeSelf) {
						objectiveList [i].onScreenIcon.SetActive (true);
						objectiveList [i].offScreenIcon.SetActive (false);
						if (objectiveList [i].showDistance) {
							objectiveList [i].iconText.gameObject.SetActive (true);
						} else {
							objectiveList [i].iconText.gameObject.SetActive (false);
						}
						objectiveList [i].iconTransform.rotation = Quaternion.identity;
					}
					objectiveList [i].iconTransform.position = screenPoint;
					if (objectiveList [i].showDistance) {
						objectiveList [i].iconText.text = Mathf.Round (distance).ToString ();
					}
				} 
				//if the target is off screen, change the icon to an arrow to follow the target position and also rotate the arrow to the target direction
				else {
					if (objectiveList [i].showOffScreenIcon) {
						//change the icon from onscreen to offscreen
						if (!objectiveList [i].offScreenIcon.activeSelf) {
							objectiveList [i].onScreenIcon.SetActive (false);
							objectiveList [i].offScreenIcon.SetActive (true);
							objectiveList [i].iconText.gameObject.SetActive (false);
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
						Vector3 screenBounds = screenCenter * iconOffset;
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
						//set the position and rotation of the arrow
						screenPoint += screenCenter;
						objectiveList [i].iconTransform.position = screenPoint;
						objectiveList [i].iconTransform.rotation = Quaternion.Euler (0, 0, angle * Mathf.Rad2Deg);
					} else {
						objectiveList [i].onScreenIcon.SetActive (false);
						objectiveList [i].iconText.gameObject.SetActive (false);
					}
				}
			}
		}
	}
	//get the renderer parts of the target to set its colors with the objective color, to see easily the target to reach
	public void addElementToList(GameObject obj, bool addMapIcon, float radiusDistance, bool showOffScreen, bool showMapWindowIcon, bool showDistanceInfo){
		objectiveInfo newObjective = new objectiveInfo ();
		newObjective.mapObject = obj;
		GameObject newScreenIcon = (GameObject)Instantiate (objectiveIcon, Vector3.zero, Quaternion.identity);
		newObjective.iconTransform = newScreenIcon.GetComponent<RectTransform> ();
		newObjective.iconTransform.SetParent (objectiveIcon.transform.parent);
		newObjective.iconTransform.localScale = Vector3.one;
		newObjective.onScreenIcon = newObjective.iconTransform.GetChild (0).gameObject;
		newObjective.offScreenIcon = newObjective.iconTransform.GetChild (1).gameObject;
		newObjective.iconText = newObjective.iconTransform.GetChild (2).GetComponent<Text> ();
		if (radiusDistance < 0) {
			radiusDistance = minDefaultDistance;
		}
		newObjective.closeDistance = radiusDistance;
		newObjective.showOffScreenIcon = showOffScreen;
		newObjective.showDistance = showDistanceInfo;

		Component[] components = obj.GetComponentsInChildren (typeof(Renderer));
		foreach (Renderer child in components) {
			if (child.material.HasProperty ("_Color")) {
				for (j = 0; j < child.materials.Length; j++) {
					newObjective.materials.Add (child.materials [j]);
					newObjective.originalColor.Add (child.materials [j].color);
					child.materials [j].color = objectiveColor;
				}
			}
		}
		//add the target to the radar, to make it also visible there
		if (!mapManager) {
			mapManager = GameObject.Find ("Character").GetComponent<mapSystem> ();
		}
		if (mapManager && addMapIcon && showMapWindowIcon) {
			mapManager.addMapObject (obj, "Objective", false);
		}
		objectiveList.Add (newObjective);
	}
	//if the target is reached, disable all the parameters and clear the list, so a new objective can be added in any moment
	public void removeElmentFromList(objectiveInfo objectiveListElement){
		Destroy(objectiveListElement.iconTransform.gameObject);
		bool isPathElement = false;
		if (objectiveListElement.mapObject.GetComponent<mapObjectInformation> ()) {
			if (objectiveListElement.mapObject.GetComponent<mapObjectInformation> ().typeName == "Path Element") {
				objectiveListElement.mapObject.transform.parent.SendMessage ("pointReached", objectiveListElement.mapObject.transform, SendMessageOptions.DontRequireReceiver);
				isPathElement = true;
			}
		}
		if (mapManager) {
			mapManager.removeMapObject (objectiveListElement.mapObject, isPathElement);
		}
		if (objectiveListElement.materials.Count > 0) {
			StartCoroutine (changeObjectColors (objectiveListElement));
		} else {
			objectiveList.Remove (objectiveListElement);
		}
	}
	public void removeGameObjectFromList(GameObject objectToSearch){
		for (j = 0; j < objectiveList.Count; j++) {
			if (objectiveList [j].mapObject == objectToSearch) {
				removeElmentFromList (objectiveList [j]);
			}
		}
	}
	public void removeGameObjectListFromList(List<GameObject> list){
		for (i = 0; i < list.Count; i++) {
			for (j = 0; j < objectiveList.Count; j++) {
				if (objectiveList [j].mapObject == list [i]) {
					Destroy(objectiveList [j].iconTransform.gameObject);
					if (mapManager) {
						mapManager.removeMapObject (objectiveList [j].mapObject, true);
						objectiveList.Remove (objectiveList [j]);
					}
				}
			}
		}
	}
	IEnumerator changeObjectColors(objectiveInfo objectiveListElement){
		for (float t = 0; t < 1;) {
			t += Time.deltaTime;
			for (k = 0; k < objectiveListElement.materials.Count; k++) {
				objectiveListElement.materials [k].color = Color.Lerp (objectiveListElement.materials [k].color, objectiveListElement.originalColor [k], t / 3);
			}
			yield return null;
		}
		objectiveList.Remove (objectiveListElement);
	}
	[System.Serializable]
	public class objectiveInfo{
		public GameObject mapObject;
		public RectTransform iconTransform;
		public GameObject onScreenIcon;
		public GameObject offScreenIcon;
		public Text iconText;
		public float closeDistance;
		public bool showOffScreenIcon;
		public bool showDistance;
		[HideInInspector] public List<Material> materials=new List<Material>();
		[HideInInspector] public List<Color> originalColor = new List<Color> ();
	}
}