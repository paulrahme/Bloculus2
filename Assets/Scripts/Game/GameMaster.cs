using System;
using UnityEngine;

public partial class GameMaster : MonoBehaviour
{
	public enum ControllerTypes { KeyboardWASD, KeyboardArrows, Gamepad, Touchscreen };
	enum GameStates { Menu, PreGame, Gameplay, Paused, GameOver };

	#region Inspector variables

	[Header("Prefabs")]
	[SerializeField] Tower towerPrefab = null;

	[Header("View Layouts")]
	[SerializeField] ViewLayout viewLayout1Tower = null;
	[SerializeField] ViewLayout viewLayout2Towers = null;

	[Header("Level tuning")]
	public int levelMax = 33;

	#endregion   // Inspector variables

	#region Non-inspector variables + properties

	ControllerTypes[] controllerTypes = { ControllerTypes.KeyboardWASD, ControllerTypes.KeyboardArrows };
	GameMode gameMode;
	GameStates gameState;
	Player[] players;
	float levelIncreaseRate;
	public static System.Random randomGen = new System.Random();

	public bool IsGameOver { get { return (gameState == GameStates.GameOver); } }

	#endregion // Non-inspector variables + properties

	/// <summary> Singleton instance </summary>
	public static GameMaster instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;

		Load();

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
		}
	}

	/// <summary> Called once per frame during this GameState </summary>
	/// <param name='_dTime'> Time elapsed since last Update() </param>
	void UpdateGameplay(float _dTime)
	{
		// Update each player/tower
		for (int i = 0; i < players.Length; ++i)
		{
			// Update the level
			UI_PlayerHUD playerHUD = players[i].hud;
			playerHUD.UpdateLevelProgress(gameMode.LevelProgressRate * _dTime);

			// Update the tower
			int scoreChainFromTower;
			players[i].tower.UpdateTower(_dTime, out scoreChainFromTower);

			// Add score from tower, if any
			if (scoreChainFromTower != 0)
				playerHUD.AddScore(1 << scoreChainFromTower);
		}

		// Update ground
		Environment.instance.UpdateEffects(_dTime);
	}

	#region Player controls

	#region Game states

	public ControllerTypes GetPlayerControls(int _playerIdx)
	{
		return controllerTypes[_playerIdx];
	}

	public ControllerTypes ChangePlayerControls(int _playerIdx)
	{
		ControllerTypes controllerType = controllerTypes[_playerIdx];
		ControllerTypes prevType = controllerType;
		int controlCount = Enum.GetValues(typeof(ControllerTypes)).Length;
		bool finished = false;
		while (!finished)
		{
			// Increment with wraparound
			++controllerType;
			if ((int)controllerType == controlCount)
				controllerType = (ControllerTypes)0;

			finished = true;
			// If rolled back around to same value, give up
			if (controllerType != prevType)
			{
				// Check no other player has the same control type
				for (int i = 0; i < controllerTypes.Length; ++i)
				{
					finished &= (controllerTypes[i] != controllerType);
				}

				// Found a valid one
				if (finished)
				{
					controllerTypes[_playerIdx] = controllerType;
					SaveControllerTypes();
				}
			}
		}

		return controllerType;
	}

	#endregion // Player controls

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
			case GameMode.GameModeTypes.Original:				gameMode = gameObject.AddComponent<GameModeOriginal>();			break;
			case GameMode.GameModeTypes.Arcade:					gameMode = gameObject.AddComponent<GameModeArcade>();			break;
			case GameMode.GameModeTypes.PVPLocal_Continuous:	gameMode = gameObject.AddComponent<GameModePVPContinuous>();	break;
			case GameMode.GameModeTypes.PVPLocal_Race:			gameMode = gameObject.AddComponent<GameModePVPRace>();			break;

			default: throw new UnityException("Unhandled Game Mode Type " + _gameModeType);
		}

		ViewLayout viewLayout;
		switch (gameMode.NumPlayers)
		{
			case 1: viewLayout = viewLayout1Tower; break;
			case 2: viewLayout = viewLayout2Towers; break;

			default: throw new UnityException("Unhandled tower layout for '" + gameMode.NumPlayers + "' towers");
		}

		TowerCamera.instance.SetLayout(viewLayout);
		GroundController.instance.SetLayout(viewLayout);
		UIMaster.instance.hud.ClearScores();
		players = new Player[gameMode.NumPlayers];
		int startingLevel = (gameMode.AlwaysStartOnLevel1 ? 1 : PlayerPrefs.GetInt(Constants.ppStartingLevel, 1));

		for (int i = 0; i < players.Length; ++i)
		{
			Tower newTower = Instantiate(towerPrefab);
			newTower.basePosition = viewLayout.towerPositions[i];

			switch (controllerTypes[i])
			{
				case ControllerTypes.KeyboardWASD:
					{
						TowerControlKeyboard controller = newTower.gameObject.AddComponent<TowerControlKeyboard>();
						newTower.controller = controller;
						controller.Init(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.Space);
					}
					break;

				case ControllerTypes.KeyboardArrows:
					{
						TowerControlKeyboard controller = newTower.gameObject.AddComponent<TowerControlKeyboard>();
						newTower.controller = controller;
						controller.Init(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Return);
					}
					break;

				case ControllerTypes.Gamepad:
					newTower.controller = newTower.gameObject.AddComponent<TowerControlGamepad>();
					break;

				default:
					throw new UnityException("Unhandled Controller Type " + controllerTypes[i]);
			}

			players[i] = new Player(controllerTypes[i], newTower, UIMaster.instance.hud.AddPlayerHUD("Player " + (i + 1), startingLevel));
		}

		SetStartingLevel(startingLevel);
		SetNewLevel(0, true);

		SetGameState(GameStates.Gameplay);
	}

	/// <summary> Sets the speeds & tower layout </summary>
	/// <param name='_level'> New level to set </param>
	/// <param name="_resetTowers"> True to reset towers, false to leave them as is </param>
	public void SetNewLevel(int _level, bool _resetTowers)
	{
		for (int i = 0; i < players.Length; ++i)
			players[i].SetLevel(_level, _resetTowers);

		// Update background effects
		Environment.instance.flowerOfLife.SetMaxActiveMaterials(Mathf.FloorToInt(players[0].hud.LevelInt));
		Environment.instance.groundController.SetScrollSpeed(_level);
	}

	/// <summary> Sets the starting level and adjusts game speeds accordingly </summary>
	/// <param name='_level'> New level to start game on </param>
	public void SetStartingLevel(int _level)
	{
		for (int i = 0; i < players.Length; ++i)
			players[i].SetLevel(_level);

		Environment.instance.UpdateBackground(_level, false);
	}

	/// <summary> Restarts with the previous settings </summary>
	public void RestartGame()
	{
		gameMode.GameHasBegun();
		for (int i = 0; i < players.Length; ++i)
			players[i].ReplayGame();
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
		for (int i = 0; i < players.Length; ++i)
			players[i].OnDestroy();

		players = null;
		RecyclePool.ClearAllPools();

		gameState = GameStates.Menu;
	}

	#endregion // Game states

	#region Saving & Loading

	const string ppPlayerControlsPrefix = "PlayerControls";

	/// <summary> Loads everything from PlayerPrefs </summary>
	void Load()
	{
		for (int i = 0; i < controllerTypes.Length; ++i)
		{
			if (PlayerPrefs.HasKey(ppPlayerControlsPrefix + i))
				controllerTypes[i] = (ControllerTypes)PlayerPrefs.GetInt(ppPlayerControlsPrefix + i);
		}
	}

	/// <summary> Saves player control preferences </summary>
	void SaveControllerTypes()
	{
		for (int i = 0; i < controllerTypes.Length; ++i)
			PlayerPrefs.SetInt(ppPlayerControlsPrefix + i, (int)controllerTypes[i]);
		PlayerPrefs.Save();
	}

	#endregion
}
