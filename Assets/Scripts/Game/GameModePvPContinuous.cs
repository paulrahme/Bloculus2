using UnityEngine;

public class GameModePVPContinuous : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.PVPLocal_Continuous; } }
	public override int NumPlayers { get { return 2; } }
}
