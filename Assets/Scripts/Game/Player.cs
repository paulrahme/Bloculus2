using UnityEngine;

public class Player
{
	#region Variables

	string playerName;
	GameMaster.ControllerTypes controllerType;
	Tower tower;
	UI_PlayerHUD hud;
	int score;
	float startingLevelProgress;
	int level;
	int levelMax;
	float levelProgress;

	#endregion Variables

	/// <summary> Constructor </summary>
	public Player(string _playerName, GameMaster.ControllerTypes _controllerType, Tower _towerPrefab, Vector3 _towerPosition, int _startingLevel, float _startingLevelProgress)
	{
		playerName = _playerName;
		controllerType = _controllerType;

		// Create tower
		tower = GameObject.Instantiate(_towerPrefab);
		tower.basePosition = _towerPosition;

		switch (controllerType)
		{
			case GameMaster.ControllerTypes.KeyboardWASD:
				{
					TowerControlKeyboard controller = tower.gameObject.AddComponent<TowerControlKeyboard>();
					tower.controller = controller;
					controller.Init(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.Space);
				}
				break;

			case GameMaster.ControllerTypes.KeyboardArrows:
				{
					TowerControlKeyboard controller = tower.gameObject.AddComponent<TowerControlKeyboard>();
					tower.controller = controller;
					controller.Init(KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Return);
				}
				break;

			case GameMaster.ControllerTypes.Gamepad:
				tower.controller = tower.gameObject.AddComponent<TowerControlGamepad>();
				break;

			default:
				throw new UnityException("Unhandled Controller Type " + controllerType);
		}

		// Create HUD
		hud = UIMaster.instance.hud.AddPlayerHUD(playerName);
		SetScore(0);
		startingLevelProgress = _startingLevelProgress;
		levelMax = GameMaster.instance.levelMax;
		SetLevel(_startingLevel, true);
	}

	/// <summary> Restarts the current game </summary>
	public void ReplayGame()
	{
		SetScore(0);
		tower.ReplayGame(level);
	}

	/// <summary> Destroy spawned elements </summary>
	public void Destroy()
	{
		GameObject.Destroy(tower.gameObject);
		GameObject.Destroy(hud.gameObject);
	}

	/// <summary> Called once per frame </summary>
	/// <param name='_dTime'> Time elapsed since last frame </param>
	/// <param name="_gameMode"> Current GameMode being played </param>
	public void UpdateGameplay(float _dTime, GameMode _gameMode)
	{
		float levelProgressBonus = 0f;

		// Update the tower
		int scoreChainFromTower;
		tower.UpdateTower(_dTime, out scoreChainFromTower);

		// Add score from tower, if any
		if (scoreChainFromTower != 0)
		{
			int chainExp = 1 << scoreChainFromTower;
			AddScore(chainExp);
			levelProgressBonus = _gameMode.ComboProgressBoost * chainExp;
		}

		// Update the level
		if (UpdateLevelProgress((_gameMode.LevelProgressRate * _dTime) + levelProgressBonus))
			_gameMode.PlayerLevelledUp(level);
	}

	#region Score

	/// <summary> Sets the score directly and updates the UI </summary>
	/// <param name="_score"> New score </param>
	public void SetScore(int _score)
	{
		score = _score;

		hud.RefreshScore(score);
	}

	/// <summary> Adds to the score and refreshes the UI </summary>
	public void AddScore(int _score)
	{
		// Add bonus for higher levels, then add a 0 to the end
		_score += _score * level / levelMax;
		_score *= 10;
		score += _score;

		hud.RefreshScore(score);
	}

	#endregion // Score

	#region Level progress

	/// <summary> Sets the level directlyand updates the UI </summary>
	/// <param name="_level"> New level number </param>
	/// <param name="_resetTower"> True to clear & reset the tower, false to leave it as is </param>
	public void SetLevel(int _level, bool _resetTower = false)
	{
		level = _level;
		levelProgress = startingLevelProgress;

		tower.RestoreSpeeds(level, _resetTower);

		hud.SetLevel(level);
		UpdateLevelProgress(0f);
	}

	/// <summary> Updates the level and refreshes the UI </summary>
	public bool UpdateLevelProgress(float _levelProgress)
	{
		bool levelChanged = false;

		// Update level progress
		levelProgress += _levelProgress;
		if (levelProgress < 0f)
			levelProgress = 0f;		
		else if (levelProgress > 1f)
		{
			SetLevel(level + 1);

			// If it's in gameplay, trigger the "level complete" sequence
			//LevelComplete();

			levelChanged = true;
		}

		hud.UpdateLevelProgress(levelProgress);

		return levelChanged;
	}

	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='_playSound'> True to play the "level up" sound </param>
	void QuickLevelUp(bool _playSound = true)
	{
		if (_playSound)
			Environment.instance.PlayLevelUpAudio();

		Environment.instance.UpdateBackground(level);
	}

	#endregion // Level progress
}
