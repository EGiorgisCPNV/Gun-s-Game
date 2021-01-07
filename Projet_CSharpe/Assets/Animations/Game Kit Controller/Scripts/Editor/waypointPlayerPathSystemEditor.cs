using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(waypointPlayerPathSystem))]
public class waypointPlayerPathSystemEditor : Editor{
	waypointPlayerPathSystem pathManager;
	SerializedObject objectToUse;
	GUIStyle style = new GUIStyle();
	bool advancedSettings;
	Color buttonColor;

	void OnEnable(){
		objectToUse = new SerializedObject(target);
		pathManager = (waypointPlayerPathSystem)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			pathManager = (waypointPlayerPathSystem)target;
			if (pathManager.showGizmo) {
				style.normal.textColor = pathManager.gizmoLabelColor;
				style.alignment = TextAnchor.MiddleCenter;
				for (int i = 0; i < pathManager.wayPoints.Count; i++) {
					if (pathManager.wayPoints [i].point) {
						string label = "Point: " + pathManager.wayPoints [i].Name + "\n Radius: ";
						if (pathManager.useRegularGizmoRadius) {
							label += pathManager.triggerRadius;
						} else {
							label += pathManager.wayPoints [i].triggerRadius;
						}
						label +="\n Show OffScreen Icon: ";
						if(pathManager.wayPoints [i].point.GetComponent<mapObjectInformation>().showOffScreenIcon){
							label+="On";
						}
						else{
							label+="Off";
						}
						label +="\n Show Map Icon: ";
						if(pathManager.wayPoints [i].point.GetComponent<mapObjectInformation>().showMapWindowIcon){
							label+="On";
						}
						else{
							label+="Off";
						}
						label +="\n Show Distance: ";
						if(pathManager.wayPoints [i].point.GetComponent<mapObjectInformation>().showDistance){
							label+="On";
						}
						else{
							label+="Off";
						}
						Handles.Label (pathManager.wayPoints [i].point.position + 
							pathManager.wayPoints [i].point.up*pathManager.wayPoints [i].point.GetComponent<mapObjectInformation>().triggerRadius, label, style);

						pathManager.wayPoints [i].point.GetComponent<mapObjectInformation> ().showGizmo = pathManager.showGizmo;
						pathManager.wayPoints [i].point.GetComponent<mapObjectInformation> ().showOffScreenIcon = pathManager.showOffScreenIcon;
						pathManager.wayPoints [i].point.GetComponent<mapObjectInformation> ().showMapWindowIcon = pathManager.showMapWindowIcon;
						pathManager.wayPoints [i].point.GetComponent<mapObjectInformation> ().showDistance = pathManager.showDistance;
						if (pathManager.useRegularGizmoRadius) {
							pathManager.wayPoints [i].point.GetComponent<mapObjectInformation> ().triggerRadius = pathManager.triggerRadius;
						} else {
							pathManager.wayPoints [i].point.GetComponent<mapObjectInformation> ().triggerRadius = pathManager.wayPoints [i].triggerRadius;
						}
					}
				}
			}
		}
	}
	public override void OnInspectorGUI(){
		if (objectToUse == null) {
			return;
		}
		objectToUse.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("inOrder"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showOneByOne"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showGizmo"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoLabelColor"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("useRegularGizmoRadius"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("gizmoRadius"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("triggerRadius"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showOffScreenIcon"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showMapWindowIcon"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("showDistance"));
		EditorGUILayout.PropertyField (objectToUse.FindProperty ("pathActive"));
		buttonColor = GUI.backgroundColor;
		EditorGUILayout.BeginHorizontal();
		string inputListOpenedText = "";
		if (advancedSettings) {
			GUI.backgroundColor = Color.gray;
			inputListOpenedText = "Hide Advanced Settings";
		} else {
			GUI.backgroundColor = buttonColor;
			inputListOpenedText = "Show Advanced Settings";
		}
		if (GUILayout.Button (inputListOpenedText)) {
			advancedSettings = !advancedSettings;
		}
		GUI.backgroundColor = buttonColor;
		EditorGUILayout.EndHorizontal();
		if (advancedSettings) {
			GUILayout.BeginVertical ("box");
			EditorGUILayout.Space();
			GUI.color = Color.cyan;
			EditorGUILayout.HelpBox("Configure timer options and functions to call once all the path is complete", MessageType.None);
			GUI.color = Color.white;
			EditorGUILayout.Space();
			EditorGUILayout.Space ();
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("objectToActive"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("activeFunctionName"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("useTimer"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("timerSpeed"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("minutesToComplete"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("secondsToComplete"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("extraTimePerPoint"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("pathCompleteAudioSound"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("pathUncompleteAudioSound"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("secondTimerSound"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("secondSoundTimerLowerThan"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("pointReachedSound"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("useLineRenderer"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("lineRendererColor"));
			EditorGUILayout.PropertyField (objectToUse.FindProperty ("lineRendererWidth"));
			EditorGUILayout.Space ();
			GUILayout.EndVertical ();
		}
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Waypoints List", "window",GUILayout.Height(30));
		showUpperList (objectToUse.FindProperty ("wayPoints"));
		EditorGUILayout.Space ();
		if (GUILayout.Button ("Rename WayPoints")) {
			pathManager.renamePoints ();
		}
		EditorGUILayout.Space ();
		GUILayout.EndVertical ();
		if (GUI.changed) {
			objectToUse.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("Name"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("point"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("triggerRadius"));
		EditorGUILayout.PropertyField(list.FindPropertyRelative("reached"));
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.Label ("Number Of Points: \t" + list.arraySize.ToString ());
			EditorGUILayout.Space();
			GUILayout.Label ("Reached points: \t" +  objectToUse.FindProperty ("pointsReached").intValue);
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Point")){
				pathManager.addNewWayPoint ();
			}
			if (GUILayout.Button("Clear")){
				list.arraySize=0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++) {
				bool expanded = false;
				GUILayout.BeginHorizontal ();
				GUILayout.BeginHorizontal ("box");
				EditorGUILayout.Space ();
				if (i < list.arraySize && i >= 0) {
					EditorGUILayout.BeginVertical ();
					EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						expanded = true;
						showListElementInfo (list.GetArrayElementAtIndex (i));
					}
					EditorGUILayout.Space ();
					GUILayout.EndVertical ();
				}
				GUILayout.EndHorizontal ();
				if (expanded) {
					GUILayout.BeginVertical ();
				} else {
					GUILayout.BeginHorizontal ();
				}
				if (GUILayout.Button ("x")) {
					Transform point = list.GetArrayElementAtIndex (i).FindPropertyRelative ("point").objectReferenceValue as Transform;
					DestroyImmediate (point.gameObject);
					list.DeleteArrayElementAtIndex (i);
				}
				if (GUILayout.Button ("v")) {
					if (i >= 0) {
						list.MoveArrayElement (i, i + 1);
					}
				}
				if (GUILayout.Button ("^")) {
					if (i < list.arraySize) {
						list.MoveArrayElement (i, i - 1);
					}
				}
				if (expanded) {
					GUILayout.EndVertical ();
				} else {
					GUILayout.EndHorizontal ();
				}
				GUILayout.EndHorizontal ();
			}
		}       
	}
}
#endif