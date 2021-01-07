using UnityEngine;
using System.Collections;
public class AIStateManager : MonoBehaviour {
	health healthManager;

	void Start () {
		healthManager = GetComponent<health> ();
	}
	void Update () {
	
	}
	public void getHealth (float amount)
	{
		if (!healthManager.dead) {
			healthManager.healthAmount += amount;
			if (healthManager.healthAmount >= healthManager.getMaxHealthAmount ()) {
				healthManager.healthAmount = healthManager.getMaxHealthAmount ();
			}
			if (healthManager.healthAmount < healthManager.getMaxHealthAmount ()) {
				healthManager.getHealth (amount);
			}
		}
	}
}