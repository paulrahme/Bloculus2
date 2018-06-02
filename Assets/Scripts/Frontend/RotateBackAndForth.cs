using UnityEngine;
using System.Collections;

public class RotateBackAndForth : MonoBehaviour
{
	// Public variables
	public float					gSpeed = 1.0f;													// How quickly to wave back & forth, in cycles per second
	public float					gAmount = 2.0f;													// How much to rotate, in degrees
	
	// Private variables
	private Vector3					gOrigEulerAngles;												// Saved original rotation from the Unity Editor
	private float					gCurrentValue;													// Movement counter
	
	/// <summary> Called when the object/script initiates </summary>
	void Awake()
	{
		gOrigEulerAngles = transform.localEulerAngles;
		gCurrentValue = 0.0f;
	}
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		transform.localEulerAngles = gOrigEulerAngles + new Vector3(0.0f, Mathf.Sin(gCurrentValue) * gAmount, 0.0f);
		gCurrentValue += gSpeed * Time.deltaTime;
	}	
}
