using UnityEngine;

public class MusicController : MonoBehaviour
{
	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] AudioSource	audioSource = null;

	[Header("Audio clips")]
	[SerializeField] AudioClip		titleMusic = null;
	[SerializeField] AudioClip[]	inGameMusic = null;
	[SerializeField] float			fadeInSpeed = 0.5f;

	#endregion	// Inspector variables

	float							fullVolume;
	float							fadeMult = 1.1f;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		fullVolume = audioSource.volume;
		if (PlayerPrefs.GetInt(Constants.PPKeys.MusicEnabled.ToString(), 1) == 0) { ToggleMusic(); }
		if (PlayerPrefs.GetInt(Constants.PPKeys.SoundEnabled.ToString(), 1) == 0) { ToggleSoundEffects(); }
	}
	
	/// <summary> Called once per frame </summary>
	void Update()
	{
		if (fadeMult < 1.0f)
		{
			fadeMult += fadeInSpeed * Time.deltaTime;
			audioSource.volume = fullVolume * fadeMult;
		}
	}

	/// <summary> Checks if music is enabled or muted </summary>
	/// <returns> True if enabled, false if muted </returns>
	public bool IsMusicEnabled()
	{
		return !audioSource.mute;
	}

	/// <summary> Starts the music for the current level </summary>
	public void StartGameMusic(int musicIndex)
	{
		if (audioSource.mute)
			return;

		// Music changed?
		if (audioSource.clip != inGameMusic[musicIndex])
		{
			// Change music
			audioSource.Stop();
			audioSource.clip = inGameMusic[musicIndex];
			if (audioSource.clip != null)
			{
				audioSource.Play();
				audioSource.volume = fullVolume;
			}
		}
		else
		{
			// Continue current music
			if (!audioSource.isPlaying)
				UnpauseGameMusic();
		}
	}

	/// <summary> Starts the title music </summary>
	public void StartTitleMusic()
	{
		audioSource.clip = titleMusic;
		if (!audioSource.mute)
		{
			audioSource.Play();
			audioSource.volume = fullVolume;
		}
	}
	
	/// <summary> Pauses the game music </summary>
	public void PauseGameMusic()
	{
		if (!audioSource.mute && (audioSource.clip != null))
		{
			audioSource.volume = 0.0f;
			audioSource.Pause();
		}
	}
	
	/// <summary> Unpauses (and fades in) the game music </summary>
	public void UnpauseGameMusic()
	{
		if (!audioSource.mute && (audioSource.clip != null))
		{
			audioSource.Play();
			fadeMult = 0.0f;
		}
	}

	/// <summary> Pauses the game music </summary>
	public void StopGameMusic()
	{
		if (!audioSource.mute && (audioSource.clip != null))
			audioSource.Stop();
	}
	
	/// <summary> Toggles the music on/off </summary>
	/// <returns> True if audio is enabled, false if muted </returns>
	public bool ToggleMusic()
	{
		audioSource.mute = !audioSource.mute;
		if (!audioSource.mute && (audioSource.clip != null))
			audioSource.Play();

		return !audioSource.mute;
	}
	
	/// <summary> Toggles sound effects on/off </summary>
	/// <returns> True if audio is enabled, false if muted </returns>
	public bool ToggleSoundEffects()
	{
/*		bool mute = !gSFXAudioSources[0].mute;

		foreach (AudioSource aSource in gSFXAudioSources)
		{
			aSource.mute = mute;
		}

		return !mute;
*/
		return false;
	}
}
