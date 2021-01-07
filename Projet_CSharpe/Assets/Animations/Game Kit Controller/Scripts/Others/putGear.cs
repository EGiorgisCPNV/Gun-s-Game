using UnityEngine;
using System.Collections;

public class putGear : MonoBehaviour {
	public bool disposed = false;
	public bool engaged=false;
	public GameObject initPos;
	public GameObject finalPos;
	public GameObject parent;
	GameObject gear;
	GameObject player;
	Vector3 movePos;

	void Start () {
		player=GameObject.Find("Player Controller");
		movePos = initPos.transform.position;
	}
	//it set the green gear of the scene in the gap of the engine
	void Update () {
		//move the gear from its position to the position inside the mechanism
		if(disposed){
			gear.transform.position=Vector3.MoveTowards(gear.transform.position,movePos,Time.deltaTime*5);
			if(gear.transform.position==movePos && !engaged){
				movePos=finalPos.transform.position;
				engaged=true;
			}
			if(gear.transform.position==movePos && engaged){
				engaged=false;
				GetComponent<putGear>().enabled=false;
			}
		}
	}	
	//when the gear touchs the trigger, 
	void OnTriggerEnter(Collider Other){
		if(Other.GetComponent<Collider>().gameObject.tag == "box" && Other.GetComponent<Collider>().gameObject.name == "gear"){
			//drop the gear and move and rotate smoothly the gear inside the mechanism
			player.GetComponent<grabObjects> ().dropObject ();
			gear=Other.GetComponent<Collider>().gameObject;
			gear.tag="Untagged";
			Destroy(gear.GetComponent<Rigidbody>());
			gear.transform.parent=parent.transform;
			parent.SendMessage("gearRotated",gear);
			StartCoroutine( rotateGear());
		}
	}
	//rotate the gear to the corret position inside the mechanism
	IEnumerator rotateGear(){
		for (float t = 0; t < 1; ){
			t += Time.deltaTime;
			gear.transform.rotation = Quaternion.Slerp(gear.transform.rotation,finalPos.transform.rotation,t);
		}
		yield return null;
		disposed=true;
	}
}