using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
//a simple editor to add some buttons in the inputmanager script inspector
[CustomEditor(typeof(elevatorSystem))]
public class elevatorSystemEditor : Editor{
	elevatorSystem elevator;
	SerializedObject list;
	void OnEnable(){
		list = new SerializedObject(target);
		elevator = (elevatorSystem)target;
	}
	void OnSceneGUI(){   
		if (!Application.isPlaying) {
			elevator = (elevatorSystem)target;
			if (elevator.showGizmo) {
				for (int i = 0; i < elevator.floors.Count; i++) {
					GUIStyle style = new GUIStyle();
					style.normal.textColor = elevator.gizmoLabelColor;
					Handles.Label ( elevator.floors[i].floorPosition.position, elevator.floors[i].name+" - "+elevator.floors[i].floorNumber,style);						
				}
			}
		}
	}
	public override void OnInspectorGUI(){
		if (list == null) {
			return;
		}
		list.Update ();
		GUILayout.BeginVertical ("box");
		EditorGUILayout.PropertyField (list.FindProperty ("currentFloor"), new GUIContent ("Current Floor"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("speed"), new GUIContent ("Speed"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("insideElevatorDoor"), new GUIContent ("Inside Elevator Door"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("elevatorSwitchPrefab"), new GUIContent ("Elevator Switch Prefab"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("addSwitchInNewFloors"), new GUIContent ("Add Switch In New Floors"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("elevatorDoorPrefab"), new GUIContent ("Elevator Door Prefab"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("addDoorInNewFloors"), new GUIContent ("Add Door In New Floors"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("moving"), new GUIContent ("Moving"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("doorsClosed"), new GUIContent ("Doors Closed"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("showGizmo"), new GUIContent ("Show Gizmo"), false);
		EditorGUILayout.PropertyField (list.FindProperty ("gizmoLabelColor"), new GUIContent ("GizmoLabelColor"), false);
		GUILayout.EndVertical ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ("Floors List", "window");
		showUpperList (list.FindProperty ("floors"));
		GUILayout.EndVertical ();
		if (GUI.changed) {
			list.ApplyModifiedProperties ();
		}
		EditorGUILayout.Space ();
	}
	void showListElementInfo(SerializedProperty list){
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(list.FindPropertyRelative("name"), new GUIContent("Name"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("floorNumber"), new GUIContent("Floor Number"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("floorPosition"), new GUIContent("Floor Position"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("floorButton"), new GUIContent("Floor Button"), false);
		EditorGUILayout.PropertyField(list.FindPropertyRelative("outsideElevatorDoor"), new GUIContent("Outside Elevator Door"), false);
		GUILayout.EndVertical();
	}
	void showUpperList(SerializedProperty list){
		GUILayout.BeginVertical();
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded){
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Floor")){
				elevator.addNewFloor ();
			}
			if (GUILayout.Button("Clear List")){
				list.arraySize = 0;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			for (int i = 0; i < list.arraySize; i++){
				GUILayout.BeginHorizontal();
				GUILayout.BeginHorizontal("box");
				EditorGUILayout.Space();
				if (i < list.arraySize && i >= 0){
					EditorGUILayout.BeginVertical();
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
					if (list.GetArrayElementAtIndex (i).isExpanded) {
						showListElementInfo (list.GetArrayElementAtIndex (i));
					}
					EditorGUILayout.Space();
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				if (GUILayout.Button("x")){
					if(list.GetArrayElementAtIndex (i).FindPropertyRelative ("floorPosition").objectReferenceValue){
						Transform floor = list.GetArrayElementAtIndex (i).FindPropertyRelative ("floorPosition").objectReferenceValue as Transform;
						DestroyImmediate (floor.gameObject);
					}
					list.DeleteArrayElementAtIndex(i);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
}
#endif