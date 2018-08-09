﻿using UnityEngine;

public class UIMaster : MonoBehaviour
{
	#region Inspector variables
	public UI_MainMenu mainMenu;
	public UI_HUD hud;
	public UI_PauseMenu pauseMenu;
	#endregion	// Inspector variables

	/// <summary> Singleton instance </summary>
	public static UIMaster instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;
	}

	/// <summary> Called before first Update() </summary>
	void Start()
	{
		mainMenu.gameObject.SetActive(true);
	}

	/// <summary> Shows the pause screen </summary>
	public void Pause()
	{
		hud.gameObject.SetActive(false);
		pauseMenu.gameObject.SetActive(true);
	}

	/// <summary> Disables pause screen & goes back to gameplay </summary>
	public void Unpause()
	{
		pauseMenu.gameObject.SetActive(false);
		hud.gameObject.SetActive(true);
	}

	/// <summary> Disables pause screen & quits back to main menu </summary>
	public void Quit()
	{
		pauseMenu.gameObject.SetActive(false);
		mainMenu.gameObject.SetActive(true);
	}
}