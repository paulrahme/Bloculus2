using System;
using UnityEngine;

public partial class GameMaster : MonoBehaviour
{
	public enum ControllerTypes { KeyboardWASD, KeyboardArrows, Gamepad, Touchscreen };
	enum GameStates { Menu, PreGame, Gameplay, Paused, GameOver };

	#region Inspector variables

	[Header("Prefabs")]
	[SerializeField] Tower towerPrefab = null;

	[Header("Tuning")]
	[SerializeField] GameTuning tuning = null;

	[Header("View Layouts")]
	[SerializeField] ViewLayout viewLayout1Tower = null;
	[SerializeField] ViewLayout viewLayout2Towers = null;

	#endregion   // Inspector variables

	#region Non-inspector variables + properties

	ControllerTypes[] controllerTypes = { ControllerTypes.KeyboardWASD, ControllerTypes.KeyboardArrows };
	GameMode gameMode;
	GameStates gameState;
	Player[] players;
	int startingLevel = 1;
	float levelIncreaseRate;
	public static System.Random randomGen = new System.Random();

	public static GameTuning Tuning { get { return instance.tuning; } }
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
		// Update each player (and their tower)
		for (int i = 0; i < players.Length; ++i)
		{
			if (players[i].state == Player.States.Gameplay)
				players[i].UpdateGameplay(_dTime, gameMode);
		}

		// Update ground
		Environment.instance.UpdateEffects(_dTime);
	}

	#region Player controls

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

	#region Game states

	/// <summary> Changes to a new Game State </summary>
	/// <param name="_gameState"> GameStates.* state to set </param>
	void SetGameState(GameStates _gameState)
	{
		gameState = _gameState;

		switch (gameState)
		{
			case GameStates.Menu:
				UIMaster.instance.mainMenu.gameObject.SetActive(true);
				Environment.instance.musicController.StartTitleMusic();
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
		// Create GameMode
		switch (_gameModeType)
		{
			case GameMode.GameModeTypes.Original:				gameMode = gameObject.AddComponent<GameModeOriginal>();			break;
			case GameMode.GameModeTypes.Arcade:					gameMode = gameObject.AddComponent<GameModeArcade>();			break;
			case GameMode.GameModeTypes.PVPLocal_Continuous:	gameMode = gameObject.AddComponent<GameModePVPContinuous>();	break;
			case GameMode.GameModeTypes.PVPLocal_Race:			gameMode = gameObject.AddComponent<GameModePVPRace>();			break;

			default: throw new UnityException("Unhandled Game Mode Type " + _gameModeType);
		}

		startingLevel = gameMode.GetStartingLevel();

		// Set viewing positions (camera, ground, etc)
		ViewLayout viewLayout;
		switch (gameMode.NumPlayers)
		{
			case 1: viewLayout = viewLayout1Tower; break;
			case 2: viewLayout = viewLayout2Towers; break;

			default: throw new UnityException("Unhandled tower layout for '" + gameMode.NumPlayers + "' towers");
		}
		TowerCamera.instance.SetLayout(viewLayout);
		GroundController.instance.SetLayout(viewLayout);

		// Clear current players
		if (players != null)
		{
			for (int i = 0; i < players.Length; ++i)
			{
				players[i].Destroy();
				players[i] = null;
			}
		}
	
		// Create new players
		players = new Player[gameMode.NumPlayers];
		for (int i = 0; i < players.Length; ++i)
			players[i] = new Player("Player " + (i + 1), controllerTypes[i], towerPrefab, viewLayout.towerPositions[i], startingLevel, gameMode.StartingLevelProgress);

		// Update background visuals
		RefreshEnvironment(startingLevel);

		SetGameState(GameStates.Gameplay);
	}

	/// <summary> Updates the background visuals to match the level </summary>
	/// <param name='_level'> Level to update to </param>
	public void RefreshEnvironment(int _level)
	{
		Environment.instance.UpdateBackground(_level);
		Environment.instance.flowerOfLife.SetMaxActiveMaterials(_level);
		Environment.instance.groundController.SetScrollSpeed((float)_level / tuning.levelMax);
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
		Environment.instance.SetPaused(true);
		enabled = false;
		SetGameState(GameStates.Paused);
	}

	/// <summary> Resumes gameplay </summary>
	public void UnpauseGame()
	{
		Environment.instance.SetPaused(false);
		enabled = true;
		SetGameState(GameStates.Gameplay);
	}

	/// <summary> Starts the Game Over sequence </summary>
	public void GameOver(Tower _tower)
	{
		bool allPlayersFinished = true;
		for (int i = 0; i < players.Length; ++i)
		{
			Player player = players[i];
			if (player.tower == _tower)
				player.SetState(Player.States.GameOver);
			else
				allPlayersFinished &= (player.state == Player.States.GameOver);
		}

		Environment.instance.GameOver(allPlayersFinished);
		if (allPlayersFinished)
		{
			UIMaster.instance.GameOver();
			SetGameState(GameStates.GameOver);
		}
	}

	/// <summary> Destroys all towers and resets game state </summary>
	public void QuitGame()
	{
		for (int i = 0; i < players.Length; ++i)
			players[i].Destroy();

		players = null;
		Environment.instance.ClearAllEffects();
		RecyclePool.ClearAllPools();

		SetGameState(GameStates.Menu);
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
