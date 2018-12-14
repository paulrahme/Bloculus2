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

	/// <summary> Adds a new HUD for this player, parented under the container </summary>
	/// <param name="_playerName"> Player's name to display </param>
	/// <returns> The UI_PlayerHUD created </returns>
	public UI_PlayerHUD AddPlayerHUD(string _playerName)
	{
		UI_PlayerHUD hud = Instantiate(playerHUDPrefab, playerHUDContainer);
		hud.Init(_playerName);
		return hud;
	}
}
