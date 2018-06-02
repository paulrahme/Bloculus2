using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonQuitGame : MenuButton
{
	// Public variables
	public bool				gSaveGameOnQuit;		// True to save and exit, false to exit without saving


	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		if (gSaveGameOnQuit)
		{
			Tower.gInstance.SaveAndExit();
		}
		else
		{
			Tower.gInstance.ExitNoSave();
		}
	}
}
