using UnityEngine;
using System.Collections;
public class fadeObject : MonoBehaviour {
	public bool vanish;
	public float vanishSpeed;
	Renderer meshRenderer;

	void Start () {
		meshRenderer = GetComponentInChildren<Renderer> ();
	}
	void Update () {
		if (vanish) {
			Color alpha = meshRenderer.material.color;
			alpha.a -= Time.deltaTime * vanishSpeed;
			meshRenderer.material.color = alpha;
			if (alpha.a <= 0) {
				Destroy (gameObject);
			}
		}
	}
	public void activeVanish(float speed){
		vanish = true;
		vanishSpeed = speed;
	}
}