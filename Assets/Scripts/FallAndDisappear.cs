using UnityEngine;
using System.Collections;

public class FallAndDisappear : MonoBehaviour
{
	// Public variables
	public float				gLifetime = 2.0f;															// How long it takes to disappear, in seconds
	public Vector3				gAcceleration = new Vector3(0.0f, -0.2f, 0.0f);								// Gravitational acceleration (units/second)
	public float				gSpinSpeed = 0.0f;															// How quickly it rotates around the y axis (degrees/second)
	public bool					gCreateShockwave = true;													// Create a ripple ring when it lands/disappears?
	
	// Private variables
	private float				gLifeCounter;																// Fade counter
	private Vector3				gVelocity;																	// Current movement velocity
	private Vector3				gCachedSpinVec;																// Used to avoid new Vector3() every update
	
	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		gLifeCounter = 1.0f;
		gVelocity = Vector3.zero;
		gCachedSpinVec = new Vector3(0.0f, gSpinSpeed, 0.0f);
	}
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

		gLifeCounter -= dTime / gLifetime;
		if (gLifeCounter <= 0.0f)
		{
			if (gCreateShockwave)
			{
				// Add ripple & create pulse
				GroundController.Instance.AddRipple(transform.position.x);
				Vector3 ripplePos = new Vector3(transform.position.x, GroundController.Instance.transform.position.y, Tower.gInstance.transform.position.z);
				RippleGrowAndFade.StartRipple(ripplePos, GetComponent<Renderer>().material.color);
			}

			// Disappear
			GameObject.Destroy(gameObject);
		}
		else
		{
			gVelocity += gAcceleration * dTime;
			transform.position += gVelocity;
			if (gSpinSpeed != 0.0f)
			{
				transform.eulerAngles += gCachedSpinVec * dTime;
			}
		}
	}	
}
