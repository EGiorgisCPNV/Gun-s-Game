using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class mapCreator : MonoBehaviour {
	public List<floorInfo> floorsList = new List<floorInfo> ();
	public Material floorMaterial;
	public string mapLayer;
	public bool showGizmo;
	int i;

	void Start () {
	
	}
	void Update () {
	
	}
	public void addNewMapPart(GameObject currentMapPart){
		GameObject currentFloor = currentMapPart.transform.parent.gameObject;
		GameObject newMapPart = new GameObject ();
		newMapPart.transform.SetParent (currentFloor.transform);
		newMapPart.transform.localPosition = currentMapPart.transform.localPosition;
		newMapPart.transform.localRotation = Quaternion.Euler (90, 0, 0);
		newMapPart.AddComponent<mapTileBuilder> ();
		newMapPart.GetComponent<mapTileBuilder> ().mapManager = GetComponent<mapCreator> ();
		floorInfo currentFloorInfo = new floorInfo ();
		for (i = 0; i < floorsList.Count; i++) {
			if (floorsList [i].floor == currentFloor) {
				currentFloorInfo = floorsList [i];
			}
		}
		newMapPart.name = "MapPart-" + (currentFloorInfo.mapPartsList.Count + 1).ToString ("000");
		currentFloorInfo.mapPartsList.Add (newMapPart);
	}
	public void addNewMapPartFromMapCreator(int index){
		GameObject currentFloor = floorsList [index].floor;
		GameObject newMapPart = new GameObject ();
		newMapPart.transform.SetParent (currentFloor.transform);
		if ((index -1)>=0) {
			newMapPart.transform.localPosition = floorsList [index-1].floor.transform.localPosition;
		}
		newMapPart.transform.localRotation =  Quaternion.Euler(90,0,0);
		newMapPart.AddComponent<mapTileBuilder> ();
		newMapPart.GetComponent<mapTileBuilder> ().mapManager = GetComponent<mapCreator> ();
		floorInfo currentFloorInfo = new floorInfo ();
		for (i = 0; i < floorsList.Count; i++) {
			if (floorsList [i].floor == currentFloor) {
				currentFloorInfo = floorsList [i];
			}
		}
		newMapPart.name = "MapPart-"+ (currentFloorInfo.mapPartsList.Count+1).ToString ("000");
		currentFloorInfo.mapPartsList.Add (newMapPart);
	}
	public void duplicateMapPart(GameObject currentMapPart){
		GameObject currentFloor = currentMapPart.transform.parent.gameObject;
		GameObject newMapPart = (GameObject)Instantiate (currentMapPart, Vector3.zero, Quaternion.identity);
		newMapPart.transform.SetParent (currentFloor.transform);
		newMapPart.transform.localPosition = currentMapPart.transform.localPosition;
		newMapPart.transform.localRotation = currentMapPart.transform.localRotation;
		floorInfo currentFloorInfo = new floorInfo ();
		for (i = 0; i < floorsList.Count; i++) {
			if (floorsList [i].floor == currentFloor) {
				currentFloorInfo = floorsList [i];
			}
		}
		newMapPart.name = "MapPart-" + (currentFloorInfo.mapPartsList.Count + 1).ToString ("000");
		currentFloorInfo.mapPartsList.Add (newMapPart);
	}
	public void addNewFloor(){
		floorInfo newFloorInfo = new floorInfo ();
		newFloorInfo.floorNumber = floorsList.Count + 1;
		GameObject newFloor = new GameObject ();
		newFloor.transform.SetParent (transform);
		newFloor.transform.localPosition = Vector3.zero;
		newFloor.transform.localRotation = Quaternion.identity;
		newFloor.name = "floor-"+ (newFloorInfo.floorNumber).ToString ("000");
		newFloorInfo.Name = newFloor.name;
		newFloorInfo.floor = newFloor;
		floorsList.Add (newFloorInfo);

		GameObject newMapPart = new GameObject ();
		newMapPart.transform.SetParent (newFloor.transform);
		newMapPart.transform.localPosition = Vector3.zero;
		newMapPart.transform.localRotation =  Quaternion.Euler(90,0,0);
		newMapPart.AddComponent<mapTileBuilder> ();
		newMapPart.GetComponent<mapTileBuilder> ().mapManager = GetComponent<mapCreator> ();
		newMapPart.name = "MapPart-"+ (newFloorInfo.mapPartsList.Count+1).ToString ("000");
		newFloorInfo.mapPartsList.Add (newMapPart);
	}
	void OnDrawGizmos(){
		DrawGizmos ();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos ();
	}
	void DrawGizmos(){
		if (showGizmo && !Application.isPlaying) {
			for (i = 0; i < floorsList.Count; i++) {
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere (floorsList[i].floor.transform.position, 0.8f);
				if (i + 1 <= floorsList.Count - 1) {
					Gizmos.color = Color.white;
					Gizmos.DrawLine (floorsList[i].floor.transform.position, floorsList[i+1].floor.transform.position);
				}
			}
		}
	}	
	[System.Serializable]
	public class floorInfo{
		public string Name;
		public int floorNumber;
		public GameObject floor;
		public List<GameObject> mapPartsList = new List<GameObject> ();
	}
	public class Triangulator{
		List<Vector2> m_points = new List<Vector2>();
		public Triangulator (List<Vector2> points) {
			m_points = new List<Vector2>(points);
		}
		public int[] Triangulate() {
			List<int> indices = new List<int>();
			int n = m_points.Count;
			if (n < 3) {
				return indices.ToArray ();
			}
			int[] V = new int[n];
			if (Area() > 0) {
				for (int v = 0; v < n; v++) {
					V [v] = v;
				}
			}
			else {
				for (int v = 0; v < n; v++) {
					V [v] = (n - 1) - v;
				}
			}
			int nv = n;
			int count = 2 * nv;
			for (int m = 0, v = nv - 1; nv > 2; ) {
				if ((count--) <= 0) {
					return indices.ToArray ();
				}
				int u = v;
				if (nv <= u) {
					u = 0;
				}
				v = u + 1;
				if (nv <= v) {
					v = 0;
				}
				int w = v + 1;
				if (nv <= w) {
					w = 0;
				}
				if (Snip(u, v, w, nv, V)) {
					int a, b, c, s, t;
					a = V[u];
					b = V[v];
					c = V[w];
					indices.Add(a);
					indices.Add(b);
					indices.Add(c);
					m++;
					for (s = v, t = v + 1; t < nv; s++, t++) {
						V [s] = V [t];
					}
					nv--;
					count = 2 * nv;
				}
			}
			indices.Reverse();
			return indices.ToArray();
		}
		float Area () {
			int n = m_points.Count;
			float A = 0.0f;
			for (int p = n - 1, q = 0; q < n; p = q++) {
				Vector2 pval = m_points[p];
				Vector2 qval = m_points[q];
				A += pval.x * qval.y - qval.x * pval.y;
			}
			return (A * 0.5f);
		}
		bool Snip (int u, int v, int w, int n, int[] V) {
			int p;
			Vector2 A = m_points[V[u]];
			Vector2 B = m_points[V[v]];
			Vector2 C = m_points[V[w]];
			if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x)))) {
				return false;
			}
			for (p = 0; p < n; p++) {
				if ((p == u) || (p == v) || (p == w)) {
					continue;
				}
				Vector2 P = m_points[V[p]];
				if (InsideTriangle (A, B, C, P)) {
					return false;
				}
			}
			return true;
		}
		bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
			float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
			float cCROSSap, bCROSScp, aCROSSbp;

			ax = C.x - B.x; ay = C.y - B.y;
			bx = A.x - C.x; by = A.y - C.y;
			cx = B.x - A.x; cy = B.y - A.y;
			apx = P.x - A.x; apy = P.y - A.y;
			bpx = P.x - B.x; bpy = P.y - B.y;
			cpx = P.x - C.x; cpy = P.y - C.y;

			aCROSSbp = ax * bpy - ay * bpx;
			cCROSSap = cx * apy - cy * apx;
			bCROSScp = bx * cpy - by * cpx;

			return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
		}
	}
}
