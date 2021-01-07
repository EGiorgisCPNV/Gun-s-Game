using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class changeObjectColors : MonoBehaviour {
	bool changeColors;
	public List<Material> materials=new List<Material>();
	public List<Color> originalColor = new List<Color> ();
	int i,j,k;
	float timer = 0;

	void Start () {
	
	}
	void Update () {
		if (changeColors) {
			timer += Time.deltaTime;
			for (k = 0; k < materials.Count; k++) {
				materials [k].color = Color.Lerp ( materials [k].color, originalColor [k], timer/3);
			}
		}
	}

	public void setCurrentColors(List<Material> materialsList, List<Color> originalColorList){
		materials = materialsList;
		originalColor = originalColorList;
		changeColors = true;
	}
}
