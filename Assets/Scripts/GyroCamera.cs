using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class MyTest : MonoBehaviour
{
}

public class MyChildTest1 : MyTest
{
}

public class MyChildTest2 : MyTest
{
}

public class GyroCamera : MonoBehaviour
{
	public Text textElement;
	public GPSLocation gpsLocation0;
	public GPSLocation gpsLocation1;
	public GPSLocation gpsLocation2;

	public float testLatitude = 53.24137f;
	public float testLongtitude = 6.531142f;

	public bool runningOnGlasses = false;

	private float initialYAngle = 0f;
	private float appliedGyroYAngle = 0f;
	private float calibrationYAngle = 0f;

	public IEnumerable<System.Type> FindDerivedTypes(System.Type baseType)
	{
		Assembly assembly = Assembly.GetAssembly (typeof(GyroCamera));
		return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
	}

	void InitializeServices()
	{
		Application.targetFrameRate = 60;

		// Initialize gyroscope
		if (SystemInfo.supportsGyroscope) {
			Input.gyro.enabled = true;
		}

		// Initialize GPS service
		if (SystemInfo.supportsLocationService) {
			Input.location.Start ();
		}

		// Initalize compass
		Input.compass.enabled = true;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// Get an array of types that derive from the "MyTest" class
		System.Type[] types = FindDerivedTypes(typeof(MyTest)).ToArray();
		foreach (System.Type type in types) {
			Debug.Log (type.Name);
		}

		// Add the component to my current gameObject
		gameObject.AddComponent (types [0]);
	}

	void Start()
	{
		InitializeServices ();

		// Initialize camera
		initialYAngle = transform.eulerAngles.y;
		if (runningOnGlasses) {
			GetComponent<Camera> ().fieldOfView = 10.0f;
		}
	}

	void Update()
	{
		ApplyGyroRotation();
		//ApplyManualCalibration();
		ApplyCompassCalibration();
		ApplyGPSLocation ();
	}

	void OnGUI()
	{
		if( GUILayout.Button( "Calibrate", GUILayout.Width( 300 ), GUILayout.Height( 100 ) ) )
		{
			CalibrateYAngle();
		}
	}

	public void CalibrateYAngle()
	{
		calibrationYAngle = appliedGyroYAngle - initialYAngle; // Offsets the y angle in case it wasn't 0 at edit time.
	}

	void ApplyGyroRotation()
	{
		if (textElement != null) {
			textElement.text = Input.gyro.attitude.ToString();
		}

		transform.rotation = Input.gyro.attitude;
		transform.Rotate( 0f, 0f, 180f, Space.Self ); // Swap "handedness" of quaternion from gyro.
		transform.Rotate( 90f, 180f, 0f, Space.World ); // Rotate to make sense as a camera pointing out the back of your device.
		appliedGyroYAngle = transform.eulerAngles.y; // Save the angle around y axis for use in calibration.
	}

	void ApplyManualCalibration()
	{
		transform.Rotate( 0f, -calibrationYAngle, 0f, Space.World ); // Rotates y angle back however much it deviated when calibrationYAngle was saved.
	}

	void ApplyCompassCalibration()
	{
		// get the y rotation
		float yRotation = transform.rotation.eulerAngles.y;

		if (textElement != null) {
			textElement.text = Input.compass.magneticHeading.ToString () + ", " + yRotation.ToString ();
		}
	}

	void ApplyGPSLocation()
	{
		if (Input.location.status == LocationServiceStatus.Running) {
			SetGPSLocation (Input.location.lastData.latitude, Input.location.lastData.longitude);
		}
		else
		{
			SetGPSLocation (testLatitude, testLongtitude);
		}
	}

	void SetGPSLocation(float latitude, float longtitude)
	{			
		// temp variables
		float x1 = gpsLocation0.longtitude;
		float x2 = gpsLocation1.longtitude;
		float x3 = gpsLocation2.longtitude;
		float y1 = gpsLocation0.latitude;
		float y2 = gpsLocation1.latitude;
		float y3 = gpsLocation2.latitude;
		float x = longtitude;
		float y = latitude;

		float determinant = (y2-y3)*(x1-x3) + (x3-x2)*(y1-y3);
		float lambda1 = ((y2-y3)*(x-x3) + (x3-x2)*(y-y3)) / determinant;
		float lambda2 = ((y3-y1)*(x-x3) + (x1-x3)*(y-y3)) / determinant;
		float lambda3 = 1.0f - lambda1 - lambda2;

//		Debug.Log ("det: " + determinant);
//		Debug.Log ("lambda1: " + lambda1);
//		Debug.Log ("lambda2: " + lambda2);
//		Debug.Log ("lambda3: " + lambda3);

		float nx = lambda1 * gpsLocation0.transform.position.x + lambda2 * gpsLocation1.transform.position.x + lambda3 * gpsLocation2.transform.position.x;
		float nz = lambda1 * gpsLocation0.transform.position.z + lambda2 * gpsLocation1.transform.position.z + lambda3 * gpsLocation2.transform.position.z;

		transform.position = new Vector3 (nx, 1.6f, nz);
	}
}