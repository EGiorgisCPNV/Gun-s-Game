using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class setLaserRefractions : MonoBehaviour {
	public GameObject objectToChange;
	public Text textValue;

	//change a value in the object to change
 	public void getSliderValue(Slider info){
		int value = (int)info.value;
		objectToChange.SendMessage ("setValue",value);
		textValue.text = info.value.ToString ("#");
	}
}
