using UnityEngine;

public class MusicController : MonoBehaviour
{
	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] AudioSource audioSource = null;

	[Header("Audio clips")]
	[SerializeField] AudioClip titleMusic = null;
	[SerializeField] AudioClip[] inGameMusic = null;
	[SerializeField] float fadeInSpeed = 0.5f;

	#endregion   // Inspector variables

	public bool MusicEnabled { get; private set; }
	public bool SfxEnabled { get; private set; }
	float fullVolume = -1;
	float fadeMult = 1.1f;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		LoadIfNecessary();
	}

	/// <summary> If not initialised, loads saved settings </summary>
	void LoadIfNecessary()
	{
		// Not yet loaded?
		if (fullVolume < 0)
		{
			// Use setting from Unity editor as full volume
			fullVolume = audioSource.volume;

			// Load music/sound settings?
			if (PlayerPrefs.HasKey(Constants.PPKeys.MusicEnabled.ToString()))
			{
				bool musicEnabledSetting = (PlayerPrefs.GetInt(Constants.PPKeys.MusicEnabled.ToString(), 1) == 1);
				if (MusicEnabled != musicEnabledSetting)
					ToggleMusic();

				bool sfxEnabledSetting = (PlayerPrefs.GetInt(Constants.PPKeys.SoundEnabled.ToString(), 1) == 0);
				if (SfxEnabled != sfxEnabledSetting)
					ToggleSFX();
			}
			else
			{
				// No saved settings, default both to on and save now
				MusicEnabled = SfxEnabled = true;
				SavePlayerPrefs();
			}
		}
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

	/// <summary> Starts the music for the current level </summary>
	public void StartGameMusic(int musicIndex)
	{
		LoadIfNecessary();

		// Music changed?
		if (audioSource.clip != inGameMusic[musicIndex])
		{
			// Change music
			audioSource.Stop();
			audioSource.clip = inGameMusic[musicIndex];

			if (MusicEnabled && (audioSource.clip != null))
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
		LoadIfNecessary();

		if (audioSource.clip != null)
			audioSource.Stop();

		audioSource.clip = titleMusic;
		if (MusicEnabled)
		{
			audioSource.Play();
			audioSource.volume = fullVolume;
		}
	}

	/// <summary> Pauses the game music </summary>
	public void PauseGameMusic()
	{
		if (MusicEnabled && (audioSource.clip != null))
		{
			audioSource.volume = 0.0f;
			audioSource.Pause();
		}
	}

	/// <summary> Unpauses (and fades in) the game music </summary>
	public void UnpauseGameMusic()
	{
		if (MusicEnabled && (audioSource.clip != null))
		{
			audioSource.Play();
			fadeMult = 0.0f;
		}
	}

	/// <summary> Pauses the game music </summary>
	public void StopGameMusic()
	{
		if (audioSource.clip != null)
			audioSource.Stop();
	}

	/// <summary> Toggles the music on/off </summary>
	/// <returns> True if audio is enabled, false if muted </returns>
	public bool ToggleMusic()
	{
		MusicEnabled = !MusicEnabled;
		if (audioSource.clip != null)
		{
			if (MusicEnabled)
				audioSource.Play();
			else
				audioSource.Stop();
		}

		SavePlayerPrefs();	

		return MusicEnabled;
	}
	
	/// <summary> Toggles sound effects on/off </summary>
	/// <returns> True if audio is enabled, false if muted </returns>
	public bool ToggleSFX()
	{
		SfxEnabled = !SfxEnabled;

		SavePlayerPrefs();

		return SfxEnabled;
	}

	/// <summary> Saves the current audio settings </summary>
	void SavePlayerPrefs()
	{
		PlayerPrefs.SetInt(Constants.PPKeys.MusicEnabled.ToString(), MusicEnabled ? 1 : 0);
		PlayerPrefs.SetInt(Constants.PPKeys.SoundEnabled.ToString(), SfxEnabled ? 1 : 0);
		PlayerPrefs.Save();
	}
}