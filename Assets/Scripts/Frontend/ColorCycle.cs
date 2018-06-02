using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ColorCycle : MonoBehaviour
{
	// Constants & enums
	public enum						eCycleBehaviours { BackAndForth, Random16 };

	// Public variables
	public float					gSpeed = 0.35f;													// How quickly to cycle, in units per second
	public Tower					gTowerScript;													// Pointer to Tower's script, for colours
	public eCycleBehaviours			gCycleBehaviour = eCycleBehaviours.BackAndForth;				// What type of cycling to do
	
	// Private variables
	private Color					gFromColor, gToColor;											// Saved original rotation from the Unity Editor
	private float					gLerpValue;														// Movement counter
	private int						gToColorIndex;													// Colour to fade towards, in the Tower script's kColor array
	private List<Color>				gColors = new List<Color>();									// List of colours to cycle between
	
	// Inline/helper functions
	private float					GetRandomColorComponent() { return Convert.ToSingle(Tower.gInstance.gRandom.Next(3, 10)) / 10.0f; }
	
	/// <summary> Called before first Update() </summary>
	void Start()
	{
		switch (gCycleBehaviour)
		{
			case eCycleBehaviours.BackAndForth:
				// Cycle to a random colour & back a couple of times
				gColors.Add(new Color(GetRandomColorComponent(), GetRandomColorComponent(), GetRandomColorComponent()));
				gColors.Add(GetComponent<Renderer>().material.color);
				gColors.Add(new Color(GetRandomColorComponent(), GetRandomColorComponent(), GetRandomColorComponent()));
				gColors.Add(GetComponent<Renderer>().material.color);
				break;
			
			case eCycleBehaviours.Random16:
				for (int i = 0; i < 16; ++i)
				{
					gColors.Add(new Color(GetRandomColorComponent(), GetRandomColorComponent(), GetRandomColorComponent()));
				}
				break;
			
			default:
				throw new Exception("Unhandled cycle behaviour "+gCycleBehaviour);
		}
		
		// Start cycling
		gToColorIndex = gTowerScript.gRandom.Next(gColors.Count);
		gFromColor = gToColor = GetComponent<Renderer>().material.color;
		StartFadingToNextColor();
	}
	
	
	/// <summary> Resets the colours, finds next one to fade to, and begins fading </summary>
	void StartFadingToNextColor()
	{
		gFromColor = gToColor;
		if (gToColorIndex == (gColors.Count - 1))
		{
			gToColorIndex = 0;
		}
		else
		{
			++gToColorIndex;
		}
		gToColor = gColors[gToColorIndex];
		gLerpValue = 0.0f;
	}
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		gLerpValue += gSpeed * Time.deltaTime;
		if (gLerpValue > 1.0f)
		{
			StartFadingToNextColor();
		}
		GetComponent<Renderer>().material.color = new Color(Mathf.Lerp(gFromColor.r, gToColor.r, gLerpValue),
											Mathf.Lerp(gFromColor.g, gToColor.g, gLerpValue),
											Mathf.Lerp(gFromColor.b, gToColor.b, gLerpValue),
											Mathf.Lerp(gFromColor.a, gToColor.a, gLerpValue));
	}
}
