using UnityEngine;

public class GameModeArcade : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.Arcade; } }
	public override int NumPlayers { get { return 1; } }
	public override float StartingLevelProgress { get { return 0f; } }
	public override float LevelProgressRate { get { return 0.02333f; } }

	/// <summary> Which level this GameMode starts on </summary>
	public override int GetStartingLevel()
	{
		return 1;
	}
}
