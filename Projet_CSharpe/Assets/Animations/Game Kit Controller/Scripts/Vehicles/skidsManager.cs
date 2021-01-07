using UnityEngine;
using System.Collections;
public class skidsManager : MonoBehaviour {
	public int maxMarks = 1000;
	public float markWidth = 0.3f;
	public float groundOffset = 0.02f;
	public float minDistance = 0.1f;
	public GameObject originalMesh;
	markSection[] skidmarks;
	bool updated = false;
	int numMarks = 0;
	Mesh currentMesh;

	void  Start (){
		skidmarks = new markSection[maxMarks];
		for (int i = 0; i < maxMarks; i++) {
			skidmarks [i] = new markSection ();
		}
		originalMesh.GetComponent<MeshFilter> ().mesh = new Mesh ();
		originalMesh.transform.parent = null;
		originalMesh.transform.position = Vector3.zero;
		currentMesh = originalMesh.GetComponent<MeshFilter> ().mesh;
	}
	void  LateUpdate (){
		// If the mesh needs to be updated, i.e. a new section has been added,
		// the current mesh is removed, and a new mesh for the skidmarks is generated.
		if (updated) {
			updated = false;
			Mesh mesh = currentMesh;
			mesh.Clear ();
			int segmentCount = 0;
			for (int j = 0; j < numMarks && j < maxMarks; j++) {
				if (skidmarks [j].lastIndex != -1 && skidmarks [j].lastIndex > numMarks - maxMarks) {
					segmentCount++;
				}
			}
			Vector3[] vertices = new Vector3[segmentCount * 4];
			Vector3[] normals = new Vector3[segmentCount * 4];
			Vector4[] tangents = new Vector4[segmentCount * 4];
			Color[] colors = new Color[segmentCount * 4];
			Vector2[] uvs = new Vector2[segmentCount * 4];
			int[] triangles = new int[segmentCount * 6];
			segmentCount = 0;
			for (int i = 0; i < numMarks && i < maxMarks; i++) {
				if (skidmarks [i].lastIndex != -1 && skidmarks [i].lastIndex > numMarks - maxMarks) {
					markSection currentMark = skidmarks [i];
					markSection last = skidmarks [currentMark.lastIndex % maxMarks];
					vertices [segmentCount * 4 + 0] = last.positionLeft;
					vertices [segmentCount * 4 + 1] = last.positionRight;
					vertices [segmentCount * 4 + 2] = currentMark.positionLeft;
					vertices [segmentCount * 4 + 3] = currentMark.positionRight;

					normals [segmentCount * 4 + 0] = last.normal;
					normals [segmentCount * 4 + 1] = last.normal;
					normals [segmentCount * 4 + 2] = currentMark.normal;
					normals [segmentCount * 4 + 3] = currentMark.normal;

					tangents [segmentCount * 4 + 0] = last.tangent;
					tangents [segmentCount * 4 + 1] = last.tangent;
					tangents [segmentCount * 4 + 2] = currentMark.tangent;
					tangents [segmentCount * 4 + 3] = currentMark.tangent;

					colors [segmentCount * 4 + 0] = new Color (0, 0, 0, last.intensity);
					colors [segmentCount * 4 + 1] = new Color (0, 0, 0, last.intensity);
					colors [segmentCount * 4 + 2] = new Color (0, 0, 0, currentMark.intensity);
					colors [segmentCount * 4 + 3] = new Color (0, 0, 0, currentMark.intensity);

					uvs [segmentCount * 4 + 0] = new Vector2 (0, 0);
					uvs [segmentCount * 4 + 1] = new Vector2 (1, 0);
					uvs [segmentCount * 4 + 2] = new Vector2 (0, 1);
					uvs [segmentCount * 4 + 3] = new Vector2 (1, 1);

					triangles [segmentCount * 6 + 0] = segmentCount * 4 + 0;
					triangles [segmentCount * 6 + 2] = segmentCount * 4 + 1;
					triangles [segmentCount * 6 + 1] = segmentCount * 4 + 2;
					triangles [segmentCount * 6 + 3] = segmentCount * 4 + 2;
					triangles [segmentCount * 6 + 5] = segmentCount * 4 + 1;
					triangles [segmentCount * 6 + 4] = segmentCount * 4 + 3;

					segmentCount++;		
				}
			}
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.colors = colors;
			mesh.uv = uvs;
			mesh.triangles = triangles;
		}
	}
	// Function called by the wheels that is skidding. Gathers all the information needed to
	// create the mesh later. Sets the intensity of the skidmark section b setting the alpha
	// of the vertex color.
	public int AddSkidMark (Vector3 pos, Vector3 normal, float intensity, int lastIndex){
		int index = numMarks % maxMarks;
		if (intensity > 1) {
			intensity = 1;
		}
		if (skidmarks == null || intensity < 0 || index > skidmarks.Length) {
			return -1;
		}
		markSection curr = skidmarks [index];
		curr.position = pos + normal * groundOffset;
		curr.normal = normal;
		curr.intensity = intensity;
		curr.lastIndex = lastIndex;
		if (lastIndex != -1) {
			markSection last = skidmarks [lastIndex % maxMarks];
			Vector3 dir = (curr.position - last.position);
			Vector3 xDir = Vector3.Cross (dir, normal).normalized;
			curr.positionLeft = curr.position + xDir * markWidth * 0.5f;
			curr.positionRight = curr.position - xDir * markWidth * 0.5f;
			curr.tangent = new Vector4 (xDir.x, xDir.y, xDir.z, 1);
			if (last.lastIndex == -1) {
				last.tangent = curr.tangent;
				last.positionLeft = curr.position + xDir * markWidth * 0.5f;
				last.positionRight = curr.position - xDir * markWidth * 0.5f;
			}
		}
		numMarks++;
		updated = true;
		return numMarks - 1;
	}
	class markSection{
		public Vector3 position;
		public Vector3 normal;
		public Vector4 tangent;
		public Vector3 positionLeft;
		public Vector3 positionRight;
		public float intensity;
		public int lastIndex;
	};
}