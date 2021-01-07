using UnityEngine;
using System.Collections;
public class vendingMachine : MonoBehaviour {
	public GameObject objectToSpawn;
	public Transform spawnPosition;
	public float radiusToSpawn;
	public bool showGizmo;
	public void getObject(){
		//simple script to spawn vehicles in the scene, or other objects
		Vector3 positionToSpawn=spawnPosition.position;
		if (radiusToSpawn > 0) {
			Vector2 circlePosition = Random.insideUnitCircle * radiusToSpawn;
			Vector3 newSpawnPosition = new Vector3 (circlePosition.x, 0, circlePosition.y);
			positionToSpawn += newSpawnPosition;
		}
		GameObject objectToSpawnClone = (GameObject)Instantiate (objectToSpawn, positionToSpawn, spawnPosition.rotation);
		objectToSpawnClone.name = objectToSpawn.name;
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (showGizmo) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere (spawnPosition.position, radiusToSpawn);
		}
	}
}