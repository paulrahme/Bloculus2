using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonChangeLevel : MenuButton
{
	// Public variables
	public int		gLevel = 0;			// Level to change to


	/// <summary> Refreshes tint colour depending on current states/conditions </summary>
	public override void UpdateColor()
	{
		SetColor((gLevel == Tower.gInstance.gStartingLevel) ? Color.white : Color.grey);
	}

	/// <summary> Should this button be locked out? </summary>
	/// <returns> eLockStates.... state </returns>
	protected override eLockStates GetLockState()
	{
#if FREE_VERSION
		return eLockStates.Locked;
#else
		if (Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.P))
		{
			return eLockStates.Unlocked;
		}
		else
		{
			return ((Tower.gInstance.gGameMode == Tower.eGameModes.Arcade) || (gLevel > Math.Max(PlayerPrefs.GetInt(Constants.kPPHighestLevel), Constants.kMinHighestLevel))) ? eLockStates.Locked : eLockStates.Unlocked;
		}
#endif
	}
	

	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		Tower.gInstance.SetStartingLevel(gLevel);
		gParentMenu.UpdateColors();
		PlayerPrefs.SetInt(Constants.kPPStartingLevel, gLevel);
	}
}
