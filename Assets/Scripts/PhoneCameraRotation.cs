using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneCameraRotation : MonoBehaviour {
	static bool gyroBool;
	private Gyroscope gyro;
	private Quaternion quatMult;
	private Quaternion quatMap;

	// Use this for initialization
	void Awake2 () {


		//Input.compensateSensors = true;
		//Input.location.Start ();

		// https://forum.unity3d.com/threads/sharing-gyroscope-controlled-camera-on-iphone-4.98828/#post-849438
		Transform currentParent = transform.parent;
		GameObject camParent = new GameObject ("camParent");
		camParent.transform.position = transform.position;
		transform.parent = camParent.transform;

		GameObject camGrandParent = new GameObject ("camGrandParent");
		camGrandParent.transform.position = transform.position;
		camParent.transform.parent = camGrandParent.transform;
		camGrandParent.transform.parent = currentParent;

		gyroBool = SystemInfo.supportsGyroscope;
		if (gyroBool) {
			gyro = Input.gyro;
			gyro.enabled = true;

			camParent.transform.eulerAngles = new Vector3 (-90, 0, 0);
			if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
				quatMult = new Quaternion(0f, 0f, 0.7071f, -0.7071f);
			} else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
				quatMult = new Quaternion(0f, 0f, -0.7071f, -0.7071f);
			} else if (Screen.orientation == ScreenOrientation.Portrait) {
				quatMult = new Quaternion(0f, 0f, 0f, 1f);
			} else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
				quatMult = new Quaternion(0f, 0f, 1f, 0f);
			}
		}
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
	// Update is called once per frame
	void Update () {
		if (gyroBool) {
			quatMap = new Quaternion(gyro.attitude. w,gyro.attitude.x, gyro.attitude.y, gyro.attitude.z);
			transform.localRotation = quatMap * quatMult;
		}
	}
}
