﻿using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
	public enum GameModeTypes { Original, Arcade, TimeChallenge, SpeedChallenge, ScoreChallenge, PVPLocal_Continuous, PVPLocal_Race };
	public abstract GameModeTypes GameModeType { get; }
	public abstract int NumTowers { get; }
	public virtual bool AlwaysStartOnLevel1 { get { return false; } }
	public virtual bool HasRingBar { get { return true; } }	// TODO: false for Score Challenge
	public virtual float LevelIncreaseRate { get { return 0.00666f; } }	// TODO: 0.01666f for Score Challenge, 0.02333f for Arcade

	public virtual void GameHasBegun()
	{
		// gameStartTime = Time.time;	// TODO: for timed game modes, put back in GameMaster?
	}
}
