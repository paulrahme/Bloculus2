using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonToggleMusic : MenuButton
{
	/// <summary> Called before first Update() </summary>
	void Start()
	{
		UpdateTexture();
	}
	
	
	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		PlayerPrefs.SetInt(Constants.kPPMusicEnabled, (BackgroundMusic.gInstance.ToggleMusic() ? 1 : 0));
		UpdateTexture();
	}


	/// <summary> Updates the texture to match the current music state </summary>
	private void UpdateTexture()
	{
		gTextureObject.GetComponent<Renderer>().material.mainTexture = (BackgroundMusic.gInstance.IsMusicEnabled() ? gTextureOn : gTextureOff);
	}
}
