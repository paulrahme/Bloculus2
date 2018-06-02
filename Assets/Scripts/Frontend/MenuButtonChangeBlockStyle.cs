using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonChangeBlockStyle : MenuButton
{
	// Enums
	public enum				eBlockStyles { Solid, Transparent };
	
	// Public variables
	public eBlockStyles		gBlockStyle;							// Block style, corresponging to int index in Tower.gBlockStyle


	/// <summary> Refreshes tint colour depending on current states/conditions </summary>
	public override void UpdateColor()
	{
		SetColor(((int)gBlockStyle == Tower.gInstance.gBlockStyle) ? Color.white : Color.grey);
	}


	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		Tower.gInstance.gBlockStyle = (int)gBlockStyle;
		Tower.gInstance.ClearBlocks(true);
		Tower.gInstance.EmptyRecyclePool();
		Tower.gInstance.RefreshTower(true);
		gParentMenu.UpdateColors();
		PlayerPrefs.SetInt(Constants.kPPBlockStyle, Tower.gInstance.gBlockStyle);
	}
}
