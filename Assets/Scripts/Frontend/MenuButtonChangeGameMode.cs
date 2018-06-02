using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuButtonChangeGameMode : MenuButton
{
	// Public variables
	public int		gGameModeOffset = 1;		// Game Mode change offset (usually -1 or 1)


	/// <summary> Called before first Update() </summary>
	void Start()
	{
		UpdateGameModeText();
	}


	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		int newMode = Convert.ToInt32(Tower.gInstance.gGameMode) + gGameModeOffset;
		int numGameModes = Enum.GetNames(typeof(Tower.eGameModes)).Length;
		if (newMode < 0) { newMode += numGameModes; }
		else if (newMode >= numGameModes) { newMode -= numGameModes; }
		Tower.gInstance.SetGameMode((Tower.eGameModes)newMode);
		UpdateGameModeText();
		gParentMenu.RefreshLockedOrUnlockedCubes();
	}

	
	/// <summary> Refreshes the text + description for the current game mode. </summary>
	private void UpdateGameModeText()
	{
		switch (Tower.gInstance.gGameMode)
		{
			case Tower.eGameModes.Original:
				gChildTextObject.text = "Original";
				gDescriptionTextObject.text = "Full length levels with both Level Timer and Ring Bar.";
				break;
				
			case Tower.eGameModes.Arcade:
				gChildTextObject.text = "Arcade";
				gDescriptionTextObject.text = "Quick levels starting on Level 1. See how far you can go!";
				break;
				
			case Tower.eGameModes.TimeChallenge:
				gChildTextObject.text = "Time Challenge";
				gDescriptionTextObject.text = "Fill the Ring Bar to complete 5 levels for the best time!";
				break;
				
			case Tower.eGameModes.SpeedChallenge:
				gChildTextObject.text = "Speed Challenge";
				gDescriptionTextObject.text = "Fill the Ring Bar to complete 1 level for the best time!";
				break;
				
			case Tower.eGameModes.ScoreChallenge:
				gChildTextObject.text = "Score Challenge";
				gDescriptionTextObject.text = "5 quick levels with no Ring Bar. Try for a high score!";
				break;
				
			default:
				throw new Exception("Unhandled GameMode '"+Tower.gInstance.gGameMode+"'");
		}
	}
}
