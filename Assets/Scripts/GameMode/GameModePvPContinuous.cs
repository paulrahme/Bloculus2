﻿using UnityEngine;

public class GameModePVPContinuous : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.PVPLocal_Continuous; } }
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
