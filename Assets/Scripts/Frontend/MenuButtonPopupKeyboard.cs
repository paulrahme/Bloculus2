using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonPopupKeyboard : MenuButton
{
	// Private variables
	private TouchScreenKeyboard				gTouchScreenKeyboard;											// Popup keyboard
	
	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		gTouchScreenKeyboard = TouchScreenKeyboard.Open(gChildTextObject.text, TouchScreenKeyboardType.Default);
	}


	/// <summary> Called once per frame </summary>
	protected override void Update()
	{
		base.Update();

		// Per-frame updates
		if (gTouchScreenKeyboard != null)
		{
			if (gTouchScreenKeyboard.done && !gTouchScreenKeyboard.wasCanceled)
			{
				gTouchScreenKeyboard = null;
				Tower.gInstance.SaveHiScoreEntry(gChildTextObject.text);
			}
			else
			{
				gChildTextObject.text = gTouchScreenKeyboard.text;
			}
		}
	}
}
