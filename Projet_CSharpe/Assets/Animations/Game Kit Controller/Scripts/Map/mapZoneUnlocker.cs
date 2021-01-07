using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mapZoneUnlocker : MonoBehaviour {
	public List<GameObject> mapZoneToUnclokList = new List<GameObject> ();
	public List<GameObject> mapFloorsToUnclokList = new List<GameObject> ();
	public bool unlockOnlyMapParts;
	public bool unlockFullFloors;

	public void unlockMapZone(){
		if (unlockOnlyMapParts) {
			for (int i = 0; i < mapZoneToUnclokList.Count; i++) {
				mapZoneToUnclokList [i].GetComponent<mapTileBuilder> ().enableMapPart ();
			}
		}
		if (unlockFullFloors) {
			for (int i = 0; i < mapFloorsToUnclokList.Count; i++) {
				Component[] components = mapFloorsToUnclokList[i].GetComponentsInChildren (typeof(mapTileBuilder));
				foreach (Component c in components) {
					c.GetComponent<mapTileBuilder> ().enableMapPart ();
				}
			}
		}
	}
}
