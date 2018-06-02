using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextGrowAndFade : MonoBehaviour
{
	// Public variables
	public float						gFadeTime = 0.15f;								// How long it takes to disappear, in seconds
	public float						gGrowAmount = 1.0f;								// How much it grows while disappearing
	public TextMesh						gTextMesh;										// TextMesh component

	// Public non-editor variables
	[HideInInspector]
	public Color						gOpaqueColor;									// Starting colour

	// Private variables
	private float						gFadeAmount;									// Fade counter
	private float						gOriginalOffsetZ;								// Starting size
	private static Stack<GameObject>	gRecycleStack = new Stack<GameObject>();		// Stack of GameObjects ready for reuse
	public bool							gDisableDontRecycle = false;					// When true, GameObject does not move into recycle stack


	/// <summary> Creates (or reuses) a fading text GameObject </summary>
	/// <param name="scorePopupPos"> Score popup position </param>
	/// <param name="rotation"> Rotation </param>
	/// <param name="name="color"> Opaque colour </param>
	/// <param name="text"> Text to use for score </param>
	public static void StartPopupText(Vector3 scorePopupPos, Quaternion rotation, Color color, string text)
	{
		GameObject gameObj = (gRecycleStack.Count > 0) ? gRecycleStack.Pop() : (GameObject.Instantiate(Tower.gInstance.gScoreTextPrefab) as GameObject);
		
		// Set position, rotation & text
		gameObj.transform.parent = null;
		gameObj.transform.position = scorePopupPos;
		gameObj.transform.rotation = rotation;
		TextGrowAndFade growFadeScript = gameObj.GetComponent<TextGrowAndFade>();
		growFadeScript.gTextMesh.text = text;
		growFadeScript.gTextMesh.color = growFadeScript.gOpaqueColor = color;
		
		// (Re)start popup animation
		gameObj.GetComponent<TextGrowAndFade>().ResetAnim();
	}
	
	
	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		gOriginalOffsetZ = gTextMesh.offsetZ;
		gOpaqueColor = gTextMesh.color;
		ResetAnim();
	}
	
	
	/// <summary> Resets the grow/fade </summary>
	public void ResetAnim()
	{
		gTextMesh.color = gOpaqueColor;
		gFadeAmount = 1.0f;
		gTextMesh.offsetZ = gOriginalOffsetZ;
	}

	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		gFadeAmount -= Time.deltaTime / gFadeTime;
		if (gFadeAmount <= 0.0f)
		{
			if (gDisableDontRecycle)
			{
				gameObject.SetActive(false);
			}
			else
			{
				transform.parent = Tower.gInstance.gDisabledGameObjectPool;
				gRecycleStack.Push(gameObject);
			}
		}
		else
		{
			gTextMesh.offsetZ = -1.0f - gGrowAmount + ((gFadeAmount / gFadeTime) * gGrowAmount);
			gTextMesh.color = new Color(gOpaqueColor.r, gOpaqueColor.g, gOpaqueColor.b, gFadeAmount);
		}
	}	
}
