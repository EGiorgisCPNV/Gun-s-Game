using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class pickUpElementInfo {
	public string pickUpType;
	public List<pickUpTypeElementInfo> pickUpTypeList = new List<pickUpTypeElementInfo> ();

	[System.Serializable]
	public class pickUpTypeElementInfo {
		public string name;
		public GameObject pickUpObject;
	}
}