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

	internal float ProgressThroughAllLevels { get; private set; }
	internal int Score { get; private set; }
	internal float Level { get; private set; }
	internal int LevelInt { get; private set; }

	/// <summary> Initialises the UI </summary>
	/// <param name="_playerName"> Player string to display </param>
	/// <param name="_startingLevel"> Level to start on </param>
	public void Init(string _playerName, int _startingLevel)
	{
		playerText.text = _playerName;
		SetScore(0);
		SetLevel(_startingLevel);
	}

	/// <summary> Sets the score directly and updates the UI </summary>
	/// <param name="_score"> New score </param>
	public void SetScore(int _score)
	{
		Score = 0;
		AddScore(0);
	}

	/// <summary> Adds to the score and refreshes the UI </summary>
	public void AddScore(int _score)
	{
		// Add bonus for higher levels, then add a 0 to the end
		_score += (int)(_score * ProgressThroughAllLevels);
		_score *= 10;
		Score += _score;

		scoreText.text = string.Format("{0:N0}", Score);
	}

	/// <summary> Sets the level directlyand updates the UI </summary>
	/// <param name="_level"> New level number </param>
	public void SetLevel(int _level)
	{
		Level = LevelInt = _level;

		ProgressThroughAllLevels = ((Level - 1f) / (GameMaster.instance.levelMax - 1f));
		if (ProgressThroughAllLevels > 1f)
			ProgressThroughAllLevels = 1f;

		UpdateLevelProgress(0f);
	}

	/// <summary> Updates the level and refreshes the UI </summary>
	public bool UpdateLevelProgress(float _levelProgress)
	{
		bool levelChanged = false;

		Level += _levelProgress;

		// Has level just changed?
		int newLevelInt = Mathf.FloorToInt(Level);
		if (newLevelInt != LevelInt)
		{
			SetLevel(newLevelInt);

			// If it's in gameplay, trigger the "level complete" sequence
			//LevelComplete();

			levelChanged = true;
		}

		progressBarFillImage.fillAmount = Level - LevelInt;   // Progress bar shows fractional part

		return levelChanged;
	}

	/// <summary> Triggers the "Level up" fading text </summary>
	/// <param name='_playSound'> True to play the "level up" sound </param>
	void QuickLevelUp(bool _playSound = true)
	{
		if (_playSound)
			Environment.instance.PlayLevelUpAudio();

		Environment.instance.UpdateBackground(Level, true);
	}
}
