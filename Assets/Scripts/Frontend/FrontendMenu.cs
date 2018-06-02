using UnityEngine;
using System.Collections;

public class FrontendMenu : MonoBehaviour
{
	// Enums and constants
	public enum						eFrontendStates { MenuAppearing, Menu, MenuDisappearing };
	public enum						eFrontendScreens { MainMenu, HowToPlay, Settings, ReleaseNotes };
	
	// Public variables
	public eFrontendStates			gState { get; private set; }											// Current action
	public BackgroundMusic			gBGMScript;																// Background Music script
	public GameObject				gMainMenuHierarchy;														// Hierarchy of button, label, etc
	public GameObject				gHelpScreenHierarchy;													// Hierarchy of all widgets on the help screen
	public GameObject[]				gHelpScreens;															// Help Screen pages
	public GameObject				gSettingsScreenHierarchy;												// Hierarchy of all widgets on the settings screen
	public GameObject				gReleaseNotesScreenHierarchy;											// Hierarchy of all widgets on the "what's new" screen
	public TextMesh					gVersionNoText;															// Version text label

	// Private variables
	public float 					gFullScale;																// Scale when zoomed out to full size


	/// <summary> Sets the state & performs necessary actions </summary>
	/// <param name='newState'> FrontendState.... state name </param>
	public void SetState(eFrontendStates newState)
	{
		switch (newState)
		{
			case eFrontendStates.MenuAppearing:
				GetComponent<AudioSource>().Play();
				gBGMScript.StartTitleMusic();
				RefreshLockedOrUnlockedCubes();
				UpdateColors();
				break;
			
			case eFrontendStates.MenuDisappearing:
				GetComponent<AudioSource>().Play();
				break;
			
			default:
				break;
		}
		gState = newState;
	}
	

	/// <summary> Called when object/script initiates </summary>
	void Awake()
	{
		gFullScale = transform.localScale.x;
		gVersionNoText.text = "v"+CurrentBundleVersion.Version;
		SetHelpScreenIndex(-1);
		gMainMenuHierarchy.SetActive(true);
		gHelpScreenHierarchy.SetActive(false);
		gSettingsScreenHierarchy.SetActive(false);
	}
	
	/// <summary> Updates all cubes' locked/unlocked states </summary>
	public void RefreshLockedOrUnlockedCubes()
	{
		foreach (MenuButton menuButtonScript in transform.GetComponentsInChildren<MenuButton>())
		{
			menuButtonScript.SetInitialMenuState();
		}
	}
	
	
	/// <summary> Updates selected / deselected colours </summary>
	public void UpdateColors()
	{
		foreach (MenuButton menuButtonScript in transform.GetComponentsInChildren<MenuButton>())
		{
			if (menuButtonScript.enabled)
			{
				menuButtonScript.UpdateColor();
			}
		}
	}

	
	// Resets menu's position/size
	void Reset()
	{
		transform.localScale = new Vector3(gFullScale, gFullScale, gFullScale);
		gameObject.SetActive(true);
	}


	// <summary> Called once per frame </summary>
	void Update()
	{
		switch (gState)
		{
			case eFrontendStates.MenuAppearing:
				transform.localScale *= 1.4f;
				if (transform.localScale.x >= gFullScale)
				{
					transform.localScale = new Vector3(gFullScale, gFullScale, gFullScale);
					SetState(eFrontendStates.Menu);
				}
				break;
				
			case eFrontendStates.Menu:
				break;
			
			case eFrontendStates.MenuDisappearing:
				transform.localScale *= 0.8f;
				if (transform.localScale.x < 0.01f)
				{
					gameObject.SetActive(false);
					Tower.gInstance.GameHasBegun();
				}
				break;

			default:
				throw new UnityException("Unhandled FrontendState '"+gState+"'");
		}
	}


	/// <summary> Changes to the specified frontend screen </summary>
	/// <param name="screen"> eFrontendScreens.... screen name </param>
	public void SetFrontendScreen(eFrontendScreens screen)
	{
		// Enable required screen, disable others
		gMainMenuHierarchy.SetActive(screen == eFrontendScreens.MainMenu);
		gHelpScreenHierarchy.SetActive(screen == eFrontendScreens.HowToPlay);
		gSettingsScreenHierarchy.SetActive(screen == eFrontendScreens.Settings);
		gReleaseNotesScreenHierarchy.SetActive(screen == eFrontendScreens.ReleaseNotes);
	}


	/// <summary> Enables the specified help page's hierarchy, and disables the other pages </summary>
	/// <param name="pageToShow"> Index of help page to show </param>
	public void SetHelpScreenIndex(int pageToShow)
	{
		for (int i = 0; i < gHelpScreens.Length; ++i)
		{
			gHelpScreens[i].SetActive(i == pageToShow);
		}
	}
}
