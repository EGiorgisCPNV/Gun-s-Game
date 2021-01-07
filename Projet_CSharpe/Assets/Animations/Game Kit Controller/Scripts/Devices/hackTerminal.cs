using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;
[Serializable]
public class hackTerminal : MonoBehaviour {
	public GameObject hackHud;
	public GameObject hackText;
	public GameObject content;
	public GameObject objectToHack;
	public Image backGround;
	public Text currentPasswordText;
	public Color hackedColor;
	public Slider timeSlider;
	public Texture[] icons;
	public float timerSpeed = 10;
	public float typeTime;
	public float minSwipeDist=50;
	public bool usingPanel;
	public string panelAnimation;
	public string hackingAnimation;
	List<int> order=new List<int>();
	List<RawImage> iconsImage=new List<RawImage>();
	int currentButton = 0;
	int i;
	int j;
	float hackTimer;
	float typeRate;
	float time;
	float characterChangeRate = 0.1f;
	float characterTime;
	bool checkHackState;
	bool disableHud;
	bool swiping;
	bool touchPlatform;
	bool hackTerminalOpened;
	bool hackTextComplete;
	bool setSliderToZero;
	bool changeColor;
	bool hacked;
	bool panelClosed;
	String finalText;
	string randomCharacter;
	Color alpha;
	RaycastHit hit;
	Vector3 swipeStartPos;
	Vector3 swipeFinalPos;
	Touch currentTouch;
	Animation hackHudAnimation;
	Animation hackPanelAnimation;
	bool objectToHackHasMoveCameraToDevice;

	//the four directions of a swipe
	class swipeDirections {
		public static Vector2 up = new Vector2( 0, 1 );
		public static Vector2 down = new Vector2( 0, -1 );
		public static Vector2 right = new Vector2( 1, 0 );
		public static Vector2 left = new Vector2( -1, 0 );
	}

	//the hack system has been changed from the player to an independent script, so like this, can be used in any type of device to hack it.
	void Start () {
		alpha = Color.white;
		alpha.a = 0.5f;
		getElements ();
		touchPlatform=touchJoystick.checkTouchPlatform ();
		if (currentPasswordText) {
			currentPasswordText.text = "";
			int length = objectToHack.GetComponent<accessTerminal> ().code.Length;
			for (int i = 0; i < length; i++) {
				currentPasswordText.text += "#";
			}
		}
		content.SetActive (false);
		hackHudAnimation = hackHud.GetComponent<Animation> ();
		hackPanelAnimation = GetComponent<Animation> ();
	}

	void Update () {
		//if the player is hacking a device, check if the WASD keys are pressed, to compare if the current key direction to press matches with the arrow direction
		if (checkHackState && !hackHudAnimation.IsPlaying(hackingAnimation)) {
			//in PC, the control use the key directions to hack the turret
			if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)){
				checkButton(0);
			}
			if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)){
				checkButton(1);
			}
			if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)){
				checkButton(2);
			}
			if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)){
				checkButton(3);
			}
			//also, if the touch controls are enabled, check any swipe in the screen, and check the direction
			int touchCount = Input.touchCount;
			if (!touchPlatform) {
				touchCount++;
			}
			for (int i = 0; i < touchCount; i++){
				if (!touchPlatform) {
					currentTouch = touchJoystick.convertMouseIntoFinger();
				}
				else{
					currentTouch = Input.GetTouch(i);
				}
				if (currentTouch.phase == TouchPhase.Began && !swiping) {
					swipeStartPos = currentTouch.position;
					swiping=true;
				}
				if (currentTouch.phase == TouchPhase.Ended && swiping) {
					//get the start and the final position of the swipe to get the direction, left, right, vertical or horizontal
					swipeFinalPos = currentTouch.position;
					swiping=false;
					Vector2 currentSwipe = new Vector2( swipeFinalPos.x - swipeStartPos.x, swipeFinalPos.y - swipeStartPos.y );
					if (currentSwipe.magnitude > minSwipeDist){
						currentSwipe.Normalize();
						if ( Vector2.Dot(currentSwipe, swipeDirections.up ) > 0.906f ) {
							//print("up swipe");
							checkButton(2);
							return;
						}
						if ( Vector2.Dot(currentSwipe, swipeDirections.down ) > 0.906f ) {
							//print("down swipe");
							checkButton(3);
							return;
						}
						if ( Vector2.Dot(currentSwipe, swipeDirections.left ) > 0.906f ) {
							//print("left swipe");
							checkButton(0);
							return;
						}
						if ( Vector2.Dot(currentSwipe, swipeDirections.right ) > 0.906f) {
							//print("right swipe");
							checkButton(1);
							return;
						}
					}
				}
			}
			//the player has a limit of time, if the time reachs the limit, the kack will fail
			hackTimer+=Time.deltaTime*timerSpeed;
			timeSlider.value=hackTimer;
			if(hackTimer>=timeSlider.maxValue){
				checkButton(-1);
			}
		}
		//active the animation to close the hack HUD
		if(setSliderToZero){
			timeSlider.value-=Time.deltaTime*15;
			if(timeSlider.value == 0){
				setSliderToZero=false;
				disableHud=true;
				hackHudAnimation[hackingAnimation].speed = 1; 
				hackHudAnimation.Play(hackingAnimation);
			}
		}
		//disable all the components of the hack interface
		if (disableHud && !hackHudAnimation.IsPlaying(hackingAnimation)) {
			disableHud=false;
			usingPanel=false;
		}
		//if the player ends the hacking, set a text in the HUD to show if he has successed or failed
		//also the text is displayed letter by letter with a random symbol before the real letter is set
		//for a hacking looking
		if (!hackTextComplete && hackHud.activeSelf) {
			if (Time.time - characterTime >= characterChangeRate) {
				randomCharacter = randomChar ();
				characterTime = Time.time;
			}
			hackText.GetComponent<Text> ().text = finalText.Substring (0, j) + randomCharacter;
			if (Time.time - time >= typeRate) {
				j++;
				time = Time.time;
			}
			bool isChar = false;
			while (!isChar) {
				if ((j + 1) < finalText.Length) {
					if (finalText.Substring (j, 1) == " ") {
						j++;
					} else {
						isChar = true;
					}
				} else {
					isChar = true;
				}
			}
			if (hackText.GetComponent<Text> ().text.Length == finalText.Length + 1) {
				hackText.GetComponent<Text> ().text = finalText;
				j = 0;
				time = 0;
				hackTextComplete = true;
			}
		}
		if (changeColor) {
			backGround.color = Vector4.MoveTowards (backGround.color, hackedColor, Time.deltaTime * 3);
			if (backGround.color == hackedColor) {
				changeColor = false;
			}
		}
		if(hacked && panelClosed && !hackPanelAnimation.IsPlaying(panelAnimation)){
			gameObject.SetActive (false);
		}
	}
	//enable the hack system
	public void activeHack() {
		if (!usingPanel && !hacked) {
			content.SetActive (true);
			moveHackTerminal (true);
			GetComponent<moveCameraToDevice> ().moveCamera (true);
			order.Clear ();
			currentButton = 0;
			hackHud.SetActive (true);
			//play the hack interface animation to "open" it
			hackHudAnimation [hackingAnimation].speed = -1; 
			hackHudAnimation [hackingAnimation].time = hackHudAnimation [hackingAnimation].length;
			hackHudAnimation.Play (hackingAnimation);
			hackTimer = 0;
			setHackText("Hacking...");
			hackTextComplete=false;
			timeSlider.value = 0;
			//get a random value to every button. There are four arrow Rawimage, stored in a gameObject array, so the random number is the index in the array
			//the right order is an integer array, that will be compared to the pressed keys
			for (int k=0; k<icons.Length; k++) {
				int randomNumber = UnityEngine.Random.Range (0, icons.Length - 1);
				iconsImage [k].texture = icons [randomNumber];
				iconsImage [k].color = alpha;
				order.Add (randomNumber);
			}
			checkHackState = true;
			usingPanel=true;
		}
	}
	//disable hacking
	public void disableHack(bool state){
		setSliderToZero = true;
		hackTextComplete = false;
		if (state) {
			StartCoroutine (resetCameraPos ());
		} else {
			GetComponent<moveCameraToDevice> ().moveCamera (false);
		}
		currentButton=0;
		checkHackState=false;
	}
	//if the hack has failed or not, the camera is reset to its regular position in the camera player
	IEnumerator resetCameraPos (){
		yield return new WaitForSeconds (1);
		GetComponent<moveCameraToDevice> ().moveCamera (false);
	}
	//open or close the hack terminal in the object that uses it
	public void moveHackTerminal(bool state){
		if (state) {
			if (!hackTerminalOpened) {
				hackPanelAnimation [panelAnimation].speed = 1; 
				hackPanelAnimation.Play (panelAnimation);
				hackTerminalOpened = state;
			} 
		} else {
			if (hackTerminalOpened) {
				hackPanelAnimation[panelAnimation].speed = -1; 
				hackPanelAnimation[panelAnimation].time=hackPanelAnimation[panelAnimation].length;
				hackPanelAnimation.Play(panelAnimation);
				panelClosed = true;
			}
		}
	}
	//get a random character
	string randomChar(){
		byte value = (byte)UnityEngine.Random.Range(41f,128f);
		string c = System.Text.Encoding.ASCII.GetString(new byte[]{value});
		return c;
	}
	//a button is pressed, so check if the button direction matches with the direction of the arrow
	void checkButton(int pressedButton){
		if (order [currentButton] == pressedButton) {
			currentButton++;
			iconsImage[currentButton-1].color=Color.white;
			//the buttons have been pressed in the correct order, so the device is hacked
			if (currentButton == icons.Length) {
				setHackText("Hacked Successfully!");
				if (objectToHack.GetComponent<accessTerminal> ()) {
					currentPasswordText.text = objectToHack.GetComponent<accessTerminal> ().code;
				}
				if(objectToHack.GetComponent<enemyHackPanel>()){
					objectToHack.SendMessage("hackResult",true);
				}
				disableHack(true);
				changeColor=true;
				hacked = true;
			}
			return;
		} 
		//incorrect order, activate alarm
		if(pressedButton==-1 || order [currentButton] != pressedButton){
			setHackText("Hack failed!");
			if (objectToHack.GetComponent<enemyHackPanel> ()) {
				objectToHack.SendMessage ("hackResult", false);
				disableHack (false);
			} else {
				disableHack(true);
			}
		}
	}
	void setHackText(string text){
		finalText=text;
		hackText.GetComponent<Text> ().text = "";
		typeRate = typeTime/(float)finalText.Length;
	}
	//get the rawimages inside the hack HUD
	void getElements(){
		hackHud.gameObject.SetActive (true);
		Component[] components=hackHud.transform.GetComponentsInChildren(typeof(RawImage));
		foreach (Component c in components){
			iconsImage.Add (c.GetComponent<RawImage>());
		}
		hackHud.gameObject.SetActive (false);
	}

	public void hasMoveCamerToDevice(){
		objectToHackHasMoveCameraToDevice = true;
		GetComponent<moveCameraToDevice> ().hasSecondMoveCameraToDevice ();
	}
}