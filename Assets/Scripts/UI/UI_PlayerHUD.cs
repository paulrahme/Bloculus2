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

	int score;
	float level;

	/// <summary> Wrapper for keeping the score and UI text label in sync </summary>
	public int Score
	{
		get { return score; }
		set
		{
			score = value;
			scoreText.text = string.Format("{0:N0}", score);
		}
	}

	/// <summary> Wrapper for keeping the score and UI text label in sync </summary>
	public float Level
	{
		get { return level; }
		set
		{
			level = value;
			progressBarFillImage.fillAmount = level - (int)level;	// Progress bar shows fractional part
		}
	}

	/// <summary> Initialises the UI </summary>
	/// <param name="_playerName"> Player string to display </param>
	/// <param name="_startingLevel"> Level to start on </param>
	public void Init(string _playerName, int _startingLevel)
	{
		playerText.text = _playerName;
		Score = 0;
		Level = _startingLevel;
	}
}
