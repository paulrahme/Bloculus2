using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonSetHelpScreen : MenuButton
{
	// Public variables
	public int		gHelpScreenIndex = 0;		// Index of help screen to show


	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		gParentMenu.SetHelpScreenIndex(gHelpScreenIndex);
		gParentMenu.SetFrontendScreen(FrontendMenu.eFrontendScreens.HowToPlay);
	}
}
