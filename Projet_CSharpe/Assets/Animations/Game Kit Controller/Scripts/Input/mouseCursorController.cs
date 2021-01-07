using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
public class mouseCursorController : MonoBehaviour {
	public RectTransform cursor;
	public float cursorSpeed=1000;
	Vector2 cursorPosition;
	Vector2 axisInput;
	float newX,newY;
	bool cursorCanBeEnabled;
	inputManager input;
	[DllImport("user32.dll")]
	static extern bool SetCursorPos(int X, int Y);
	[DllImport("user32.dll")]
	static extern bool GetCursorPos(out Point pos);
	Point cursorPos = new Point();

	void Start () {
		input = GetComponent<inputManager> ();
		resetCursorPosition ();
	}
	void Update () {
		if (!input.touchControlsCurrentlyEnabled && cursorCanBeEnabled && Input.GetJoystickNames().Length==1) {
			if (input.checkInputButton ("Grab Objects", inputManager.buttonType.getKeyDown)) {
				MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp | MouseOperations.MouseEventFlags.LeftDown);
			}
			//			if (input.getButton ("Grab Objects", inputManager.buttonType.getKeyUp)) {
			//				MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown | MouseOperations.MouseEventFlags.LeftUp);
			//			}
			axisInput.x = input.getMovementAxis ("keys").x;
			axisInput.y = input.getMovementAxis ("keys").y;

			GetCursorPos (out cursorPos);
			if (axisInput.x > 0) {
				cursorPosition.x += Time.deltaTime * cursorSpeed;
			} else if (axisInput.x < 0) {
				cursorPosition.x -= Time.deltaTime * cursorSpeed;
			}
			if (axisInput.y > 0) {
				cursorPosition.y -= Time.deltaTime * cursorSpeed;
			} else if (axisInput.y < 0) {
				cursorPosition.y += Time.deltaTime * cursorSpeed;
			}
			newX = Mathf.Lerp (newX, cursorPosition.x, Time.deltaTime * 10);
			newY = Mathf.Lerp (newY, cursorPosition.y, Time.deltaTime * 10);
			SetCursorPos ((int)newX, (int)newY);
		}
	}
	public void showOrHideCursor(bool state){
		cursorCanBeEnabled = state;
		if (cursorCanBeEnabled) {
			resetCursorPosition();
		}
	}
	public void resetCursorPosition(){
		cursorPosition.x = (int)(Screen.width / 1.5f);
		cursorPosition.y = (int)(Screen.height / 1.5f);
	}
	public struct Point{
		public int X;
		public int Y;

		public Point(int x, int y){
			this.X = x;
			this.Y = y;
		}
	}
}
