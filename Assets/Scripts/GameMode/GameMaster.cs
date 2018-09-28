﻿using System;
using UnityEngine;

public partial class GameMaster : MonoBehaviour
{
	enum ControllerTypes { KeyboardWASD, KeyboardArrows, Gamepad, Touchscreen };
	readonly ControllerTypes[] controllerTypes = { ControllerTypes.KeyboardWASD, ControllerTypes.KeyboardArrows };
	enum GameStates { Menu, PreGame, Gameplay, Paused, GameOver };

	#region Inspector variables

	[Header("Prefabs")]
	[SerializeField] Tower towerPrefab = null;

	[Header("View Layouts")]
	[SerializeField] ViewLayout viewLayout1Tower = null;
	[SerializeField] ViewLayout viewLayout2Towers = null;

	[Header("Level tuning")]
	[SerializeField] int ringFillCapacityMin = 45;
	[SerializeField] int ringFillCapacityMax = 150;
	[SerializeField] int levelMin = 1;
	[SerializeField] int levelMax = 33;

	#endregion	// Inspector variables

	GameMode gameMode;
	GameStates gameState;
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

	bool IsPlayerBarFull() { return (playerBarValue >= playerBarCapacity); }
	public bool IsGameOver { get { return (gameState == GameStates.GameOver); } }

	/// <summary> Singleton instance </summary>
	public static GameMaster instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		UnityEngine.SceneManagement.SceneManager.LoadScene("UI", UnityEngine.SceneManagement.LoadSceneMode.Additive);
	}

	/// <summary> Called before first Update() </summary>
	void Start()
	{
		SetGameState(GameStates.Menu);
	}

	/// <summary> Called once per frame </summary>
	void Update()
	{
		float dTime = Time.deltaTime;

		switch (gameState)
		{
			case GameStates.Gameplay: UpdateGameplay(dTime); break;

			default: break;
		}
	}

	/// <summary> Called once per frame during this GameState </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	void UpdateGameplay(float _dTime)
	{
		UpdateLevelProgress(_dTime);

		// Update tower/s
		int scoreChainThisFrame = 0;
		for (int i = 0; i < towers.Length; ++i)
		{
			int scoreChainFromTower;
			towers[i].UpdateTower(_dTime, out scoreChainFromTower);
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
		Environment.instance.UpdateEffects(_dTime);

		// Check for level complete
		if (IsPlayerBarFull())
		{
			// Ensure player bar has not overflowed
			playerBarValue = playerBarCapacity;

//			LevelComplete();
		}
	}

	/// <summary> Changes to a new Game State </summary>
	/// <param name="_gameState"> GameStates.* state to set </param>
	void SetGameState(GameStates _gameState)
	{
		gameState = _gameState;

		switch (gameState)
		{
			case GameStates.Menu:
				UIMaster.instance.mainMenu.gameObject.SetActive(true);
				break;

			case GameStates.Gameplay:
				break;

			default:
				break;
		}
	}

	/// <summary> Starts the specified GameMode </summary>
	/// <param name="_gameModeType"> Which Game Mode to start </param>
	public void StartGame(GameMode.GameModeTypes _gameModeType)
	{
		switch (_gameModeType)
		{
			case GameMode.GameModeTypes.Original:	gameMode = gameObject.AddComponent<GameModeOriginal>();		break;
			case GameMode.GameModeTypes.Arcade:		gameMode = gameObject.AddComponent<GameModeArcade>();		break;
			case GameMode.GameModeTypes.PVPLocal:	gameMode = gameObject.AddComponent<GameModePVP>();			break;

			default: throw new UnityException("Unhandled Game Mode Type " + _gameModeType);
		}

		ViewLayout viewLayout;
		switch (gameMode.NumTowers)
		{
			case 1: viewLayout = viewLayout1Tower; break;
			case 2: viewLayout = viewLayout2Towers; break;

			default: throw new UnityException("Unhandled tower layout for '" + gameMode.NumTowers + "' towers");
		}
		TowerCamera.instance.SetLayout(viewLayout);
		GroundController.instance.SetLayout(viewLayout);

		towers = new Tower[gameMode.NumTowers];
		for (int i = 0; i < towers.Length; ++i)
		{
			Tower tower = Instantiate(towerPrefab);
			tower.basePosition = viewLayout.towerPositions[i];

			switch (controllerTypes[i])
			{
				case ControllerTypes.KeyboardWASD:
					{
						TowerControlKeyboard controller = tower.gameObject.AddComponent<TowerControlKeyboard>();
						tower.controller = controller;
						controller.Init(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.Space);
					}
					break;

				case ControllerTypes.KeyboardArrows:
					{
						TowerControlKeyboard controller = tower.gameObject.AddComponent<TowerControlKeyboard>();
						tower.controller = controller;
						controller.Init(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Return);
					}
					break;

				case ControllerTypes.Gamepad:
					tower.controller = tower.gameObject.AddComponent<TowerControlGamepad>();
					break;

				default:
					throw new UnityException("Unhandled Controller Type " + controllerTypes[i]);
			}

			towers[i] = tower;
		}

		levelIncreaseRate = gameMode.LevelIncreaseRate;
		SetStartingLevel(gameMode.AlwaysStartOnLevel1 ? 1 : PlayerPrefs.GetInt(Constants.ppStartingLevel, 1));
		SetNewLevel(0.0f, true);
		ResetScore();

		SetGameState(GameStates.Gameplay);
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


	/// <summary> Resets the score & the player's progress </summary>
	void ResetScore()
	{
		score = 0;
		ResetPlayerBar();
	}
	
	/// <summary> Gets the current progress through all levels </summary>
	/// <returns> Level progress between from 0 to 1 </returns>
	public float GetProgressThroughLevels(bool _capTo1 = false)
	{
		float progress = ((level - Convert.ToSingle(levelMin)) / Convert.ToSingle(levelMax - levelMin));

		if (_capTo1 && (progress > 1f))
			progress = 1f;

		return progress;
	}

	/// <summary> Restarts with the previous settings </summary>
	public void RestartGame()
	{
		ResetScore();
		gameMode.GameHasBegun();
		for (int i = 0; i < towers.Length; ++i)
			towers[i].ReplayGame();
		UnpauseGame();
	}

	/// <summary> Pauses the gameplay </summary>
	public void Pause()
	{
		Environment.instance.PauseAllShockwaves(true);
		Environment.instance.musicController.PauseGameMusic();
		enabled = false;
		gameState = GameStates.Paused;
	}

	/// <summary> Resumes gameplay </summary>
	public void UnpauseGame()
	{
		Environment.instance.PauseAllShockwaves(false);
		Environment.instance.musicController.UnpauseGameMusic();
		enabled = true;
		gameState = GameStates.Gameplay;
	}

	/// <summary> Starts the Game Over sequence </summary>
	public void GameOver()
	{
		Environment.instance.GameOver();
		UIMaster.instance.GameOver();
		gameState = GameStates.GameOver;
	}

	/// <summary> Destroys all towers and resets game state </summary>
	public void QuitGame()
	{
		for (int i = 0; i < towers.Length; ++i)
		{
			Tower tower = towers[i];
			Destroy(tower.gameObject);
		}
		towers = null;
		RecyclePool.ClearAllPools();

		gameState = GameStates.Menu;
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
			float levelPercent = GetProgressThroughLevels(true);
			SetNewLevel(levelPercent, false);

			// If it's in gameplay, trigger the "level complete" sequence
			// if (!gFrontendMenuObject.activeSelf && !IsGameFrozen())
			//	LevelComplete();

			levelInt = Mathf.FloorToInt(level);
		}
	
		// Add to the player's progress bar
		if (gameMode.HasRingBar)
			++playerBarValue;
	}

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='_progressThroughAllLevels'> Speed scale: 0.0f = slowest, 1.0f = fastest </param>
	/// <param name="_resetTowers"> True to reset towers, false to leave them as is </param>
	public void SetNewLevel(float _progressThroughAllLevels, bool _resetTowers)
	{
		playerBarCapacity = ringFillCapacityMin + Convert.ToInt32(Convert.ToSingle(ringFillCapacityMax - ringFillCapacityMin) * _progressThroughAllLevels);

		for (int i = 0; i < towers.Length; ++i)
			towers[i].SetNewLevel(_progressThroughAllLevels, _resetTowers);

		// Update score multiplier
		scoreDifficultyMult = _progressThroughAllLevels;

		// Update background effects
		Environment.instance.flowerOfLife.SetMaxActiveMaterials(Mathf.FloorToInt(level));
		Environment.instance.groundController.SetScrollSpeed(_progressThroughAllLevels);
	}
}
