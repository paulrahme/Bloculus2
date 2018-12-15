using UnityEngine;

public class GameModeOriginal : GameMode
{
	public override GameModeTypes GameModeType { get { return GameModeTypes.Original; } }
	public override int NumPlayers { get { return 1; } }
}
