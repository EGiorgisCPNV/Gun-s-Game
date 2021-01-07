using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;
public class gameManager : MonoBehaviour {
	public float playTime;
	public string chapterInfo;
	public bool loadEnabled;
	public bool useRelativePath;
	public string relativePath;
	public string saveFileName;
	public int slotBySaveStation;
	GameObject player;
	GameObject pCamera;
	saveGameSystem saveGameInfo;
	int i,j;
	RaycastHit hit;
	public LayerMask layer;

	void Start () {
		//print (Application.persistentDataPath);
		if (loadEnabled) {
			if (PlayerPrefs.HasKey ("chapterInfo")) {
				chapterInfo = PlayerPrefs.GetString ("chapterInfo");
			}
			if (PlayerPrefs.HasKey ("loadingGame")) {
				if (PlayerPrefs.GetInt ("loadingGame") == 1) {
					player = GameObject.Find ("Player Controller");
					pCamera = GameObject.Find ("Player Camera");
					Vector3 newPosition = new Vector3 (PlayerPrefs.GetFloat ("saveStationPositionX"), PlayerPrefs.GetFloat ("saveStationPositionY"), PlayerPrefs.GetFloat ("saveStationPositionZ"));
					Quaternion newRotation = Quaternion.Euler (PlayerPrefs.GetFloat ("saveStationRotationX"), PlayerPrefs.GetFloat ("saveStationRotationY"), PlayerPrefs.GetFloat ("saveStationRotationZ"));
					if (Physics.Raycast (newPosition, -Vector3.up, out hit, Mathf.Infinity, layer)) {
						newPosition = hit.point;
					}
					player.transform.position = newPosition;
					pCamera.transform.position = newPosition;
					player.transform.rotation = newRotation;
					pCamera.transform.rotation = newRotation;
					PlayerPrefs.SetInt ("loadingGame", 0);
				}
			}
		} else {
			PlayerPrefs.SetInt ("loadingGame", 0);
		}
	}
	void Update () {
		playTime += Time.deltaTime;
	}

	public void getPlayerPrefsInfo(saveGameSystem.saveStationInfo save){
		PlayerPrefs.SetInt ("loadingGame",1);
		PlayerPrefs.SetInt ("currentSaveStationId",save.id);
		PlayerPrefs.SetFloat ("saveStationPositionX", save.saveStationPositionX);
		PlayerPrefs.SetFloat ("saveStationPositionY", save.saveStationPositionY);
		PlayerPrefs.SetFloat ("saveStationPositionZ", save.saveStationPositionZ);
		SceneManager.LoadScene (save.saveStationScene);
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
	public string getDataName(){
		return saveFileName;
	}
}
