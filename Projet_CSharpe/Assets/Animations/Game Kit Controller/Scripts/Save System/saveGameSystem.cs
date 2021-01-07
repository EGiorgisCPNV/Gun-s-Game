using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class saveGameSystem : MonoBehaviour
{
	[HideInInspector] public saveStationInfo saveInfo;
	public int saveStationId;
	public Transform saveStationPosition;
	public bool usingSaveStation;
	public GameObject saveMenu;
	public string animationName;
	public Image saveButton;
	public Image loadButton;
	public Image deleteButton;
	public Scrollbar scrollBar;
	public GameObject saveGameList;
	public Color disableButtonsColor;
	public Camera photoCapturer;
	public Vector2 captureResolution;
	Animation stationAnimation;
	List<buttonInfo> saveGameListElements = new List<buttonInfo> ();
	int i, j;
	Button currentButton;
	Color originalColor;
	gameManager gameManagerComponent;
	int currentButtonIndex;
	public bool canSave;
	public bool canLoad;
	public bool canDelete;
	string currentSaveDataPath;
	string currentSaveDataName;

	void Start ()
	{
		gameManagerComponent = transform.parent.GetComponent<gameManager> ();
		currentSaveDataPath = gameManagerComponent.getDataPath ();
		currentSaveDataName = gameManagerComponent.getDataName ();
		saveMenu.SetActive (true);

		Component component = saveGameList.GetComponentInChildren (typeof(saveGameSlot));
		GameObject slotPrefab = component.gameObject;

		saveGameListElements.Add (slotPrefab.GetComponent<saveGameSlot>().buttonInfo);
		int slotsAmount = gameManagerComponent.slotBySaveStation - 1;
		for (i = 0; i <slotsAmount; i++) {	
			GameObject newSlotPrefab = (GameObject)Instantiate (slotPrefab, slotPrefab.transform.position, slotPrefab.transform.rotation);
			newSlotPrefab.transform.SetParent (slotPrefab.transform.parent);
			newSlotPrefab.transform.localScale = Vector3.one;
			newSlotPrefab.name = "saveGameSlot_" + (i + 2).ToString ();
			saveGameListElements.Add (newSlotPrefab.GetComponent<saveGameSlot>().buttonInfo);
		}

		scrollBar.value = 1;
		saveMenu.SetActive (false);
		stationAnimation = GetComponent<Animation> ();
		saveInfo.saveStationPositionX = saveStationPosition.position.x;
		saveInfo.saveStationPositionY = saveStationPosition.position.y;
		saveInfo.saveStationPositionZ = saveStationPosition.position.z;
		saveInfo.saveStationRotationX = saveStationPosition.eulerAngles.x;
		saveInfo.saveStationRotationY = saveStationPosition.eulerAngles.y;
		saveInfo.saveStationRotationZ = saveStationPosition.eulerAngles.z;
		saveInfo.saveStationScene = SceneManager.GetActiveScene ().buildIndex;
		originalColor = saveButton.color;
		changeButtonsColor (false, false, false);
	}

	public void activateDevice ()
	{
		usingSaveStation = !usingSaveStation;
		GetComponent<moveCameraToDevice> ().moveCamera (usingSaveStation);
		if (usingSaveStation) {
			loadStates ();
			saveMenu.SetActive (true);
			stationAnimation.Stop ();
			stationAnimation [animationName].speed = 1;
			stationAnimation.Play (animationName);
		} else {
			saveMenu.SetActive (false);
			stationAnimation.Stop ();
			stationAnimation [animationName].speed = -1;
			stationAnimation [animationName].time = stationAnimation [animationName].length;
			stationAnimation.Play (animationName);
			changeButtonsColor (false, false, false);
		}
	}

	public void getSaveButtonSelected (Button button)
	{
		currentButtonIndex = -1;
		bool save = false;
		bool load_delete = false;
		for (i = 0; i < saveGameListElements.Count; i++) {		
			if (saveGameListElements [i].button == button) {
				currentButtonIndex = i;	
				currentButton = button;
				save = true;
				if (saveGameListElements [i].infoAdded) {
					load_delete = true;
				}
			}
		}
		changeButtonsColor (true, save, load_delete);
	}

	public void saveGame ()
	{
		if (currentButton && canSave) {
			int index = -1;
			for (i = 0; i < saveGameListElements.Count; i++) {
				if (saveGameListElements [i].button == currentButton) {
					saveGameListElements [i].infoAdded = true;
					index = i;	
				}
			}
			bool saveLocated = false;
			saveStationInfo newSave = saveInfo;
			List<saveStationInfo> saveList = loadFile ();
		
			for (j = 0; j < saveList.Count; j++) {
				if (saveList [j].saveNumber - 1 == index) {
					newSave = saveList [j];
					saveLocated = true;
					print ("save encontrado");
				}
			}

			newSave.saveStationPositionX = saveInfo.saveStationPositionX;
			newSave.saveStationPositionY = saveInfo.saveStationPositionY;
			newSave.saveStationPositionZ = saveInfo.saveStationPositionZ;
			newSave.saveStationRotationX = saveInfo.saveStationRotationX;
			newSave.saveStationRotationY = saveInfo.saveStationRotationY;
			newSave.saveStationRotationZ = saveInfo.saveStationRotationZ;
			if (!saveLocated) {
				print ("save nuevo");
				newSave.playTime = gameManagerComponent.playTime;
			} else {
				newSave.playTime += gameManagerComponent.playTime;
			}
			gameManagerComponent.playTime = 0;
			newSave.chapterNumberAndName = gameManagerComponent.chapterInfo;
			newSave.saveNumber = index + 1;
			newSave.saveDate = System.DateTime.Now;
			if (!saveLocated) {
				saveList.Add (newSave);
			}
			saveCameraView (newSave.saveNumber.ToString ());
			//	showSaveList (saveList);

			saveGameListElements [index].icon.enabled = true;
			byte[] bytes = File.ReadAllBytes (currentSaveDataPath + (currentSaveDataName + "_" + newSave.saveNumber.ToString () + ".png"));
			Texture2D texture = new Texture2D ((int)captureResolution.x, (int)captureResolution.y);
			texture.filterMode = FilterMode.Trilinear;
			texture.LoadImage (bytes);
			saveGameListElements [index].icon.texture = texture;
			saveGameListElements [index].chapterName.text = newSave.chapterNumberAndName;
			saveGameListElements [index].playTime.text = convertSecondsIntoHours (newSave.playTime);
			saveGameListElements [index].saveNumber.text = "Save " + newSave.saveNumber.ToString ();
			saveGameListElements [index].saveDate.text = String.Format ("{0:dd/MM/yy}", newSave.saveDate);
			saveGameListElements [index].saveHour.text = System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute;
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (currentSaveDataPath + currentSaveDataName + ".txt"); 
			bf.Serialize (file, saveList);
			file.Close ();
			changeButtonsColor (false, false, false);
		}
	}

	public void loadGame ()
	{
		if (currentButton && canLoad) {
			saveStationInfo newSave = saveInfo;
			List<saveStationInfo> saveList = loadFile ();

			for (j = 0; j < saveList.Count; j++) {
				if (saveList [j].saveNumber - 1 == currentButtonIndex) {
					newSave = saveList [j];
					print ("save cargado");
				}
			}
			gameManagerComponent.getPlayerPrefsInfo (newSave);
		}
	}

	public void deleteGame ()
	{
		if (currentButton && canDelete) {
			bool saveLocated = false;
			saveStationInfo newSave = saveInfo;
			List<saveStationInfo> saveList = loadFile ();

			for (j = 0; j < saveList.Count; j++) {
				if (saveList [j].saveNumber - 1 == currentButtonIndex) {
					newSave = saveList [j];
					saveLocated = true;
					print ("save eliminado");
				}
			}
			if (File.Exists (currentSaveDataPath + (currentSaveDataName + "_" + newSave.saveNumber.ToString () + ".png"))) {
				File.Delete (currentSaveDataPath + (currentSaveDataName + "_" + newSave.saveNumber.ToString () + ".png"));
			}
			if (saveLocated) {
				saveList.Remove (newSave);
			}

			//	showSaveList (saveList);
			saveGameListElements [currentButtonIndex].icon.enabled = false;
			saveGameListElements [currentButtonIndex].chapterName.text = "Chapter -";
			saveGameListElements [currentButtonIndex].saveNumber.text = "Save -";
			saveGameListElements [currentButtonIndex].playTime.text = "--:--:--";
			saveGameListElements [currentButtonIndex].saveDate.text = "--/--/--";
			saveGameListElements [currentButtonIndex].saveHour.text = "--:--";
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (currentSaveDataPath + currentSaveDataName + ".txt"); 
			bf.Serialize (file, saveList);
			file.Close ();
			changeButtonsColor (false, false, false);
		}
	}

	public void changeButtonsColor (bool state, bool save, bool load_delete)
	{
		if (save) {
			saveButton.color = originalColor;
		} else {
			saveButton.color = disableButtonsColor;
		}
		if (load_delete) {
			loadButton.color = originalColor;
			deleteButton.color = originalColor;
		} else {
			loadButton.color = disableButtonsColor;
			deleteButton.color = disableButtonsColor;
		}
		canSave = save;
		canLoad = load_delete;
		canDelete = load_delete;
		if (!state) {
			currentButton = null;
		}
	}

	public void loadStates ()
	{
		List<saveStationInfo> saveList = loadFile ();
		//showSaveList (saveList);
		for (i = 0; i < saveGameListElements.Count; i++) {
			for (j = 0; j < saveList.Count; j++) {
				if (saveList [j].saveNumber - 1 == i) {
					saveGameListElements [i].icon.enabled = true;
					byte[] bytes = File.ReadAllBytes (currentSaveDataPath + (currentSaveDataName + "_" + saveList [j].saveNumber.ToString () + ".png"));
					Texture2D texture = new Texture2D ((int)captureResolution.x, (int)captureResolution.y);
					texture.filterMode = FilterMode.Trilinear;
					texture.LoadImage (bytes);
					saveGameListElements [i].icon.texture = texture;
					saveGameListElements [i].chapterName.text = saveList [j].chapterNumberAndName;
					saveGameListElements [i].saveNumber.text = "Save " + saveList [j].saveNumber.ToString ();
					saveGameListElements [i].playTime.text = convertSecondsIntoHours (saveList [j].playTime);
					saveGameListElements [i].saveDate.text = String.Format ("{0:dd/MM/yy}", saveList [j].saveDate);
					saveGameListElements [i].saveHour.text = saveList [j].saveDate.Hour + ":" + System.DateTime.Now.Minute;
					saveGameListElements [i].infoAdded = true;
				}
			}
		}
		for (i = 0; i < saveGameListElements.Count; i++) {
			if (!saveGameListElements [i].infoAdded) {
				saveGameListElements [i].icon.enabled = false;
				saveGameListElements [i].chapterName.text = "Chapter -";
				saveGameListElements [i].saveNumber.text = "Save -";
				saveGameListElements [i].playTime.text = "--:--:--";
				saveGameListElements [i].saveDate.text = "--/--/--";
				saveGameListElements [i].saveHour.text = "--:--";
			}
		}
	}

	void saveCameraView (string saveNumber)
	{
		// get the camera's render texture
		photoCapturer.enabled = true;
		photoCapturer.targetTexture = new RenderTexture ((int)captureResolution.x, (int)captureResolution.y, 24);
		RenderTexture rendText = RenderTexture.active;
		RenderTexture.active = photoCapturer.targetTexture;

		// render the texture
		photoCapturer.Render ();
		// create a new Texture2D with the camera's texture, using its height and width
		Texture2D cameraImage = new Texture2D ((int)captureResolution.x, (int)captureResolution.y, TextureFormat.RGB24, false);
		cameraImage.ReadPixels (new Rect (0, 0, (int)captureResolution.x, (int)captureResolution.y), 0, 0);
		cameraImage.Apply ();
		RenderTexture.active = rendText;
		// store the texture into a .PNG file
		byte[] bytes = cameraImage.EncodeToPNG ();
		// save the encoded image to a file
		System.IO.File.WriteAllBytes (currentSaveDataPath + (currentSaveDataName + "_" + saveNumber + ".png"), bytes);
		photoCapturer.enabled = false;
	}

	string convertSecondsIntoHours (float value)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds (value);
		string timeText = string.Format ("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		return timeText;
	}

	public List<saveStationInfo> loadFile ()
	{
		List<saveStationInfo> saveList = new List<saveStationInfo> ();
		if (File.Exists (currentSaveDataPath + currentSaveDataName + ".txt")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (currentSaveDataPath + currentSaveDataName + ".txt", FileMode.Open);
			saveList = (List<saveStationInfo>)bf.Deserialize (file);
			file.Close ();	
		}
		return saveList;
	}

	public void setStationIde (int idValue)
	{
		saveInfo.id = idValue;
	}

	public void showSaveList (List<saveStationInfo> saveList)
	{
		for (i = 0; i < saveList.Count; i++) {
			print ("SAVE " + (i + 1));
			print ("Chapter " + saveList [i].chapterNumberAndName);
			print ("Position " + saveList [i].saveStationPositionX + " " + saveList [i].saveStationPositionY + " " + saveList [i].saveStationPositionZ);
			print ("Scene Index " + saveList [i].saveStationScene);
			print ("Id " + saveList [i].id);
			print ("Save Number " + saveList [i].saveNumber);
			print ("PlayTime " + saveList [i].playTime);
			print ("Date " + saveList [i].saveDate);
			print ("Hour " + saveList [i].saveDate.Hour +":"+saveList [i].saveDate.Minute);
			print ("\n");
		}
	}

	[System.Serializable]
	public class saveStationInfo
	{
		public string chapterNumberAndName;
		public float saveStationPositionX;
		public float saveStationPositionY;
		public float saveStationPositionZ;
		public float saveStationRotationX;
		public float saveStationRotationY;
		public float saveStationRotationZ;
		public int saveStationScene;
		public int id;
		public int saveNumber;
		public float playTime;
		public DateTime saveDate;
	}

	[System.Serializable]
	public class buttonInfo
	{
		public Button button;
		public RawImage icon;
		public Text chapterName;
		public Text playTime;
		public Text saveNumber;
		public Text saveDate;
		public Text saveHour;
		public bool infoAdded;
	}
}