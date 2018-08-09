using UnityEngine;

public class GameModeArcade : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.Arcade; } }
	public override int NumTowers { get { return 1; } }
/*
	public override void Init()
	{
	}

	public override void GameHasBegun()
	{
	}
*/
}
