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
public class homeMenu : MonoBehaviour {
	public GameObject mainMenuWindow;
	public GameObject loadGameWindow;
	public GameObject exitWindow;
	public GameObject saveListContent;
	public Image loadButton;
	public Image deleteButton;
	public Color disableButtonsColor;
	public Scrollbar scrollBar;
	public bool useRelativePath;
	public string relativePath;
	public string saveFileName;
	bool loadGameWindowOpened;
	bool exitWindownOpened;
	List<saveGameSystem.buttonInfo> saveGameListElements =new List<saveGameSystem.buttonInfo>();
	int i,j;
	Button currentButton;
	Color originalColor;
	int currentButtonIndex;
	public bool canLoad;
	public bool canDelete;
	string currentSaveDataPath;

	void Start () {
		currentSaveDataPath = getDataPath ();
		print (currentSaveDataPath);
		loadGameWindow.SetActive (true);
		Component[] components=saveListContent.GetComponentsInChildren(typeof(LayoutElement));
		foreach (Component c in components)	{
			saveGameSystem.buttonInfo newButtonInfo=new saveGameSystem.buttonInfo();
			newButtonInfo.button = c.GetComponentInChildren<Button> ();
			newButtonInfo.icon = c.GetComponentInChildren<RawImage> ();
			newButtonInfo.chapterName = c.transform.GetChild (2).GetComponent<Text> ();
			newButtonInfo.playTime = c.transform.GetChild (3).GetComponent<Text> ();
			newButtonInfo.saveNumber = c.transform.GetChild (4).GetComponent<Text> ();
			newButtonInfo.saveDate= c.transform.GetChild (5).GetComponent<Text> ();
			saveGameListElements.Add (newButtonInfo);
		}
		scrollBar.value = 1;
		loadGameWindow.SetActive (false);
		loadStates ();
		originalColor = loadButton.color;
		changeButtonsColor (false,false);
	}
	void Update () {
	
	}
	public void getSaveButtonSelected(Button button){
		currentButtonIndex = -1;
		bool load_delete = false;
		for (i = 0; i < saveGameListElements.Count; i++) {
			if (saveGameListElements [i].button == button && saveGameListElements [i].infoAdded) {
				currentButtonIndex = i;	
				currentButton = button;
				load_delete = true;
			}
		}
		changeButtonsColor (true,load_delete);
	}
	public void continueGame(){
		saveGameSystem.saveStationInfo recentSave = new saveGameSystem.saveStationInfo();
		List<saveGameSystem.saveStationInfo> saveList = loadFile ();
		long closestDate = 0;
		for (j = 0; j < saveList.Count; j++) {
			//print (saveList [j].saveDate.Ticks);
			if (saveList[j].saveDate.Ticks>closestDate) {
				closestDate =saveList [j].saveDate.Ticks;
				recentSave = saveList [j];
			}
		}
		//print ("mas reciente" + recentSave.saveDate+" "+recentSave.saveNumber);
		loadScene (recentSave);
	}

	public void openLoadWindow(){
		loadGameWindowOpened = !loadGameWindowOpened;
		loadGameWindow.SetActive (loadGameWindowOpened);
		mainMenuWindow.SetActive (!loadGameWindowOpened);
	}
	public void loadGame(){
		if (currentButton && canLoad) {
			saveGameSystem.saveStationInfo newSave = new saveGameSystem.saveStationInfo();
			List<saveGameSystem.saveStationInfo> saveList = loadFile ();
			for (j = 0; j < saveList.Count; j++) {
				print (saveList [j].saveNumber + " " + saveList [j].saveStationPositionX + " " + saveList [j].saveStationPositionY + " " + saveList [j].saveStationPositionZ);
				if (saveList [j].saveNumber - 1 == currentButtonIndex) {
					newSave = saveList [j];
					print ("save cargado");
				}
			}
			print (newSave.saveNumber);
			loadScene (newSave);
		}
	}
	public void loadScene(saveGameSystem.saveStationInfo newSave){
		PlayerPrefs.SetInt ("loadingGame", 1);
		PlayerPrefs.SetInt ("currentSaveStationId", newSave.id);
		PlayerPrefs.SetFloat ("saveStationPositionX", newSave.saveStationPositionX);
		PlayerPrefs.SetFloat ("saveStationPositionY", newSave.saveStationPositionY);
		PlayerPrefs.SetFloat ("saveStationPositionZ", newSave.saveStationPositionZ);
		PlayerPrefs.SetFloat ("saveStationRotationX", newSave.saveStationRotationX);
		PlayerPrefs.SetFloat ("saveStationRotationY", newSave.saveStationRotationY);
		PlayerPrefs.SetFloat ("saveStationRotationZ", newSave.saveStationRotationZ);

		SceneManager.LoadScene (newSave.saveStationScene);
	}
	public void deleteGame(){
		if (currentButton && canDelete) {
			bool saveLocated = false;
			saveGameSystem.saveStationInfo newSave = new saveGameSystem.saveStationInfo();
			List<saveGameSystem.saveStationInfo> saveList = loadFile ();

			for (j = 0; j < saveList.Count; j++) {
				if (saveList [j].saveNumber - 1 == currentButtonIndex) {
					newSave = saveList [j];
					saveLocated = true;
					print ("save eliminado");
				}
			}
			if(saveLocated) {
				saveList.Remove (newSave);
			}

			if(File.Exists(currentSaveDataPath +(saveFileName + "_" +newSave.saveNumber.ToString()+".png"))){
				File.Delete(currentSaveDataPath +(saveFileName + "_" +newSave.saveNumber.ToString()+".png"));
			}
			saveGameListElements [currentButtonIndex].icon.enabled = false;
			saveGameListElements [currentButtonIndex].chapterName.text = "Chapter -";
			saveGameListElements [currentButtonIndex].saveNumber.text = "Save -";
			saveGameListElements [currentButtonIndex].playTime.text = "--:--:--";
			saveGameListElements [currentButtonIndex].saveDate.text = "--/--/--";
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (currentSaveDataPath + saveFileName +".txt"); 
			bf.Serialize (file, saveList);
			file.Close ();
			changeButtonsColor (false,false);
		}
	}
	public void newGame(){
		PlayerPrefs.SetInt ("loadingGame", 0);
		SceneManager.LoadScene (1);
	}

	public void exit(){
		exitWindownOpened = !exitWindownOpened;
		exitWindow.SetActive (exitWindownOpened);
		mainMenuWindow.SetActive (!exitWindownOpened);
	}

	public void confirmExit(){
		Application.Quit();
	}
	void loadStates() {
		List<saveGameSystem.saveStationInfo> saveList = loadFile ();
		for (i = 0; i < saveGameListElements.Count; i++) {
			for (j = 0; j < saveList.Count; j++) {
				if (saveList [j].saveNumber-1 == i) {
					saveGameListElements [i].icon.enabled = true;
					byte[] bytes = File.ReadAllBytes(currentSaveDataPath +(saveFileName + "_" + saveList[j].saveNumber.ToString()+".png"));
					Texture2D texture = new Texture2D(1024,1024);
					texture.filterMode = FilterMode.Trilinear;
					texture.LoadImage(bytes);
					saveGameListElements [i].icon.texture =texture;
					saveGameListElements [i].chapterName.text = saveList [j].chapterNumberAndName;
					saveGameListElements [i].saveNumber.text = "Save "+saveList [j].saveNumber.ToString();
					saveGameListElements [i].playTime.text = convertSecondsIntoHours (saveList [j].playTime);
					saveGameListElements [i].saveDate.text = String.Format("{0:dd/MM/yy}", saveList [j].saveDate);
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
			}
		}
	}
	string convertSecondsIntoHours(float value){
		TimeSpan timeSpan = TimeSpan.FromSeconds(value);
		string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		return timeText;
	}
	List<saveGameSystem.saveStationInfo> loadFile(){
		List<saveGameSystem.saveStationInfo> saveList =new List<saveGameSystem.saveStationInfo>();
		if (File.Exists (currentSaveDataPath + saveFileName + ".txt")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (currentSaveDataPath + saveFileName + ".txt", FileMode.Open);
			saveList = (List<saveGameSystem.saveStationInfo>)bf.Deserialize (file);
			file.Close ();	
		}
		return saveList;
	}
	public void changeButtonsColor(bool state,bool load_delete){
		if (load_delete) {
			loadButton.color = originalColor;
			deleteButton.color = originalColor;
		} else {
			loadButton.color = disableButtonsColor;
			deleteButton.color = disableButtonsColor;
		}
		canLoad = load_delete;
		canDelete = load_delete;
		if (!state) {
			currentButton = null;
		}
	}
	public string getDataPath ()
	{
		string dataPath = "";
		if (useRelativePath) {
			if (!Directory.Exists (relativePath)) {
				Directory.CreateDirectory (relativePath);
			}
			dataPath = relativePath + "/";
		} else {
			dataPath = Application.persistentDataPath + "/";
		}
		return dataPath;
	}
}