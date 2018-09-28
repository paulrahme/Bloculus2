using UnityEngine;

public class DebugKeys : MonoBehaviour
{
#if UNITY_EDITOR
	/// <summary> Called once per frame </summary>
	void Update()
	{
		HandleScreenshotKey();
		HandleDebugKeys();
	}

	/// <summary> Handles debug key/s for saving screenshots </summary>
	void HandleScreenshotKey()
	{
		if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
		{
			string fileName = Screen.width + "x" + Screen.height + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm_ss") + ".png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log("Saved screenshot in project's folder: '" + fileName + "'");
		}
	}

	/// <summary> Handles misc debug keys </summary>
	void HandleDebugKeys()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				Time.timeScale = 0.1f;
			else
				Time.timeScale = 7.0f;
		}
		else if (Input.GetKeyUp(KeyCode.F))
			Time.timeScale = 1.0f;
	}
#endif
}
