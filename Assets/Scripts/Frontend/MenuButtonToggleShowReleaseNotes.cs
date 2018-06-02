using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonToggleShowReleaseNotes : MenuButton
{
	/// <summary> Called before first Update() </summary>
	void Start()
	{
		UpdateTexture();
	}
	
	
	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		Tower towerScript = Tower.gInstance;

		bool showInFuture = !towerScript.ShowReleaseNotes();
		towerScript.ShowReleaseNotesInFuture(showInFuture);
		UpdateTexture();
		towerScript.ShowReleaseNotesInFuture(showInFuture);
	}
	
	
	/// <summary> Updates the texture to match the current music state </summary>
	private void UpdateTexture()
	{
		gTextureObject.SetActive(Tower.gInstance.ShowReleaseNotes());
	}
}
