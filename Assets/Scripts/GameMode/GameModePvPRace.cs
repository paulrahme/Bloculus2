using UnityEngine;

public class GameModePVPRace : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.PVPLocal_Race; } }
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
