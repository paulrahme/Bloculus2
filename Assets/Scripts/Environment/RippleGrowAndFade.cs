using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RippleGrowAndFade : MonoBehaviour
{
	// Public variables
	public float						gFadeTime = 1.0f;								// How long it takes to disappear, in seconds
	public float						gGrowAmount = 5.0f;								// How much it grows while disappearing
	public bool							gDisableDontDestroy = false;					// WHen it's finished, SetActive(false) rather than GameObject.Destroy
	
	// Private variables
	private float						gFadeAmount;									// Fade counter
	private static Stack<GameObject>	gRecycleStack = new Stack<GameObject>();		// Stack of GameObjects ready for reuse


	/// <summary> Creates (or reuses) a shockwave GameObject </summary>
	/// <param name="position"> Centre position </param>
	/// <param name="color"> Ripple's colour </param>
	public static void StartRipple(Vector3 position, Color color)
	{
		GameObject gameObj = (gRecycleStack.Count > 0) ? gRecycleStack.Pop() : (GameObject.Instantiate(Tower.instance.rippleRingPrefab) as GameObject);

		// Set position & rotation
		gameObj.transform.parent = null;
		gameObj.transform.position = position;
		gameObj.transform.localScale = Vector3.zero;
		gameObj.GetComponent<Renderer>().material.color = color;

		// (Re)start popup animation
		gameObj.GetComponent<RippleGrowAndFade>().Reset();
	}
	

	/// <summary> Restarts the animation </summary>
	void Reset()
	{
		gFadeAmount = 1.0f;
	}


	/// <summary> Called once per frame </summary>
	void Update()
	{
		gFadeAmount -= Time.deltaTime / gFadeTime;
		if (gFadeAmount <= 0.0f)
		{
			if (gDisableDontDestroy)
			{
				gameObject.SetActive(false);
			}
			else
			{
				transform.parent = null;//Tower.gInstance.gDisabledGameObjectPool;
				gRecycleStack.Push(gameObject);
			}
		}
		else
		{
			// Grow
			float scale = (1.0f - gFadeAmount) * gGrowAmount;
			transform.localScale = new Vector3(scale, scale, scale);
			
			// Fade
			Color newColor = GetComponent<Renderer>().material.color;
			newColor.a = gFadeAmount;
			GetComponent<Renderer>().material.color = newColor;
		}
	}	
}
