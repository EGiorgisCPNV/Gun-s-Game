using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class waypointPlayerPathSystem : MonoBehaviour {
	public List<wayPointInfo> wayPoints =new List<wayPointInfo>();
	public bool inOrder;
	public bool showOneByOne;
	public bool showGizmo;
	public Color gizmoLabelColor;
	public bool useRegularGizmoRadius;
	public float gizmoRadius;
	public float triggerRadius;
	public bool showOffScreenIcon;
	public bool showMapWindowIcon;
	public bool showDistance;
	public bool pathActive;
	public GameObject objectToActive;
	public string activeFunctionName;
	public bool useTimer;
	public float timerSpeed;
	[Range(0,60)] public float minutesToComplete;
	[Range(0,60)] public float secondsToComplete;
	public float extraTimePerPoint;
	public int pointsReached;
	public AudioClip pathCompleteAudioSound;
	public AudioClip pathUncompleteAudioSound;
	public AudioClip secondTimerSound;
	public float secondSoundTimerLowerThan;
	public AudioClip pointReachedSound;
	public bool useLineRenderer;
	public Color lineRendererColor= Color.yellow;
	public float lineRendererWidth;
	List<GameObject> points =new List<GameObject>();
	GameObject character;
	GameObject player;
	int i;
	int pointsNumber;
	float totalSecondsTimer;
	Text screenTimerText;
	AudioSource audioSource;
	LineRenderer lineRenderer;

	void Start () {
		audioSource = GameObject.Find ("timerAudioSource").GetComponent<AudioSource> ();
		if (inOrder && !showOneByOne && useLineRenderer) {
			gameObject.AddComponent<LineRenderer> ();
			lineRenderer = GetComponent<LineRenderer> ();
			lineRenderer.material = new Material (Shader.Find ("Sprites/Default")) { color = lineRendererColor };
			lineRenderer.SetWidth (lineRendererWidth, lineRendererWidth);
			lineRenderer.SetColors (lineRendererColor, lineRendererColor);
		}
	}
	void Update () {
		if (pathActive) {
			if (useTimer) {
				totalSecondsTimer -= Time.deltaTime * timerSpeed;
				screenTimerText.text = convertSeconds ();
				if (secondTimerSound) {
					if (totalSecondsTimer - 1 <= secondSoundTimerLowerThan && totalSecondsTimer % 1 < 0.1f) {
						audioSource.PlayOneShot (secondTimerSound);
					}
				}
				if (totalSecondsTimer <= 0) {
					stopPath ();
				}
			}
			if (inOrder && !showOneByOne && useLineRenderer) {
				lineRenderer.SetColors (lineRendererColor, lineRendererColor);
				lineRenderer.SetVertexCount (wayPoints.Count-pointsReached);
				for (i = 0; i < wayPoints.Count; i++) {
					if (!wayPoints [i].reached) {
						lineRenderer.SetPosition (i-pointsReached, wayPoints [i].point.position);
					}
				}
			}
		}
	}
	public string convertSeconds(){
		int minutes = Mathf.FloorToInt(totalSecondsTimer / 60F);
		int seconds = Mathf.FloorToInt(totalSecondsTimer - minutes * 60);
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	//add a new waypoint
	public void addNewWayPoint(){
		Vector3 newPosition = transform.position;
		if (wayPoints.Count > 0) {
			newPosition = wayPoints [wayPoints.Count - 1].point.position + wayPoints [wayPoints.Count - 1].point.forward * wayPoints [wayPoints.Count - 1].triggerRadius*3;
		}
		GameObject newWayPoint = new GameObject ();
		newWayPoint.transform.SetParent (transform);
		newWayPoint.transform.position = newPosition;
		newWayPoint.name=(wayPoints.Count+1).ToString();
		wayPointInfo newWayPointInfo = new wayPointInfo ();
		newWayPointInfo.Name = newWayPoint.name;
		newWayPointInfo.point = newWayPoint.transform;
		newWayPointInfo.triggerRadius = triggerRadius;
		newWayPoint.AddComponent<mapObjectInformation> ().setPathElementInfo (showOffScreenIcon, showMapWindowIcon, showDistance);
		newWayPoint.GetComponent<mapObjectInformation> ().enabled = false;
		wayPoints.Add(newWayPointInfo);
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<waypointPlayerPathSystem>());
		#endif
	}
	public void renamePoints(){
		for (i = 0; i < wayPoints.Count; i++) {
			wayPoints [i].Name = (i + 1).ToString ();
			wayPoints [i].point.name = wayPoints [i].Name;
		}
		#if UNITY_EDITOR
		EditorUtility.SetDirty (GetComponent<waypointPlayerPathSystem>());
		#endif
	}
	public void pointReached(Transform point){
		bool pointReachedCorrectly = false;
		if (showOneByOne) {
			if (wayPoints [pointsReached].point == point) {
				wayPoints [pointsReached].reached = true;
				pointReachedCorrectly = true;
				pointsReached++;
				if (pointsReached < pointsNumber) {
					wayPoints [pointsReached].point.GetComponent<mapObjectInformation> ().createMapIconInfo ();
				}
			} else {
				stopPath ();
			}
		} else {
			if (inOrder) {
				if (wayPoints [pointsReached].point == point) {
					wayPoints [pointsReached].reached = true;
					pointReachedCorrectly = true;
					pointsReached++;
				} else {
					stopPath ();
				}
			} else {
				for (i = 0; i < wayPoints.Count; i++) {
					if (wayPoints [i].point == point) {
						wayPoints [i].reached = true;
						pointReachedCorrectly = true;
						pointsReached++;
					}
				}
			}
		}
		if (pointReachedCorrectly) {
			if (pointsReached < pointsNumber && useTimer) {
				totalSecondsTimer += extraTimePerPoint;
			}
			if (pointReachedSound) {
				audioSource.PlayOneShot (pointReachedSound);
			}
			if (pointsReached == pointsNumber) {
				if (objectToActive && activeFunctionName != "") {
					objectToActive.SendMessage (activeFunctionName, SendMessageOptions.DontRequireReceiver);
				}
				pathActive = false;
				if (useTimer) {
					screenTimerText.gameObject.SetActive (false);
				}
				if (pathCompleteAudioSound) {
					audioSource.PlayOneShot (pathCompleteAudioSound);
				}
			}
		}
	}
	public void resetPath(){
		pointsReached = 0;
		pointsNumber = wayPoints.Count;
		if (!player) {
			player = GameObject.Find ("Player Controller");
		}
		if (!character) {
			character = GameObject.Find ("Character");
		}
		if (useTimer) {
			totalSecondsTimer = secondsToComplete + minutesToComplete * 60;
			if (!screenTimerText) {
				screenTimerText = character.GetComponent<showGameInfoHud> ().getHudElement ("Timer").GetComponent<Text> ();
			} 
			if (screenTimerText) {
				screenTimerText.gameObject.SetActive (true);
			}
		}
		points.Clear ();
		for (i = 0; i < wayPoints.Count; i++) {
			wayPoints [i].reached = false;
			points.Add (wayPoints [i].point.gameObject);
		}
		player.GetComponent<setObjective> ().removeGameObjectListFromList (points);
		if (showOneByOne) {
			wayPoints [pointsReached].point.GetComponent<mapObjectInformation> ().createMapIconInfo ();
		} else {
			for (i = 0; i < wayPoints.Count; i++) {
				wayPoints [i].point.GetComponent<mapObjectInformation> ().createMapIconInfo ();
			}
		}
		pathActive = true;
		if (inOrder && !showOneByOne && useLineRenderer) {
			lineRenderer.enabled = true;
		}
	}
	public void stopPath(){
		pathActive = false;
		if (useTimer) {
			screenTimerText.gameObject.SetActive (false);
		}
		player.GetComponent<setObjective> ().removeGameObjectListFromList (points);
		if (pathUncompleteAudioSound) {
			audioSource.PlayOneShot (pathUncompleteAudioSound);
		}
		if (inOrder && !showOneByOne && useLineRenderer) {
			lineRenderer.enabled = false;
		}
	}
	void OnDrawGizmos(){
		DrawGizmos();
	}
	void OnDrawGizmosSelected(){
		DrawGizmos();
	}
	void DrawGizmos(){
		if (!Application.isPlaying) {
			if (showGizmo) {
				for (i = 0; i < wayPoints.Count; i++) {
					if (wayPoints [i].point) {
						Gizmos.color = Color.yellow;
						Gizmos.DrawSphere (wayPoints [i].point.position, gizmoRadius);
						if (inOrder) {
							if (i + 1 < wayPoints.Count) {
								Gizmos.color = Color.white;
								Gizmos.DrawLine (wayPoints [i].point.position, wayPoints [i + 1].point.position);
							}
						} else {
							Gizmos.color = Color.white;
							Gizmos.DrawLine (wayPoints [i].point.position, transform.position);
						}
					}
				}
			} else {
				for (i = 0; i < wayPoints.Count; i++) {
					wayPoints [i].point.GetComponent<mapObjectInformation> ().showGizmo = showGizmo;
				}
			}
		}
	}
	[System.Serializable]
	public class wayPointInfo{
		public string Name;
		public Transform point;
		public bool reached;
		public float triggerRadius;
	}
}