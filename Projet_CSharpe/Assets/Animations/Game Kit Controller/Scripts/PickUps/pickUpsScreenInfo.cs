using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class pickUpsScreenInfo : MonoBehaviour {
	public bool pickUpScreenInfoEnabled;
	public GameObject originalText;
	public float durationTimer;
	public float positionDistance;
	List<GameObject> textList =new List<GameObject> ();
	float textTimer;

	void Start () {
	
	}
	//display in the screen the type of pick ups that the objects grabs, setting their names and amount grabbed, setting the text position and the time that
	//is visible

	void Update () {
		//if there are text elements, then check the timer, and delete them
		if (textList.Count > 0) {
			if(Time.time > textTimer+durationTimer){
				Destroy (textList[0]);
				textList.RemoveAt(0);
				setPositions();
				textTimer = Time.time;
			}
		}
	}
	//the player has grabbed a pick up, so display the info in the screen, instantiating a new text component
	public void recieveInfo(string info){
		if (pickUpScreenInfoEnabled) {
			GameObject newText = (GameObject)Instantiate (originalText, Vector3.zero, Quaternion.identity);
			Vector3 textPosition = originalText.transform.position;
			if (info.Length > 12) {
				float extraPositionX = info.Length - 12;
				textPosition.x -= extraPositionX * 5;
				newText.GetComponent<RectTransform> ().sizeDelta = new Vector2 (newText.GetComponent<RectTransform> ().sizeDelta.x + extraPositionX * 20, 
					newText.GetComponent<RectTransform> ().sizeDelta.y);
			}
			newText.transform.position = textPosition;
			newText.transform.SetParent (originalText.transform.parent);
			newText.transform.localScale = Vector3.one / 2;
			newText.SetActive (true);
			newText.GetComponent<Text> ().text = info;
			textList.Add (newText);
			//set the text in the correct position
			setPosition (newText);
			textTimer = Time.time;
		}
	}
	//when the bottom text is removed, move the remaining list of text 1 unit(positionDistance) down
	void setPositions(){
		for (int i=textList.Count-1; i>=0; i--) {
			textList [i].GetComponent<RectTransform> ().position += positionDistance * (-1) * Vector3.up;
		}
	}
	//using the textlist count, set the position of every text element
	void setPosition(GameObject obj){
		obj.transform.GetComponent<RectTransform> ().position += positionDistance * textList.Count * Vector3.up;
	}
}
