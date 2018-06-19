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

	[Header("Hieracchy")]
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

	#endregion	// Inspector variables

	/// <summary> Singleton instance </summary>
	public static Environment instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;
	}

	public void SetBackground(int _bgIndex, float _progress, int _musicIdx)
	{
		BgColor colors = bgColors[_bgIndex];
		TowerCamera.instance.SetBackgroundColor(Color.Lerp(colors.colorStart, colors.colorEnd, _progress));
		GroundController.Instance.SetMaterial(_bgIndex);
		if (_musicIdx >= 0)
			musicController.StartGameMusic(_musicIdx);
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
}
