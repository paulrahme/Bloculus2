using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonStartOrResumeGame : MenuButton
{
	// Enums
	public enum				eButtonActions { Start, Resume };

	// Public variables
	public eButtonActions	gButtonAction;							// This button's function
	public GameObject		gFullVersionOnly3DText;					// Overlay displayed when this game mode is not available


	protected override void Awake()
	{
		base.Awake();
#if FREE_VERSION
		if (gButtonAction == eButtonActions.Resume)
		{
			gameObject.SetActive(false);
		}
#endif
	}
	
	/// <summary> Should this button be locked out? </summary>
	/// <returns> eLockStates.... state </returns>
	protected override eLockStates GetLockState()
	{
		switch (gButtonAction)
		{
			case eButtonActions.Start:
#if FREE_VERSION
				eLockStates retState = (Tower.gInstance.gGameMode == Tower.eGameModes.Arcade) ? eLockStates.Unlocked : eLockStates.Locked;
				gFullVersionOnly3DText.SetActive(retState == eLockStates.Locked);
				return retState;
#else
				gFullVersionOnly3DText.SetActive(false);
				return eLockStates.Unlocked;
#endif

			case eButtonActions.Resume:
				return ((Tower.gInstance.gGameMode == Tower.eGameModes.Original) && (PlayerPrefs.GetInt(Constants.kQSExists) == 1)) ? eLockStates.Unlocked : eLockStates.Locked;

			default:
				throw new UnityException("Unhandled button action "+gButtonAction);
		}
	}


	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		switch (gButtonAction)
		{
			case eButtonActions.Start:
				TowerCamera.gInstance.SetState(TowerCamera.eStates.Gameplay);
				gParentMenu.SetState(FrontendMenu.eFrontendStates.MenuDisappearing);
				PlayerPrefs.Save();
				break;

			case eButtonActions.Resume:
				TowerCamera.gInstance.SetState(TowerCamera.eStates.Gameplay);
				gParentMenu.SetState(FrontendMenu.eFrontendStates.MenuDisappearing);
				break;
				
			default:
				throw new UnityException("Unhandled button action "+gButtonAction);
		}
	}


	/// <summary> Tasks to perform before select animation plays </summary>
	protected override void PrepareForSelectAnim()
	{
		switch (gButtonAction)
		{
			case eButtonActions.Start:
				gParentMenu.gBGMScript.GetComponent<AudioSource>().Stop();
				DisableFrontendColliders();
				break;

			case eButtonActions.Resume:
				Tower.gInstance.QuickLoad();
				gParentMenu.gBGMScript.GetComponent<AudioSource>().Stop();
				DisableFrontendColliders();
				break;

			default:
				throw new UnityException("Unhandled button action "+gButtonAction);
		}
	}
	
	
	/// <summary> Turns off colliders (and interaction) with all frontend objects </summary>
	private void DisableFrontendColliders()
	{
		// Disable any more interaction with frontend objects
		foreach (GameObject gameObj in GameObject.FindGameObjectsWithTag("FrontendButton"))
		{
			gameObj.GetComponent<Collider>().enabled = false;
		}
	}
}
