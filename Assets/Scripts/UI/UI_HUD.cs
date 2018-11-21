using UnityEngine;

public class UI_HUD : MonoBehaviour
{
	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] GameObject pauseButtonHierarchy = null;
	[SerializeField] Transform playerHUDContainer = null;

	[Header("Prefabs")]
	[SerializeField] UI_PlayerHUD playerHUDPrefab = null;

	#endregion // Inspector variables

	/// <summary> Shows/hides anything that should only be visible during gameplay </summary>
	/// <param name="_visible"> True to show, false to hide </param>
	public void SetGameplayElementsVisible(bool _visible)
	{
		pauseButtonHierarchy.SetActive(_visible);
	}

	/// <summary> Called from button's OnClick event </summary>
	public void PauseGame()
	{
		UIMaster.instance.Pause();
		GameMaster.instance.Pause();
	}

	public void ClearScores()
	{
		while (playerHUDContainer.childCount > 0)
			DestroyImmediate(playerHUDContainer.GetChild(0).gameObject);
	}

	public UI_PlayerHUD AddPlayerHUD(string _playerName, int _startingLevel)
	{
		UI_PlayerHUD score = Instantiate(playerHUDPrefab, playerHUDContainer);
		score.Init(_playerName, _startingLevel);
		return score;
	}
}
