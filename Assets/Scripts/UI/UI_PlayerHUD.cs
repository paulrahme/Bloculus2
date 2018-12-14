using UnityEngine.UI;
using UnityEngine;

public class UI_PlayerHUD : MonoBehaviour
{
	#region Inspector variables

	[Header("Score")]
	[SerializeField] Text playerText = null;
	[SerializeField] Text scoreText = null;

	[Header("Level Progress")]
	[SerializeField] Image progressBarFillImage = null;

	#endregion // Inspector variables

	/// <summary> Initialises the UI </summary>
	/// <param name="_playerName"> Player string to display </param>
	/// <param name="_startingLevel"> Level to start on </param>
	public void Init(string _playerName)
	{
		playerText.text = _playerName;
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
}
