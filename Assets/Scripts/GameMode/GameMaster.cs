using System;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
	public enum GameModes { Original, Arcade, TimeChallenge, SpeedChallenge, ScoreChallenge };

	#region Inspector variables

	[SerializeField] GameObject towerPrefab = null;
	[SerializeField] int ringFillCapacityMin = 45;
	[SerializeField] int ringFilleCapacityMax = 150;
	[SerializeField] float levelIncreaseRateFull = 0.00666f;
	[SerializeField] float levelIncreaseRateShorter = 0.01666f;
	[SerializeField] float levelIncreaseRateArcade = 0.02333f;
	[SerializeField] int levelMin = 1;
	[SerializeField] int levelMax = 33;

	#endregion	// Inspector variables

	internal GameModes gameMode;
	Tower[] towers;
	float level;
	int levelInt;
	float levelIncreaseRate;
	int score;
	int playerBarCapacity;
	int playerBarValue;
	public static System.Random randomGen = new System.Random();
	float startingLevel;
	float scoreDifficultyMult;

	float	GetLevelPercentCapped() { return Mathf.Min(GetProgressThroughLevels(), 1.0f); }
	bool	IsPlayerBarFull() { return (playerBarValue >= playerBarCapacity); }

	/// <summary> Singleton instance </summary>
	public static GameMaster instance;

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
		SetGameMode(GameModes.Original);
		SetNewLevel(0.0f, true);

		ResetScore();
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

		UpdateLevelProgress(dTime);

		// Update tower/s
		int scoreChainThisFrame = 0;
		for (int i = 0; i < towers.Length; ++i)
		{
			int scoreChainFromTower;
			towers[i].UpdateTower(dTime, out scoreChainFromTower);
			scoreChainThisFrame += scoreChainFromTower;
		}

		// Add score, if any
		if (scoreChainThisFrame != 0)
		{
			int scoreThisFrame = 1 << scoreChainThisFrame;
			scoreThisFrame += Convert.ToInt32(Convert.ToSingle(scoreThisFrame) * scoreDifficultyMult);
			scoreThisFrame += Convert.ToInt32(level * 3.0f / Convert.ToSingle(levelMax - levelMin));
			scoreThisFrame *= 10;
			score += scoreThisFrame;
		}

		// Update ground
		GroundController.Instance.UpdateEffect();

		// Check for level complete
		if (IsPlayerBarFull())
		{
			// Ensure player bar has not overflowed
			playerBarValue = playerBarCapacity;

//			LevelComplete();
		}

#if UNITY_EDITOR
		HandleScreenshotKey();
#endif
	}

#if UNITY_EDITOR
	/// <summary> Handles debug key/s for saving screenshots </summary>
	void HandleScreenshotKey()
	{
		// Save screenshot
		if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
		{
			string fileName = "Screenshots/"+Screen.width+"x"+Screen.height+"_"+System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm_ss")+".png";
			ScreenCapture.CaptureScreenshot(fileName);
			Debug.Log("Saved screenshot '"+fileName+"'");
		}
	}
#endif

	/// <summary> Sets a new game mode & performs any appropriate actions </summary>
	/// <param name='_gameMode'> eGameMode.... name </param>
	public void SetGameMode(GameModes _gameMode)
	{
		gameMode = _gameMode;

		towers = new Tower[1];
		towers[0] = Instantiate(towerPrefab).GetComponent<Tower>();

		switch (gameMode)
		{
			case GameModes.Original:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				levelIncreaseRate = levelIncreaseRateFull;
				break;

			case GameModes.Arcade:
				SetStartingLevel(1);
				levelIncreaseRate = levelIncreaseRateArcade;
				break;

			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				levelIncreaseRate = 0;
				break;

			case GameModes.ScoreChallenge:
				SetStartingLevel(PlayerPrefs.GetInt(Constants.ppkPPStartingLevel, 1));
				levelIncreaseRate = levelIncreaseRateShorter;
				break;

			default:
				throw new Exception("Unhandled GameMode '" + gameMode + "'");
		}
	}

	/// <summary> Sets the starting level and adjusts game speeds accordingly </summary>
	/// <param name='level'> New level to start game on </param>
	public void SetStartingLevel(int _level)
	{
		startingLevel = _level;
		level = startingLevel;
		levelInt = Mathf.FloorToInt(startingLevel);
		for (int i = 0; i < towers.Length; ++i)
			towers[i].RestoreSpeeds();
		Environment.instance.UpdateBackground(level, levelMax, false);
	}


	/// <summary> Resets the score & the jar </summary>
	void ResetScore()
	{
		score = 0;
		ResetPlayerBar();
	}
	
	/// <summary> Triggered as soon as the frontend disappears and the game begins </summary>
	public void GameHasBegun()
	{
		switch (gameMode)
		{
			case GameModes.Original:
			case GameModes.Arcade:
				break;
				
			case GameModes.TimeChallenge:
			case GameModes.SpeedChallenge:
//				timeChallengeStartTime = Time.fixedTime;
				break;
				
			case GameModes.ScoreChallenge:
//				timeChallengeStartTime = Time.fixedTime;
				break;

			default:
				throw new Exception("Unhandled GameMode " + gameMode);
		}
		Environment.instance.UpdateBackground(level, levelMax, true);
	}

	/// <summary> Gets the current progress through all levels </summary>
	/// <returns> Level progress between from 0 to 1 </returns>
	public float GetProgressThroughLevels()
	{
		return ((level - Convert.ToSingle(levelMin)) / Convert.ToSingle(levelMax - levelMin));
	}

	/// <summary> Restarts with the previous settings </summary>
	void ReplayGame()
	{
		ResetScore();
		GameHasBegun();
		for (int i = 0; i < towers.Length; ++i)
			towers[i].ReplayGame();
	}

	/// <summary> Closes the popup window & continues gameplay </summary>
	public void UnpauseGame()
	{
		Environment.instance.PauseAllShockwaves(false);
		Environment.instance.musicController.UnpauseGameMusic();
	}

	/// <summary> Starts the Game Over sequence </summary>
	public void GameOver()
	{
		Environment.instance.GameOver();
	}

	/// <summary> Resets the player's progress bar </summary>
	void ResetPlayerBar()
	{
		playerBarValue = 0;
	}

	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='_playSound'> True to play the "level up" sound </param>
	void QuickLevelUp(bool _playSound = true)
	{
		if (_playSound)
			Environment.instance.LevelUp();

		Environment.instance.UpdateBackground(level, levelMax, true);
	}

	/// <summary> Updates the gradual level increase, reacting if the level has been completed </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	void UpdateLevelProgress(float _dTime)
	{
		level += levelIncreaseRate * _dTime;

		// Has level just changed?
		if (Mathf.FloorToInt(level) != levelInt)
		{
			// Update speeds & tower layout for next level
			float levelPercent = GetLevelPercentCapped();
			SetNewLevel(levelPercent, false);

			// If it's in gameplay, trigger the "level complete" sequence
			// if (!gFrontendMenuObject.activeSelf && !IsGameFrozen())
			//	LevelComplete();

			levelInt = Mathf.FloorToInt(level);
		}

	
		// Add to the player's progress bar
		if (gameMode != GameModes.ScoreChallenge)
			++playerBarValue;
	}

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='_progressThroughAllLevels'> Speed scale: 0.0f = slowest, 1.0f = fastest </param>
	/// <param name="_resetTowers"> True to reset towers, false to leave them as is </param>
	public void SetNewLevel(float _progressThroughAllLevels, bool _resetTowers)
	{
		playerBarCapacity = ringFillCapacityMin + Convert.ToInt32(Convert.ToSingle(ringFilleCapacityMax - ringFillCapacityMin) * _progressThroughAllLevels);

		for (int i = 0; i < towers.Length; ++i)
			towers[i].SetNewLevel(_progressThroughAllLevels, _resetTowers);

		// Update score multiplier
		scoreDifficultyMult = _progressThroughAllLevels;

		// Update background effects
		Environment.instance.flowerOfLife.SetMaxActiveMaterials(Mathf.FloorToInt(level));
		Environment.instance.groundController.SetScrollSpeed(_progressThroughAllLevels);

	}
}
