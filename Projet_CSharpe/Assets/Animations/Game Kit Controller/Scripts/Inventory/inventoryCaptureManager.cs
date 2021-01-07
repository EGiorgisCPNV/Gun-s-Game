using UnityEngine;
using System.Collections;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;


public class inventoryCaptureManager : EditorWindow
{
	public Vector2 captureResolution = new Vector2 (1024, 1024);
	public Vector3 rotationOffset;
	public Vector3 positionOffset;
	public string fileName = "New Capture";
	bool checkCapturePath;
	string currentSaveDataPath;
	GameObject player;
	inventoryManager inventory;
	Camera inventoryCamera;
	static inventoryCaptureManager window;
	inventoryManager.inventoryInfo currentInventoryInfo;
	GameObject currentInventoryObjectMesh;
	Rect renderRect;
	Transform inventoryLookObjectTransform;
	RenderTexture originalRenderTexture;
	Texture2D captureFile;


	static void ShowWindow ()
	{
		window = (inventoryCaptureManager)EditorWindow.GetWindow (typeof(inventoryCaptureManager));
		window.init ();
	}

	public void init ()
	{
		inventoryCamera = GameObject.Find ("inventoryCamera").GetComponent<Camera> ();
		player = GameObject.FindGameObjectWithTag ("Player");
		inventory = player.GetComponent<inventoryManager> ();
		inventoryLookObjectTransform = inventory.lookObjectsPosition;
		captureFile = null;
		checkCapturePath = false;
	}
	public void OnDisable(){
		if (currentInventoryObjectMesh) {
			DestroyImmediate (currentInventoryObjectMesh);
		}
	}

	void OnGUI ()
	{
		if (window == null) {
			window = (inventoryCaptureManager)EditorWindow.GetWindow (typeof(inventoryCaptureManager));
		}
		GUILayout.Label ("Inventory Object Capture Tool", EditorStyles.boldLabel);
		captureResolution = EditorGUILayout.Vector2Field ("Capture Resolution", captureResolution);
		fileName = EditorGUILayout.TextField ("File Name", fileName);
		rotationOffset = EditorGUILayout.Vector3Field ("Rotation Offset", rotationOffset);
		GUILayout.Label ("Position Offset", EditorStyles.boldLabel);
		positionOffset.x = EditorGUILayout.Slider (positionOffset.x, -5, 5);
		positionOffset.y = EditorGUILayout.Slider (positionOffset.y, -5, 5);
		positionOffset.z = EditorGUILayout.Slider (positionOffset.z, -5, 5);
		if (currentInventoryObjectMesh) {
			currentInventoryObjectMesh.transform.localRotation = Quaternion.Euler (rotationOffset);
			currentInventoryObjectMesh.transform.localPosition = positionOffset;
		}
		if (inventoryCamera) {       
			inventoryCamera.Render ();
			renderRect = new Rect (position.width / 4, 220, position.width / 2, position.height / 2);
			GUI.DrawTexture (renderRect, inventoryCamera.targetTexture);       
		}
		if (GUILayout.Button ("Get Capture")) {
			getCapture ();
		}
	}
	public void getCapture ()
	{
		if (fileName == "") {
			fileName = currentInventoryInfo.Name;
		}
		originalRenderTexture = inventoryCamera.targetTexture;
		inventoryCamera.targetTexture = new RenderTexture ((int)captureResolution.x, (int)captureResolution.y, 24);
		RenderTexture rendText = RenderTexture.active;
		RenderTexture.active = inventoryCamera.targetTexture;

		// render the texture
		inventoryCamera.Render ();
		// create a new Texture2D with the camera's texture, using its height and width
		Texture2D cameraImage = new Texture2D ((int)captureResolution.x, (int)captureResolution.y, TextureFormat.RGB24, false);
		cameraImage.ReadPixels (new Rect (0, 0, (int)captureResolution.x, (int)captureResolution.y), 0, 0);
		cameraImage.Apply ();
		RenderTexture.active = rendText;
		// store the texture into a .PNG file
		byte[] bytes = cameraImage.EncodeToPNG ();
		// save the encoded image to a file
		System.IO.File.WriteAllBytes (currentSaveDataPath + (fileName + " (Inventory Capture).png"), bytes);
		inventoryCamera.targetTexture = originalRenderTexture;
	
		AssetDatabase.Refresh ();
		checkCapturePath = true;
	}

	void Update(){
		if (checkCapturePath) {
			captureFile = (Texture2D)AssetDatabase.LoadAssetAtPath ((currentSaveDataPath + fileName + " (Inventory Capture).png"), typeof(Texture2D));
			if (captureFile) {
				inventory.setInventoryCaptureIcon (currentInventoryInfo, captureFile);
				checkCapturePath = false;
				closeWindow ();
			}
		}
	}
	public void closeWindow(){
		if (currentInventoryObjectMesh) {
			DestroyImmediate (currentInventoryObjectMesh);
		}
		window.Close ();
	}

	public void setCurrentInventoryObjectInfo (inventoryManager.inventoryInfo info, string savePath)
	{
		currentInventoryInfo = info;
		currentInventoryObjectMesh = (GameObject)Instantiate (info.inventoryGameObject, inventoryLookObjectTransform.position, Quaternion.identity);
		currentInventoryObjectMesh.transform.SetParent (inventoryLookObjectTransform);
		currentSaveDataPath = savePath;
	}
}
#endif