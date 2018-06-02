using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMusic : MonoBehaviour
{
	// Public variables
	public AudioClip			gMusicTitle;																	// Music for title screen
	public AudioClip[]			gInGameMusic;																	// Music for levels
	public AudioClip			gMusicGameOver;																	// Music for Game Over
	public Tower				gTowerScript;																	// Tower script
	public float				gFadeSpeed = 0.5f;																// How much it fades up/down per second
	public List<AudioSource>	gSFXAudioSources;																// All sound effect audio sources, for muting
	
	// Private variables
	private float				gFullVolume;																	// Full volume to fade up to
	private float				gFadeMult = 1.1f;																// Fade multiplier
	
	// Helper/inline functions
	public bool					IsMusicEnabled() { return !(GetComponent<AudioSource>().mute); }
	public bool					IsSoundEnabled() { return !(gSFXAudioSources[0].mute); }

	// Instance
    public static BackgroundMusic	gInstance { get; private set; }

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		gInstance = this;
		gFullVolume = GetComponent<AudioSource>().volume;
		if (PlayerPrefs.GetInt(Constants.kPPMusicEnabled, 1) == 0) { ToggleMusic(); }
		if (PlayerPrefs.GetInt(Constants.kPPSoundEnabled, 1) == 0) { ToggleSoundEffects(); }
	}
	
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		if (gFadeMult < 1.0f)
		{
			gFadeMult += gFadeSpeed * Time.deltaTime;
			GetComponent<AudioSource>().volume = gFullVolume * gFadeMult;
		}
	}
	
	
	/// <summary> Starts the music for the current level </summary>
	public void StartGameMusic(int musicIndex)
	{
		if (GetComponent<AudioSource>().mute) { return; }

		// Music changed?
		if (GetComponent<AudioSource>().clip != gInGameMusic[musicIndex])
		{
			// Change music
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().clip = gInGameMusic[musicIndex];
			if (GetComponent<AudioSource>().clip != null)
			{
				GetComponent<AudioSource>().Play();
				GetComponent<AudioSource>().volume = gFullVolume;
			}
		}
		else
		{
			// Continue current music
			if (!GetComponent<AudioSource>().isPlaying)
			{
				UnpauseGameMusic();
			}
		}
	}


	/// <summary> Starts the title music </summary>
	public void StartTitleMusic()
	{
		GetComponent<AudioSource>().clip = gMusicTitle;
		if (!GetComponent<AudioSource>().mute)
		{
			GetComponent<AudioSource>().Play();
			GetComponent<AudioSource>().volume = gFullVolume;
		}
	}
	
	
	/// <summary> Starts the game over music </summary>
	public void StartGameOverMusic()
	{
		if (!GetComponent<AudioSource>().mute)
		{
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().PlayOneShot(gMusicGameOver);
		}
	}
	
	
	/// <summary> Pauses the game music </summary>
	public void PauseGameMusic()
	{
		if (!GetComponent<AudioSource>().mute && (GetComponent<AudioSource>().clip != null))
		{
			GetComponent<AudioSource>().volume = 0.0f;
			GetComponent<AudioSource>().Pause();
		}
	}
	
	
	/// <summary> Unpauses (and fades in) the game music </summary>
	public void UnpauseGameMusic()
	{
		if (!GetComponent<AudioSource>().mute && (GetComponent<AudioSource>().clip != null))
		{
			GetComponent<AudioSource>().Play();
			gFadeMult = 0.0f;
		}
	}


	/// <summary> Pauses the game music </summary>
	public void StopGameMusic()
	{
		if (!GetComponent<AudioSource>().mute && (GetComponent<AudioSource>().clip != null))
		{
			GetComponent<AudioSource>().Stop();
		}
	}
	
	
	/// <summary> Toggles the music on/off </summary>
	/// <returns> True if audio is enabled, false if muted </returns>
	public bool ToggleMusic()
	{
		GetComponent<AudioSource>().mute = !GetComponent<AudioSource>().mute;
		if (!GetComponent<AudioSource>().mute && (GetComponent<AudioSource>().clip != null)) { GetComponent<AudioSource>().Play(); }

		return !GetComponent<AudioSource>().mute;
	}

	
	/// <summary> Toggles sound effects on/off </summary>
	/// <returns> True if audio is enabled, false if muted </returns>
	public bool ToggleSoundEffects()
	{
		bool mute = !gSFXAudioSources[0].mute;

		foreach (AudioSource aSource in gSFXAudioSources)
		{
			aSource.mute = mute;
		}

		return !mute;
	}
}
