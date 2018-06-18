using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BlockDisappear : MonoBehaviour
{
	// Public variables
	public float						gFadeTime = 0.25f;								// How long it takes to disappear, in seconds

	// Private variables
	private Color						gColor;											// Fading colour
	private Material					gMaterial;										// Cached material
	private static Stack<GameObject>	gRecycleStack = new Stack<GameObject>();		// Stack of GameObjects ready for reuse


	/// <summary> Creates (or reuses) a disappearing block GameObject </summary>
	/// <param name="block"> Block from which to create </param>
	public static void StartDisappearing(Tower.Block block)
	{
		GameObject gameObj = (gRecycleStack.Count > 0) ? gRecycleStack.Pop() : (GameObject.Instantiate(Tower.gInstance.blockDisappearPrefab) as GameObject);

		// Match pos/rot/scale of original block
		Transform trans = gameObj.transform;
		Transform blockTrans = block.gameObj.transform;
		trans.parent = blockTrans.parent;
		trans.localPosition = blockTrans.localPosition;
		trans.localRotation = blockTrans.localRotation;
		trans.localScale = blockTrans.localScale;

		// (Re)start the disappear anim
		gameObj.GetComponent<BlockDisappear>().ResetAnim();
	}
	
	
	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		gMaterial = GetComponent<Renderer>().material;
	}


	/// <summary> Resets the disappear animation </summary>
	void ResetAnim()
	{
		gColor = Color.white;
	}


	/// <summary> Called once per frame </summary>
	void Update()
	{
		gColor.a -= Time.deltaTime / gFadeTime;
		if (gColor.a <= 0.0f)
		{
			transform.parent = null;//Tower.gInstance.gDisabledGameObjectPool;
			gRecycleStack.Push(gameObject);
		}
		else
		{
			gMaterial.color = gColor;
		}
	}	
}
