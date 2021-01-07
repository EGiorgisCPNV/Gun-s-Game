using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class showGameInfoHud : MonoBehaviour {
	public List<hudElementInfo> hudElements =new List<hudElementInfo>();
	public enum elementType
	{
		Text, Slider
	}

	void Start () {
	
	}
	void Update () {
	
	}
	public GameObject getHudElement(string name){
		for (int i = 0; i < hudElements.Count; i++) {
			if (hudElements [i].name == name) {
				return(hudElements [i].hudElement.gameObject);
			}
		}
		return null;
	}
	[System.Serializable]
	public class hudElementInfo{
		public string name;
		public GameObject hudElement;
		public elementType hudElementyType;
	}
}