using UnityEngine;

public class GameModeOriginal : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.Original; } }
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
