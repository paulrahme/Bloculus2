using UnityEngine;

public class Player
{
	public GameMaster.ControllerTypes controllerType;
	public Tower tower;
	public UI_PlayerHUD hud;

	public Player(GameMaster.ControllerTypes _controllerType, Tower _tower, UI_PlayerHUD _hud)
	{
		controllerType = _controllerType;
		tower = _tower;
		hud = _hud;
	}

	public void SetLevel(int _level, bool _resetTower = false)
	{
		hud.SetLevel(_level);
		tower.RestoreSpeeds(hud.ProgressThroughAllLevels, _resetTower);
	}

	public void ReplayGame()
	{
		hud.SetScore(0);
		tower.ReplayGame(hud.ProgressThroughAllLevels);
	}

	public void OnDestroy()
	{
		GameObject.Destroy(tower.gameObject);
		GameObject.Destroy(hud.gameObject);
	}
}
