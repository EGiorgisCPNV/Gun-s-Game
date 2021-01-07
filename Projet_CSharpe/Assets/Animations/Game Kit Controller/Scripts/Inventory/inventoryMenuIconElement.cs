using UnityEngine;
using System.Collections;
using UnityEngine.UI;
[System.Serializable]
public class inventoryMenuIconElement : MonoBehaviour {
	public Button button;
	public Text name;
	public Text amount;
	public RawImage icon;
	public GameObject pressedIcon;
}