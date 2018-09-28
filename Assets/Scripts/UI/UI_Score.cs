using UnityEngine.UI;
using UnityEngine;

public class UI_Score : MonoBehaviour
{
	#region Inspector variables

	[SerializeField] Text playerText = null;
	[SerializeField] Text scoreText = null;

	#endregion // Inspector variables

	int score;

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

	/// <summary> Initialises the UI </summary>
	/// <param name="_playerName"> Player string to display </param>
	public void Init(string _playerName)
	{
		playerText.text = _playerName;
		Score = 0;
	}
}
