using UnityEngine;

public class UI_HUD : MonoBehaviour
{
	#region Inspector variables

	[Header("Hierarchy")]
	[SerializeField] GameObject pauseButtonHierarchy = null;
	[SerializeField] Transform scoreContainer = null;

	[Header("Prefabs")]
	[SerializeField] UI_Score scorePrefab = null;

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
		while (scoreContainer.childCount > 0)
			DestroyImmediate(scoreContainer.GetChild(0).gameObject);
	}

	public UI_Score AddScore(string _playerName)
	{
		UI_Score score = Instantiate(scorePrefab, scoreContainer);
		score.Init(_playerName);
		return score;
	}
}
