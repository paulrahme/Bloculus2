using UnityEngine.UI;
using UnityEngine;

public class UI_PlayerHUD : MonoBehaviour
{
	#region Inspector variables

	[Header("Score")]
	[SerializeField] Text playerText = null;
	[SerializeField] Text levelText = null;
	[SerializeField] Text scoreText = null;

	[Header("Level Progress")]
	[SerializeField] Image progressBarFillImage = null;

	[Header("Game Over")]
	[SerializeField] GameObject gameOverHierarchy = null;

	#endregion // Inspector variables

	/// <summary> Initialises the UI </summary>
	/// <param name="_playerName"> Player string to display </param>
	public void Init(string _playerName)
	{
		playerText.text = _playerName;
		gameOverHierarchy.SetActive(false);
	}

	/// <summary> Updates the level display </summary>
	/// <param name="_level"> Player's level </param>
	public void SetLevel(int _level)
	{
		levelText.text = "Level " + _level;
	}

	/// <summary> Updates the score display </summary>
	/// <param name="_score"> New score </param>
	public void RefreshScore(int _score)
	{
		scoreText.text = string.Format("{0:N0}", _score);
	}

	/// <summary> Updates the level progress display </summary>
	/// <param name="_currentLevelsProgress"> Fractional progress through current level </param>
	public void UpdateLevelProgress(float _currentLevelsProgress)
	{
		progressBarFillImage.fillAmount = _currentLevelsProgress;
	}

	/// <summary> Called to refresh this player's GameOver state </summary>
	/// <param name="_isGameOver"> True if game over, false if in gameplay </param>
	public void SetGameOver(bool _isGameOver)
	{
		gameOverHierarchy.SetActive(_isGameOver);
	}
}
