using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuButtonChangeControlMethod : MenuButton
{
	// Public variables
	public int		gSelectionOffset = 1;		// Switching offset (usually -1 or 1)
	
	
	/// <summary> Called before first Update() </summary>
	void Start()
	{
		UpdateText();
	}
	
	
	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		int selection = Convert.ToInt32(Tower.gInstance.gControlMethod) + gSelectionOffset;
		int numControlMethods = Enum.GetNames(typeof(Tower.eControlMethods)).Length;
		if (selection < 0) { selection += numControlMethods; }
		else if (selection >= numControlMethods) { selection -= numControlMethods; }
		Tower.gInstance.SetControlMethod((Tower.eControlMethods)selection);
		UpdateText();
	}
	
	
	/// <summary> Refreshes the text + description for the current game mode. </summary>
	private void UpdateText()
	{
		switch (Tower.gInstance.gControlMethod)
		{
			case Tower.eControlMethods.TouchButtons:
				gChildTextObject.text = "Button Control";
				gDescriptionTextObject.text = "Control the selector using on-screen buttons.";
				break;
				
			case Tower.eControlMethods.SwipeSelector:
				gChildTextObject.text = "Swipe Selector";
				gDescriptionTextObject.text = "Contol the selector by swiping across the screen.";
				break;
				
			case Tower.eControlMethods.SwipeTower:
				gChildTextObject.text = "Swipe Tower";
				gDescriptionTextObject.text = "Control the tower by swiping across the screen.";
				break;
				
			default:
				throw new Exception("Unhandled ControlMethod '"+Tower.gInstance.gControlMethod+"'");
		}
	}
}
