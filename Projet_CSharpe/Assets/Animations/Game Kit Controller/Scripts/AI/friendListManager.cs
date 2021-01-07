using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class friendListManager : MonoBehaviour
{
	public bool friendManagerEnabled;
	public GameObject friendsMenuContent;
	public GameObject friendListContent;
	public GameObject friendListElement;
	public Button attackButton;
	public Button followButton;
	public Button waitButton;
	public Button hideButton;
	public bool menuOpened;
	public List<friendInfo> friendsList = new List<friendInfo> ();
	List<GameObject> closestEnemyList = new List<GameObject> ();
	menuPause pauseManager;
	inputManager input;
	int i, j;
	AIHidePositionsManager hidePositionsManager;

	void Start ()
	{
		input = transform.parent.GetComponent<inputManager> ();
		pauseManager = transform.parent.GetComponent<menuPause> ();
		friendListElement.SetActive (false);
		friendsMenuContent.SetActive (false);
		GameObject hidePositions = GameObject.Find ("AIHidePositionsManager");
		if (hidePositions && hidePositions.GetComponent<AIHidePositionsManager> ()) {
			hidePositionsManager = hidePositions.GetComponent<AIHidePositionsManager> ();
		}
	}

	void Update ()
	{
		if (friendManagerEnabled) {
			if (input.checkInputButton ("Friend Menu", inputManager.buttonType.getKeyDown)) {
				openOrCloseFriendMenu (!menuOpened);
			}
		}
	}

	public void openOrCloseFriendMenu (bool state)
	{
		if ((!pauseManager.playerMenuActive || menuOpened) && !pauseManager.usingDevice && !pauseManager.pauseGame) {
			menuOpened = state;
			pauseManager.openOrClosePlayerMenu (menuOpened);
			friendsMenuContent.SetActive (menuOpened);
			//set to visible the cursor
			pauseManager.showOrHideCursor (menuOpened);
			//disable the touch controls
			pauseManager.checkTouchControls (!menuOpened);
			//disable the camera rotation
			pauseManager.changeCameraState (!menuOpened);
			GetComponent<playerController> ().changeScriptState (!menuOpened);
			pauseManager.usingSubMenuState (menuOpened);
		}
	}

	public void openOrCLoseFriendMenuFromTouch ()
	{
		openOrCloseFriendMenu (!menuOpened);
	}

	public void addFriend (GameObject friend)
	{
		if (!checkIfContains (friend.transform)) {
			GameObject newFriendListElement = (GameObject)Instantiate (friendListElement, friendListElement.transform.position, Quaternion.identity);
			newFriendListElement.name = "newFriendListElement_"+(friendsList.Count + 1).ToString ();
			friendInfo newFriend = newFriendListElement.GetComponent<friendListElement>().friendListElementInfo;
			newFriend.Name = friend.GetComponent<health> ().settings.allyName;
			newFriend.friendTransform = friend.transform;
			newFriendListElement.SetActive (true);
			newFriendListElement.transform.SetParent (friendListElement.transform.parent);
			newFriendListElement.transform.localScale = Vector3.one;
			newFriend.friendListElement = newFriendListElement;
			if (canAIAttack (friend)) {
				newFriend.attackButton.onClick.AddListener (() => {
					setIndividualOrder (newFriend.attackButton);
				});
			} else {
				newFriend.attackButton.gameObject.SetActive (false);
			}
			newFriend.followButton.onClick.AddListener (() => {
				setIndividualOrder (newFriend.followButton);
			});
			newFriend.waitButton.onClick.AddListener (() => {
				setIndividualOrder (newFriend.waitButton);
			});
			newFriend.hideButton.onClick.AddListener (() => {
				setIndividualOrder (newFriend.hideButton);
			});
			friendsList.Add (newFriend);
			setCurrentStateText (friendsList.Count - 1, "Following");
			setFriendListName ();
		}
	}

	public void setFriendListName(){
		for (i = 0; i < friendsList.Count; i++) {
			friendsList[i].nameText.text = (i+1).ToString () + ".- " + friendsList[i].Name;
		}
	}

	public bool checkIfContains (Transform friend)
	{
		bool itContains = false;
		for (i = 0; i < friendsList.Count; i++) {
			if (friendsList [i].friendTransform == friend) {
				itContains = true;
			}
		}
		return itContains;
	}

	public void setIndividualOrder (Button pressedButton)
	{
		for (i = 0; i < friendsList.Count; i++) {
			if (friendsList [i].attackButton == pressedButton) {
				if (canAIAttack (friendsList [i].friendTransform.gameObject)) {
					//print ("attack");
					Transform closestEnemy = getClosestEnemy ();
					if (closestEnemy) {
						friendsList [i].friendTransform.SendMessage ("attack", closestEnemy, SendMessageOptions.DontRequireReceiver);
					}
					setCurrentStateText (i, "Attacking");
				}
			} else if (friendsList [i].followButton == pressedButton) {
				//print ("follow");
				friendsList [i].friendTransform.SendMessage ("follow", transform, SendMessageOptions.DontRequireReceiver);
				setCurrentStateText (i, "Following");
			} else if (friendsList [i].waitButton == pressedButton) {
				//print ("wait");
				friendsList [i].friendTransform.SendMessage ("wait", transform, SendMessageOptions.DontRequireReceiver);
				setCurrentStateText (i, "Waiting");
			} else if (friendsList [i].hideButton == pressedButton) {
				//print ("hide");
				friendsList [i].friendTransform.SendMessage ("hide", getClosestHidePosition (friendsList [i].friendTransform), SendMessageOptions.DontRequireReceiver);
				setCurrentStateText (i, "Hiding");
			}
		}
	}

	public void setGeneralOrder (Button pressedButton)
	{
		Transform target = transform;
		string action = "";
		if (attackButton == pressedButton) {
			//print ("attack");
			action = "attack";
			target = getClosestEnemy ();
		} else if (followButton == pressedButton) {
			//print ("follow");
			action = "follow";
		} else if (waitButton == pressedButton) {
			//print ("wait");
			action = "wait";
		} else if (hideButton == pressedButton) {
			//print ("hide");
			action = "hide";
		}
		for (i = 0; i < friendsList.Count; i++) {
			bool canDoAction = true;
			if (action == "attack") {
				if (!canAIAttack (friendsList [i].friendTransform.gameObject)) {
					canDoAction = false;
				}
			}
			if (action == "hide") {
				target = getClosestHidePosition (friendsList [i].friendTransform);
			}
			if (canDoAction) {
				friendsList [i].friendTransform.SendMessage (action, target, SendMessageOptions.DontRequireReceiver);
				switch (action) {
				case "attack":
					setCurrentStateText (i, "Attacking");
					break;
				case "follow":
					setCurrentStateText (i, "Following");
					break;
				case "wait":
					setCurrentStateText (i, "Waiting");
					break;
				case "hide":
					setCurrentStateText (i, "Hiding");
					break;
				}
			}
		}
	}

	public bool canAIAttack (GameObject AIFriend)
	{
		bool canAttack = false;
		if (AIFriend.GetComponent<findObjectivesSystem> ().attackType != findObjectivesSystem.AIAttackType.none) {
			canAttack = true;
		}
		return canAttack;
	}

	public void setCurrentStateText (int index, string state)
	{
		friendsList [index].currentState.text = "State: " + state;
	}

	public Transform getClosestEnemy ()
	{
		GameObject closestEnemy;
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("enemy");
		closestEnemyList.Clear ();
		for (j = 0; j < enemies.Length; j++) {
			if (!enemies [j].GetComponent<health> ().dead) {
				closestEnemyList.Add (enemies [j]);
			}
		}
		if (closestEnemyList.Count > 0) {
			float distance = Mathf.Infinity;
			int index = -1;
			for (j = 0; j < closestEnemyList.Count; j++) {
				float currentDistance = Vector3.Distance (closestEnemyList [j].transform.position, transform.position);
				if (currentDistance < distance) {
					distance = currentDistance;
					index = j;
				}
			}
			if (index != -1) {
				closestEnemy = closestEnemyList [index];
				return closestEnemy.transform;
			}
		}
		return null;
	}

	public Transform getClosestHidePosition (Transform AIFriend)
	{
		if (hidePositionsManager) {
			if (hidePositionsManager.hidePositionList.Count > 0) {
				float distance = Mathf.Infinity;
				int index = -1;
				for (j = 0; j < hidePositionsManager.hidePositionList.Count; j++) {
					float currentDistance = Vector3.Distance (AIFriend.position, hidePositionsManager.hidePositionList [j].position);
					if (currentDistance < distance) {
						distance = currentDistance;
						index = j;
					}
				}
				return hidePositionsManager.hidePositionList [index];
			}
		}
		return null;
	}

	public void removeFriend (Transform friend)
	{
		for (i = 0; i < friendsList.Count; i++) {
			if (friendsList [i].friendTransform == friend) {
				Destroy (friendsList [i].friendListElement);
				friendsList.RemoveAt (i);
				return;
			}
		}
	}

	[System.Serializable]
	public class friendInfo
	{
		public string Name;
		public Transform friendTransform;
		public Text nameText;
		public Text currentState;
		public GameObject friendListElement;
		public Button attackButton;
		public Button followButton;
		public Button waitButton;
		public Button hideButton;
	}
}