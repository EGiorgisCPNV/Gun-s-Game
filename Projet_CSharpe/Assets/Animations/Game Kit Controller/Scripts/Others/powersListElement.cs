using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class powersListElement : MonoBehaviour {

	//info about every power created in otherPowers
	public otherPowers.Powers powerData;
	public powerListType listType;
	public int defaultKeyNumber;

	void Start () {
		
	}
	//set the type of power elememt, list is for show the list of powers in the right of the screen, where will appear all of them
	//slot is for the wheel power and there will be only the powers selected
	public enum powerListType{
		list,slot,
	}
	//create the whole list of powers in the right of the screen, according to the powers created in the otherPowers inspector
	//setting the data power with the info of the power and the icon of the power 
	public void setData(otherPowers.Powers data){
		powerData = data;
		Component iconTexture=GetComponentInChildren(typeof(RawImage));
		iconTexture.GetComponent<RawImage>().texture=powerData.texture;
	}
	//get the default key number of every slot in the wheel when a power is drop from the list to the powers wheel
	public void setKey(){
		powerData.numberKey = defaultKeyNumber;
	}
}