using UnityEngine;
using System.Collections;

public class moveObject : MonoBehaviour {
	Vector3 bottomPosition;
	public float speed;
	public float moveAmount;
	// Use this for initialization
	void Start () {
		bottomPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = bottomPosition + ((Mathf.Cos(Time.time * speed)) / 2 ) *moveAmount * transform.up;
	}
}
