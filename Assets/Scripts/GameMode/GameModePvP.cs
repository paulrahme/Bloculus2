using UnityEngine;

public class GameModePVP : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.PVPLocal; } }
	public override int NumTowers { get { return 2; } }
/*
	public override void Init()
	{
	}

	public override void GameHasBegun()
	{
	}
*/
}
