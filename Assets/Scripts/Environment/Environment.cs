using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
	[System.Serializable]
	public class BgColor
	{
		public Color colorStart = Color.black;
		public Color colorEnd = Color.white;
	}

	#region Inspector variables

	[Header("Hierachy")]
	public GroundController groundController = null;
	public FlowerOfLife flowerOfLife = null;
	public MusicController musicController = null;
	[SerializeField] AudioSource audioSource = null;

	[Header("Background")]
	[SerializeField] BgColor[] bgColors = null;

	[Header("Audio")]
	[SerializeField] AudioClip levelUpAudio = null;
	[SerializeField] AudioClip levelEndedAudio = null;
	[SerializeField] AudioClip levelCompleteAudio = null;
	[SerializeField] AudioClip gameOverAudio = null;

	#endregion  // Inspector variables

	public List<Shockwave> shockwaves = new List<Shockwave>();

	/// <summary> Singleton instance </summary>
	public static Environment instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

		for (int i = 0; i < shockwaves.Count; ++i)
			shockwaves[i].UpdateRipple(dTime);
	}

	/// <summary> Changes the background to match the level </summary>
	/// <param name="_level"> Level numer </param>
	/// <param name="_levelMax"> Highest level </param>
	/// <returns> The index of the group of levels this one is in </returns>
	public int SetBackground(float _level, float _levelMax)
	{
		float progress;
		int levelGroup;
		if (_level < 8.0f)
		{
			progress = _level / 8.0f;
			levelGroup = 0;
		}
		else if (_level < 16.0f)
		{
			progress = (_level - 8.0f) / 8.0f;
			levelGroup = 1;
		}
		else if (_level < 24.0f)
		{
			progress = (_level - 16.0f) / 8.0f;
			levelGroup = 2;
		}
		else if (_level < _levelMax)
		{
			progress = (_level - 24.0f) / 8.0f;
			levelGroup = 3;
		}
		else
		{
			progress = 0;
			levelGroup = 4;
		}

		BgColor colors = bgColors[levelGroup];
		TowerCamera.instance.SetBackgroundColor(Color.Lerp(colors.colorStart, colors.colorEnd, progress));
		GroundController.Instance.SetMaterial(levelGroup);

		return levelGroup;
	}

	/// <summary> Updates the background colour, texture etc. </summary>
	/// <param name="_changeMusic"> When true, change background music as necessary </param>
	public void UpdateBackground(float _level, float _levelMax, bool _changeMusic)
	{
		int musicIdx = SetBackground(_level, _levelMax);
		if (_changeMusic)
			musicController.StartGameMusic(musicIdx);

	}

	public void GameOver()
	{
		musicController.StopGameMusic();
		audioSource.PlayOneShot(gameOverAudio);
	}

	public void LevelComplete(bool _quickContinue = false)
	{
		musicController.PauseGameMusic();
		audioSource.PlayOneShot(levelCompleteAudio);
		if (_quickContinue)
			musicController.UnpauseGameMusic();
	}

	public void LevelEnded()
	{
		musicController.PauseGameMusic();
		audioSource.PlayOneShot(levelEndedAudio);
	}

	public void LevelUp()
	{
		audioSource.PlayOneShot(levelUpAudio);
	}

	public void PauseAllShockwaves(bool _paused)
	{
		for (int i = 0; i < shockwaves.Count; ++i)
			shockwaves[i].enabled = _paused;
	}
}
