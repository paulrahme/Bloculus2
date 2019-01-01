using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
	public enum GameModeTypes { Original, Arcade, TimeChallenge, SpeedChallenge, ScoreChallenge, PVPLocal_Continuous, PVPLocal_Race };
	public abstract GameModeTypes GameModeType { get; }
	public abstract int NumPlayers { get; }
	public virtual bool HasRingBar { get { return true; } }	// TODO: false for Score Challenge
	public virtual float StartingLevelProgress { get { return 0.5f; } }
	public virtual float LevelProgressRate { get { return -0.002f; } }
	public virtual float ComboProgressBoost { get { return 0.003f; } }

	public virtual void GameHasBegun()
	{
		// gameStartTime = Time.time;	// TODO: for timed game modes, put back in GameMaster?
	}

	/// <summary> Which level this GameMode starts on </summary>
	public virtual int GetStartingLevel()
	{
		return PlayerPrefs.GetInt(GameModeType.ToString() + Constants.PPKeys.StartingLevel, 1);
	}

	public void PlayerLevelledUp(int _level)
	{
		GameMaster.instance.RefreshEnvironment(_level);
	}
}
