using UnityEngine;
using System.Collections;

public class checkCollisionType : MonoBehaviour {
	public bool onCollisionEnter;
	public bool onCollisionExit;
	public bool onTriggerEnter;
	public bool onTriggerExit;
	public bool onTriggerStay;
	public string onCollisionEnterFunctionName;
	public string onCollisionExitFunctionName;
	public string onTriggerEnterFunctionName;
	public string onTriggerExitFunctionName;
	public string onTriggerStayFunctionName;
	public GameObject parent;
	public bool active;
	public GameObject objectToCollide;

	//a script to check all the type of collisions of an object, and in that case, send a message to another object according to the type of collision
	//if you want to use a collision enter, check the bool onCollisionEnter in the editor, set the funcion called in the onCollisionEnterFunctionName string
	//and finally set the parent, the object which will receive the function
	//also, you can set an specific object to check a collision with that object
	//the variable active can be used to check when the collision happens
	void OnCollisionEnter(Collision col){
		if (onCollisionEnter) {
			if(objectToCollide){
				if(col.gameObject== objectToCollide){
					if(onCollisionEnterFunctionName!="" && parent){
						parent.SendMessage(onCollisionEnterFunctionName,col.gameObject);
						active=true;
					}
					else{
						active=true;
					}
				}
			}
			else{
				if(onCollisionEnterFunctionName!="" && parent){
					parent.SendMessage(onCollisionEnterFunctionName,col.gameObject);
					active=true;
				}
			}
		}
	}
	void OnCollisionExit(Collision col){
		if(onCollisionExit){
			active=true;
			if(onCollisionExitFunctionName!="" && parent){
				parent.SendMessage(onCollisionExitFunctionName,col.gameObject);
			}
		}
	}
	void OnTriggerEnter(Collider col){
		if(onTriggerEnter){
			if(objectToCollide){
				if(col.gameObject== objectToCollide){
					if(onTriggerEnterFunctionName!="" && parent){
						parent.SendMessage(onTriggerEnterFunctionName,col.gameObject);
						active=true;
					}
					else{
						active=true;
					}
				}
			}
			else{
				if(onTriggerEnterFunctionName!="" && parent){
					parent.SendMessage(onTriggerEnterFunctionName,col.gameObject);
					active=true;
				}
			}
		}
	}
	void OnTriggerExit(Collider col){
		if(onTriggerExit){
			active=true;
			if(onTriggerExitFunctionName!="" && parent){
				parent.SendMessage(onTriggerExitFunctionName,col.gameObject);
			}
		}
	}
	void OnTriggerStay(Collider col){
		if (onTriggerStay) {
			if(objectToCollide){
				if(col.gameObject== objectToCollide){
					if(onTriggerStayFunctionName!="" && parent){
						parent.SendMessage(onTriggerStayFunctionName,col.gameObject);
						active=true;
					}
					else{
						active=true;
					}
				}
			}
			else{
				if(onTriggerStayFunctionName!="" && parent){
					parent.SendMessage(onTriggerStayFunctionName,col.gameObject);
					active=true;
				}
			}
		}
	}
}