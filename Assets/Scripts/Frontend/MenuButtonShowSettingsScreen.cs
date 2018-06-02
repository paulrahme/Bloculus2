using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonShowSettingsScreen : MenuButton
{
	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		gParentMenu.SetFrontendScreen(FrontendMenu.eFrontendScreens.Settings);
	}
}
