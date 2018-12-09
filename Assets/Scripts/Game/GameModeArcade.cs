using UnityEngine;

public class GameModeArcade : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.Arcade; } }
	public override int NumPlayers { get { return 1; } }
	public override bool AlwaysStartOnLevel1 { get { return true; } }
	public override float LevelProgressRate { get { return 0.02333f; } }


/*
	public override void Init()
	{
	}

	public override void GameHasBegun()
	{
	}
*/
}
