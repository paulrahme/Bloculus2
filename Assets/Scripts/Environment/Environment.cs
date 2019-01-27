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

	[Header("Prefabs")]
	[SerializeField] GameObject fallingRingPrefab = null;
	[SerializeField] GameObject shockwavePrefab = null;

	[Header("Audio")]
	[SerializeField] AudioClip levelUpAudio = null;
	[SerializeField] AudioClip levelEndedAudio = null;
	[SerializeField] AudioClip levelCompleteAudio = null;
	[SerializeField] AudioClip gameOverAudio = null;

	#endregion  // Inspector variables

	Transform myTrans;
	List<FallingRing> fallingRings = new List<FallingRing>();
	internal List<Shockwave> shockwaves = new List<Shockwave>();

	/// <summary> Singleton instance </summary>
	public static Environment instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		myTrans = transform;
	}

	/// <summary> Called once per frame from GameMaster </summary>
	public void UpdateEffects(float _dTime)
	{
		groundController.UpdateEffect();

		for (int i = 0; i < fallingRings.Count; ++i)
			fallingRings[i].UpdateFalling(_dTime);

		for (int i = 0; i < shockwaves.Count; ++i)
			shockwaves[i].UpdateRipple(_dTime);
	}

	/// <summary> Pauses/unpauses environment items + effects </summary>
	/// <param name="_paused"> True to pause, false to unpause </param>
	public void SetPaused(bool _paused)
	{
		// Falling rings
		for (int i = 0; i < fallingRings.Count; ++i)
			fallingRings[i].enabled = !_paused;

		// Shockwaves
		for (int i = 0; i < shockwaves.Count; ++i)
			shockwaves[i].enabled = _paused;

		// Background music
		if (_paused)
			musicController.PauseGameMusic();
		else
			musicController.UnpauseGameMusic();
	}

	/// <summary> Recycles all spawned items </summary>
	public void ClearAllEffects()
	{
		for (int i = 0; i < fallingRings.Count; ++i)
			RecyclePool.Recycle(RecyclePool.PoolTypes.FallingRing, fallingRings[i].gameObject);
		fallingRings.Clear();

		for (int i = 0; i < shockwaves.Count; ++i)
			RecyclePool.Recycle(RecyclePool.PoolTypes.Shockwave, shockwaves[i].gameObject);
		shockwaves.Clear();
	}

	#region Background

	/// <summary> Changes the background to match the level </summary>
	/// <param name="_level"> Level numer </param>
	/// <returns> The index of the group of levels this one is in </returns>
	public int SetBackground(float _level)
	{
		float progress;
		int levelGroup;
		if (_level < 8f)
		{
			progress = _level / 8.0f;
			levelGroup = 0;
		}
		else if (_level < 16f)
		{
			progress = (_level - 8f) / 8f;
			levelGroup = 1;
		}
		else if (_level < 24f)
		{
			progress = (_level - 16f) / 8f;
			levelGroup = 2;
		}
		else if (_level < GameMaster.Tuning.levelMax)
		{
			progress = (_level - 24f) / 8f;
			levelGroup = 3;
		}
		else
		{
			progress = 0;
			levelGroup = 4;
		}

		BgColor colors = bgColors[levelGroup];
		TowerCamera.instance.SetBackgroundColor(Color.Lerp(colors.colorStart, colors.colorEnd, progress));
		GroundController.instance.SetMaterial(levelGroup);

		return levelGroup;
	}

	/// <summary> Updates the background colour, texture etc. </summary>
	public void UpdateBackground(float _level)
	{
		int musicIdx = SetBackground(_level);
		musicController.StartGameMusic(musicIdx);
	}

	#endregion // Background

	#region Falling rings

	/// <summary> Recycles or creates a falling ring as specified </summary>
	/// <param name="_pos"> Starting position </param>
	/// <param name="_scale"> Scale </param>
	/// <param name="_color"> Colour </param>
	public void SpawnFallingRing(Vector3 _pos, float _scale, Color _color)
	{
		GameObject gameObj = RecyclePool.RetrieveOrCreate(RecyclePool.PoolTypes.FallingRing, fallingRingPrefab);
		FallingRing fallingRing = gameObj.GetComponent<FallingRing>();
		fallingRing.Init(_pos, _scale, _color);
		fallingRings.Add(fallingRing);
	}

	/// <summary> Called from the ring when it hits the ground </summary>
	/// <param name="_ring"> Ring that just finished falling </param>
	public void RingFinishedFalling(FallingRing _ring)
	{
		RecyclePool.Recycle(RecyclePool.PoolTypes.FallingRing, _ring.gameObject);
		fallingRings.Remove(_ring);
	}

	/// <summary> Clears all spawned falling rings </summary>
	public void DestroyAllFallingRings()
	{
		for (int i = 0; i < fallingRings.Count; ++i)
			Destroy(fallingRings[i].gameObject);
		fallingRings.Clear();
	}

	#endregion // Falling rings

	#region Shockwaves

	/// <summary> Creates (or reuses) a shockwave GameObject </summary>
	/// <param name="_position"> Centre position </param>
	/// <param name="_color"> Ripple's colour </param>
	public void StartRipple(Vector3 _position, Color _color)
	{
		GameObject gameObj = RecyclePool.RetrieveOrCreate(RecyclePool.PoolTypes.Shockwave, shockwavePrefab);

		// Set position & rotation
		gameObj.transform.parent = myTrans;
		gameObj.transform.position = _position;
		gameObj.transform.localScale = Vector3.zero;

		// (Re)start popup animation
		Shockwave shockwaveScript = gameObj.GetComponent<Shockwave>();
		shockwaveScript.Init(_color);

		shockwaves.Add(shockwaveScript);
	}

	/// <summary> Called from a shockwave when it finishes growing + fading </summary>
	/// <param name="_shockwave"> Shockwave that just finished </param>
	public void ShockwaveFinished(Shockwave _shockwave)
	{
		RecyclePool.Recycle(RecyclePool.PoolTypes.Shockwave, _shockwave.gameObject);
		shockwaves.Remove(_shockwave);
	}

	#endregion // Shockwaves

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

	public void PlayLevelUpAudio()
	{
		audioSource.PlayOneShot(levelUpAudio);
	}
}
