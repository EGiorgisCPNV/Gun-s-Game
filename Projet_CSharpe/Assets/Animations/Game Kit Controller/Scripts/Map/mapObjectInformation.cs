using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class mapObjectInformation : MonoBehaviour {
	public string name;
	[TextArea(3,10)]
	public string description;
	public bool showGizmo;
	public bool showOffScreenIcon = true;
	public bool showMapWindowIcon = true;
	public bool showDistance = true;
	public float triggerRadius = 5;
	public Color triggerColor = Color.blue;
	public float gizmoLabelOffset;
	public Color gizmoLabelColor = Color.white;
	public int typeIndex;
	public string typeName;
	public string[] typeNameList;
	public int floorIndex;
	public string currentFloor;
	public string[] floorList;
	mapSystem mapManager;
	GameObject player;
	GameObject character;

	void Start () {
		createMapIconInfo ();
	}
	public void createMapIconInfo(){
		if (floorIndex == 0) {
			floorIndex = -1;
		}
		if (typeName != "") {
			if (typeName != "Objective" && typeName != "Path Element") {
				if (tag == "enemy") {
					typeName = "Enemy";
				} else if (tag == "friend") {
					typeName = "Friend";
				}
				mapManager = GameObject.Find ("Character").GetComponent<mapSystem> ();
				if (mapManager) {
					mapManager.addMapObject (gameObject, typeName, false);
				}
			} else {
				player = GameObject.Find ("Player Controller");
				player.GetComponent<setObjective> ().addElementToList (gameObject, true, triggerRadius, showOffScreenIcon, showMapWindowIcon, showDistance);
			}
		} else {
			print ("Object without map object information configurated "+gameObject.name);
		}
	}
	public void addMapObject(string mapIconType){
		mapManager.addMapObject (gameObject, mapIconType, false);
	}
	public void removeMapObject(){
		//remove object of the radar
		mapManager.removeMapObject(gameObject,false);
	}
	public void setPathElementInfo(bool showOffScreenIconInfo, bool showMapWindowIconInfo, bool showDistanceInfo){
		typeName = "Path Element";
		floorIndex = -1;
		showGizmo = true;
		showOffScreenIcon = showOffScreenIconInfo;
		showMapWindowIcon = showMapWindowIconInfo;
		showDistance = showDistanceInfo;
	}
	public void getMapIconTypeList(){
		if (!character) {
			character = GameObject.Find ("Character");
		} 
		if (character){
			mapManager = character.GetComponent<mapSystem> ();
			typeNameList = new string[mapManager.mapIconTypes.Count];
			for (int i = 0; i < mapManager.mapIconTypes.Count; i++) {
				typeNameList [i] = mapManager.mapIconTypes [i].typeName;
			}
			#if UNITY_EDITOR
			EditorUtility.SetDirty (GetComponent<mapObjectInformation>() );
			#endif
		}
	}
	public void getFloorList(){
		if (!character) {
			character = GameObject.Find ("Character");
		} 
		if (character){
			mapManager = character.GetComponent<mapSystem> ();
			floorList = new string[mapManager.floors.Count + 1];
			floorList [0] = "Visible in all floors";
			for (int i = 0; i < mapManager.floors.Count; i++) {
				floorList [i+1] = (i + 1 ).ToString();
			}
			#if UNITY_EDITOR
			EditorUtility.SetDirty (GetComponent<mapObjectInformation>() );
			#endif
		}
	}
	public void getIconTypeIndexByName(string iconTypeName){
		int index= mapManager.getIconTypeIndexByName (iconTypeName);
		if (index != -1) {
			typeIndex = index;
			typeName = iconTypeName;
		}
	}
	public void getMapObjectInformation(){
		getMapIconTypeList ();
		getFloorList ();
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		if (!Application.isPlaying) {
			getMapObjectInformation ();
		}
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showGizmo) {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere (transform.position, triggerRadius);
		}
	}
}