using System;
using UnityEngine;

public class Player
{
	public Tower tower;
	public float ProgressThroughAllLevels { get; private set; }
	public int Score { get; private set; }
	public float Level { get; private set; }
	public int LevelInt { get; private set; }

	GameMaster.ControllerTypes controllerType;
	UI_PlayerHUD hud;
	string playerName;

	/// <summary> Constructor </summary>
	public Player(string _playerName, GameMaster.ControllerTypes _controllerType, Tower _towerPrefab, Vector3 _towerPosition, int _startingLevel)
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
		SetLevel(_startingLevel);
	}

	/// <summary> Restarts the current game </summary>
	public void ReplayGame()
	{
		SetScore(0);
		tower.ReplayGame(ProgressThroughAllLevels);
	}

	/// <summary> Destroy spawned elements </summary>
	public void Destroy()
	{
		GameObject.Destroy(tower.gameObject);
		GameObject.Destroy(hud.gameObject);
	}

	/// <summary> Called once per frame </summary>
	/// <param name='_dTime'> Time elapsed since last frame </param>
	/// <param name="_levelProgressRate"> How much to progress the level per second </param>
	public void UpdateGameplay(float _dTime, float _levelProgressRate)
	{
		// Update the level
		UpdateLevelProgress(_levelProgressRate * _dTime);

		// Update the tower
		int scoreChainFromTower;
		tower.UpdateTower(_dTime, out scoreChainFromTower);

		// Add score from tower, if any
		if (scoreChainFromTower != 0)
			AddScore(1 << scoreChainFromTower);
	}

	#region Score

	/// <summary> Sets the score directly and updates the UI </summary>
	/// <param name="_score"> New score </param>
	public void SetScore(int _score)
	{
		Score = _score;

		hud.RefreshScore(Score);
	}

	/// <summary> Adds to the score and refreshes the UI </summary>
	public void AddScore(int _score)
	{
		// Add bonus for higher levels, then add a 0 to the end
		_score += (int)(_score * ProgressThroughAllLevels);
		_score *= 10;
		Score += _score;

		hud.RefreshScore(Score);
	}

	#endregion // Score

	#region Level progress

	/// <summary> Sets the level directlyand updates the UI </summary>
	/// <param name="_level"> New level number </param>
	/// <param name="_resetTower"> True to clear & reset the tower, false to leave it as is </param>
	public void SetLevel(int _level, bool _resetTower = false)
	{
		Level = LevelInt = _level;

		ProgressThroughAllLevels = ((Level - 1f) / (GameMaster.instance.levelMax - 1f));
		if (ProgressThroughAllLevels > 1f)
			ProgressThroughAllLevels = 1f;

		tower.RestoreSpeeds(ProgressThroughAllLevels, _resetTower);

		UpdateLevelProgress(0f);
	}

	/// <summary> Updates the level and refreshes the UI </summary>
	public bool UpdateLevelProgress(float _levelProgress)
	{
		bool levelChanged = false;

		Level += _levelProgress;

		// Has level just changed?
		int newLevelInt = Mathf.FloorToInt(Level);
		if (newLevelInt != LevelInt)
		{
			SetLevel(newLevelInt);

			// If it's in gameplay, trigger the "level complete" sequence
			//LevelComplete();

			levelChanged = true;
		}

		hud.UpdateLevelProgress(Level - LevelInt);

		return levelChanged;
	}

	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='_playSound'> True to play the "level up" sound </param>
	void QuickLevelUp(bool _playSound = true)
	{
		if (_playSound)
			Environment.instance.PlayLevelUpAudio();

		Environment.instance.UpdateBackground(Level, true);
	}

	#endregion // Level progress
}
