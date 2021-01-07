using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class mapTileBuilder : MonoBehaviour
{
	public List<Transform> verticesPosition = new List<Transform> ();
	public List<GameObject> eventTriggerList = new List<GameObject> ();
	public mapCreator mapManager;
	public Vector2 newPositionOffset;
	public bool mapPartEnabled = true;
	public bool useOtherColorIfMapPartDisabled;
	public Color colorIfMapPartDisabled;
	public bool showGizmo = true;
	public Color mapPartMaterialColor = Color.white;
	public Vector3 cubeGizmoScale = Vector3.one;
	public Color gizmoLabelColor = Color.white;
	public GameObject textMesh;
	public Vector3 center;
	GameObject wallRenderer;
	GameObject eventTriggerParent;
	int i;
	bool mapTileCreated;


	void Start ()
	{
		createMapTileElement ();
	}

	void OnEnable(){
		createMapTileElement ();
	}

	public void createMapTileElement(){
		if (!mapTileCreated) {
			List<Vector2> vertices2D = new List<Vector2> ();
			for (int i = 0; i < verticesPosition.Count; i++) {
				vertices2D.Add (new Vector2 (verticesPosition [i].localPosition.x, verticesPosition [i].localPosition.y));
			}
			// Use the triangulator to get indices for creating triangles
			mapCreator.Triangulator tr = new mapCreator.Triangulator (vertices2D);
			int[] indices = tr.Triangulate ();

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Count];
			for (int i = 0; i < vertices.Length; i++) {
				vertices [i] = new Vector3 (vertices2D [i].x, vertices2D [i].y, 0);
			}

			// Create the mesh
			Mesh msh = new Mesh ();
			msh.vertices = vertices;
			msh.triangles = indices;
			msh.RecalculateNormals ();
			msh.RecalculateBounds ();

			// Set up game object with mesh;
			GameObject newRenderer = new GameObject ();
			wallRenderer = newRenderer;
			wallRenderer.transform.SetParent (transform);
			wallRenderer.layer = LayerMask.NameToLayer (mapManager.mapLayer);
			wallRenderer.transform.localPosition = Vector3.zero;
			wallRenderer.transform.localRotation = Quaternion.identity;
			wallRenderer.name = "WallRenderer";
			wallRenderer.AddComponent (typeof(MeshRenderer));
			Material newMaterial = new Material (mapManager.floorMaterial);
			wallRenderer.GetComponent<MeshRenderer> ().material = newMaterial;
			MeshFilter filter = wallRenderer.AddComponent (typeof(MeshFilter)) as MeshFilter;
			filter.mesh = msh;
			wallRenderer.GetComponent<MeshRenderer> ().material.SetFloat ("_Mode", 2);
			setWallRendererMaterialColor (mapPartMaterialColor);
			if (!mapPartEnabled) {
				if (useOtherColorIfMapPartDisabled) {
					setWallRendererMaterialColor (colorIfMapPartDisabled);
					enableOrDisableTextMesh (false);
				} else {
					disableMapPart ();
				}
			}
			mapTileCreated = true;
		}
	}

	public void setWallRendererMaterialColor(Color newColor){
		wallRenderer.GetComponent<MeshRenderer> ().material.color = newColor;
	}
	public void enableMapPart ()
	{
		wallRenderer.SetActive (true);
		enableOrDisableTextMesh (true);
		mapPartEnabled = true;
	}

	public void disableMapPart ()
	{
		wallRenderer.SetActive (false);
		enableOrDisableTextMesh (false);
		mapPartEnabled = false;
	}

	public void enableOrDisableTextMesh(bool state){
		if (textMesh) {
			textMesh.SetActive (state);
			if (useOtherColorIfMapPartDisabled) {
				if (state) {
					setWallRendererMaterialColor (mapPartMaterialColor);
				} else {
					setWallRendererMaterialColor (colorIfMapPartDisabled);
				}
			}
		}
	}

	public void addEventTriggerToActive ()
	{
		if (eventTriggerList.Count == 0 || eventTriggerParent== null) {
			eventTriggerParent = new GameObject ();
			eventTriggerParent.name = "triggerParent";
			eventTriggerParent.transform.SetParent (transform);
			eventTriggerParent.transform.localPosition = Vector3.zero;
			eventTriggerParent.transform.localRotation = Quaternion.identity;
		}
		mapPartEnabled = false;
		GameObject trigger = new GameObject ();
		trigger.AddComponent<BoxCollider> ().isTrigger = true;
		trigger.AddComponent<eventTriggerSystem> ().setSimpleFunctionByTag ("enableMapPart", gameObject, "Player");
		trigger.transform.SetParent (eventTriggerParent.transform);
		trigger.transform.localPosition = Vector3.zero;
		trigger.transform.rotation = Quaternion.identity;
		trigger.layer = LayerMask.NameToLayer ("Ignore Raycast");
		trigger.name = "MapPartEnabledTrigger_"+(eventTriggerList.Count+1).ToString();
		eventTriggerList.Add (trigger);
	}
	public void addMapPartTextMesh(){
		if (!textMesh) {
			#if UNITY_EDITOR
			textMesh = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Game Kit Controller/Prefabs/Map System/mapPartTextMesh.prefab", typeof(GameObject));
			#endif
			if (textMesh) {
				textMesh = (GameObject)Instantiate (textMesh, transform.position, Quaternion.identity);
				textMesh.transform.SetParent (transform);
				textMesh.transform.position = center;
				textMesh.transform.localRotation = Quaternion.identity;
			} else {
				print ("Prefab not found");
			}
		}
	}

	public void addNewTransform ()
	{
		GameObject newTransform = new GameObject ();
		newTransform.transform.SetParent (transform);
		newTransform.transform.localRotation = Quaternion.identity;
		if (verticesPosition.Count > 0) {
			Vector3 lastPosition = verticesPosition [verticesPosition.Count - 1].localPosition;
			newTransform.transform.localPosition = new Vector3 (lastPosition.x + newPositionOffset.x, lastPosition.y + newPositionOffset.y, 0);
		} else {
			newTransform.transform.localPosition = Vector3.zero;
		}
		newTransform.name = (verticesPosition.Count + 1).ToString ("000");
		verticesPosition.Add (newTransform.transform);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<mapTileBuilder> ());
		#endif
	}

	public void renameTransforms ()
	{
		for (i = 0; i < verticesPosition.Count; i++) {
			if (verticesPosition [i]) {
				verticesPosition [i].name = (i + 1).ToString ("000");
			}
		}
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<mapTileBuilder> ());
		#endif
	}
	//draw every floor position and a line between floors
	void OnDrawGizmos ()
	{
		DrawGizmos ();
	}

	void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}
	//draw the pivot and the final positions of every door
	void DrawGizmos ()
	{
		if (showGizmo) {
			center = Vector3.zero;
			//if (!Application.isPlaying) {
			for (i = 0; i < verticesPosition.Count; i++) {
				if (verticesPosition [i] != null) {
					if (i + 1 < verticesPosition.Count) {
						if (verticesPosition [i + 1] != null) {
							Gizmos.color = Color.yellow;
							Gizmos.DrawLine (verticesPosition [i].position, verticesPosition [i + 1].position);
						}
					}
					if (i == verticesPosition.Count - 1) {
						if (verticesPosition [0] != null) {
							Gizmos.color = Color.yellow;
							Gizmos.DrawLine (verticesPosition [i].position, verticesPosition [0].position);
						}
					}
				} 
				center += verticesPosition [i].position;
			}
			center /= verticesPosition.Count;
			for (i = 0; i < eventTriggerList.Count; i++) {
				Gizmos.color = Color.red;
				Gizmos.DrawCube (eventTriggerList [i].transform.position, eventTriggerList [i].transform.localScale);

				Gizmos.color = Color.yellow;
				Gizmos.DrawLine (eventTriggerList [i].transform.position, center);
			}
			if (textMesh) {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere (textMesh.transform.position, 0.1f);

				Gizmos.color = Color.blue;
				Gizmos.DrawLine (textMesh.transform.position, center);
			}
			Gizmos.color = mapPartMaterialColor;
			Gizmos.DrawCube (center, cubeGizmoScale);

			//}
		}
	}
}
