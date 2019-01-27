using UnityEngine;

[CreateAssetMenu(fileName = "GameTuning", menuName = "Bloculus/GameTuning", order = 1)]
public class GameTuning : ScriptableObject
{
	[Header("Game")]
	public int levelMax = 33;

	[Header("Tower")]
	public float towerRadius = 4f;
	public float rotateSpeed = 10f;
	public float minCameraDistance = -6f;
	public float selectorSwapAnimSpeed = 9f;

	[Header("Blocks")]
	public float fallSpeedSlowest = 2f;
	public float fallSpeedFastest = 7f;
	public float newBlockAppearRateSlowest = 3.5f;
	public float newBlockAppearRateFastest = 2f;
	public int columnsMin = 10;
	public int columnsMax = 30;
	public int rowsMin = 5;
	public int rowsMax = 16;
	public int blockTypesMin = 4;
	public float fallingRingScaleMult = 0.275f;
}
