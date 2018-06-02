using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonRetryGame : MenuButton
{
	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		Tower.gInstance.ReplayGame();
	}
}
