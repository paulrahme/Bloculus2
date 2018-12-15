using UnityEngine;

public class GameModePVPRace : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.PVPLocal_Race; } }
	public override int NumPlayers { get { return 2; } }
	public override float StartingLevelProgress { get { return 0f; } }
	public override float LevelProgressRate { get { return 0.02333f; } }
}
