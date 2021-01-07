using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class shipInterfaceInfo : MonoBehaviour {
	public bool interfaceEnabled;
	public bool compassEnabled;
	public GameObject interfaceCanvas;
	public RectTransform compassBase;
	public RectTransform north;
	public RectTransform south;
	public RectTransform east;
	public RectTransform west;
	public RectTransform altitudeMarks;
	public Text pitchValue;
	public Text yawValue;
	public Text rollValue;
	public Text altitudeValue;
	public Text velocityValue;
	public Text coordinateXValue;
	public Text coordinateYValue;
	public Text coordinateZValue;
	public RectTransform level;
	public float altitudeMarkSpeed;
	public Slider healthBar;
	public Slider energyBar;
	public Text weaponName;
	public Text weaponAmmo;
	public Text canLand;
	public Text enginesState;
	vehicleHUDManager HUDManager;
	vehicleGravityControl gravityManager;
	vehicleWeaponSystem weaponManager;
	int compassDirection;
	int compassDirectionAux;
	Vector3 normal;
	float currentSpeed;
	Rigidbody mainRigidbody;

	void Start () {
		HUDManager = GetComponent<vehicleHUDManager> ();
		gravityManager = GetComponent<vehicleGravityControl> ();
		mainRigidbody = GetComponent<Rigidbody> ();
		weaponManager = GetComponent<vehicleWeaponSystem> ();
		healthBar.maxValue = HUDManager.healthAmount;
		healthBar.value = healthBar.maxValue;
		energyBar.maxValue = HUDManager.boostAmount;
		energyBar.value = energyBar.maxValue;
	}
	void Update () {
		if (interfaceEnabled) {
			currentSpeed = mainRigidbody.velocity.magnitude;
			if (compassEnabled) {
				compassDirection = (int)Mathf.Abs (transform.eulerAngles.y);
				if (compassDirection > 360) {
					compassDirection = compassDirection % 360;
				}
				compassDirectionAux = compassDirection;
				if (compassDirectionAux > 180) {
					compassDirectionAux = compassDirectionAux - 360;
				}
				north.anchoredPosition = new Vector2 (-compassDirectionAux * 2, 0);
				south.anchoredPosition = new Vector2 (-compassDirection * 2 + 360, 0);
				east.anchoredPosition = new Vector2 (-compassDirectionAux * 2 + 180, 0);
				west.anchoredPosition = new Vector2 (-compassDirection * 2 + 540, 0);
				normal = gravityManager.currentNormal;
				float angleX = Mathf.Asin (transform.InverseTransformDirection (Vector3.Cross (normal.normalized, transform.up)).x) * Mathf.Rad2Deg;
				altitudeMarks.anchoredPosition = Vector2.MoveTowards (altitudeMarks.anchoredPosition, new Vector2 (0, angleX), Time.deltaTime * altitudeMarkSpeed);
			}
			pitchValue.text = transform.eulerAngles.x.ToString ("0") + " º";
			yawValue.text = transform.eulerAngles.y.ToString ("0") + " º";
			rollValue.text = transform.eulerAngles.z.ToString ("0") + " º";
			altitudeValue.text = transform.position.y.ToString ("0") + " m";
			velocityValue.text = currentSpeed.ToString ("0") + " km/h";
			coordinateXValue.text = transform.position.x.ToString("0"); 
			coordinateYValue.text = transform.position.y.ToString("0"); 
			coordinateZValue.text = transform.position.z.ToString("0"); 
			level.localEulerAngles = new Vector3 (0, 0, transform.eulerAngles.z);

			if (weaponManager) {
				weaponName.text = weaponManager.currentWeapon.Name;
				weaponAmmo.text = weaponManager.currentWeapon.ammoPerClip.ToString () + "/" + weaponManager.currentWeapon.remainAmmo.ToString ();
			}
			healthBar.value = HUDManager.healthAmount;
			energyBar.value = HUDManager.boostAmount;
		}
	}
	public void enableOrDisableInterface(bool state){
		interfaceEnabled = state;
		interfaceCanvas.SetActive (interfaceEnabled);
	}
	public void shipCanLand(bool state){
		if (state) {
			canLand.text = "YES";
		} else {
			canLand.text = "NO";
		}
	}
	public void shipEnginesState(bool state){
		if (state) {
			enginesState.text = "ON";
		} else {
			enginesState.text = "OFF";
		}
	}
}