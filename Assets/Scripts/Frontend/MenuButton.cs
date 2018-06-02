using UnityEngine;
using System.Collections;

public abstract class MenuButton : MonoBehaviour
{
	// Enums
	protected enum							eLockStates { Locked, Unlocked };
	
	public enum eMenuOptions { Start, ChangeLevel, ChangeBlockStyle, ToggleMusic, ToggleSound, ResumeGame, ReplayGame, ContinueGame, ExitGameWithSave, ExitGameNoSave, StartNextLevel, PopupKeyboard, SetHelpScreen, PauseGame, ShowHideSettingsScreen, SetSwipeDirection };
	public enum eEffectTypes { PulseOnly, PulseAndSpin };
	
	// Public variables
//	public eMenuOptions						gMenuOption;													// What this button does
//	public int								gMenuParam;														// Parameter for button's action
	public FrontendMenu						gParentMenu;													// Main frontend menu script
	public AudioClip						gAudioSelect;													// Audio for tapping/clicking this cube
	public GameObject						gPressEffect;													// Pulse to turn on when selected
	public TextMesh							gChildTextObject;												// Child TextMesh (for changing)
	public TextMesh							gDescriptionTextObject;											// 3DText description, if any
	public GameObject						gTextureObject;													// Child Texture (for toggling)
	public float							gPressedAnimRate = 3.0f;										// Pressed fade + anim rate (per second)
	public eEffectTypes						gPressedEffectType;												// Type of anim/effect to do when button is pressed
	public bool								gImmediateReaction = false;										// True to perform action immediately, false to perform after highlight animation
	public Color							gLockedColor;													// Colour for when this options is locked out
	public Texture							gTextureOn, gTextureOff;										// Toggle textures

	// Private variables
	private Color							gUnlockedColor;													// Normal colour for this option
	private Color							gHighlightColor = Color.white;									// Highlight's colour while fading
	private ColorCycle						gColorCycleScript;												// Attached ColorCycle script, if any

	// Helper/inline functions
	public void								MatchChildTextColor() { if (gChildTextObject != null) { gChildTextObject.color = GetComponent<Renderer>().material.color; } }
	public void								SetColor(Color newColor) { gUnlockedColor = GetComponent<Renderer>().material.color = newColor; MatchChildTextColor(); }

	/// <summary> Tasks to perform before select animation plays </summary>
	protected virtual void					PrepareForSelectAnim() {}

	/// <summary> Refreshes tint colour depending on current states/conditions </summary>
	public virtual void						UpdateColor() {}

	/// <summary> Should this button be locked out? </summary>
	/// <returns>  eLockStates.... state </returns>
	protected virtual eLockStates			GetLockState() { return eLockStates.Unlocked; }

	/// <summary> What to do when tapped/clicked </summary>
	public abstract void					PerformAction();
	
	
	/// <summary> Called when object/script activates </summary>
	protected virtual void Awake()
	{
		gUnlockedColor = GetComponent<Renderer>().material.color;
		gHighlightColor = ((gPressEffect != null) ? gPressEffect.GetComponent<Renderer>().material.color : Color.white);
		gColorCycleScript = GetComponent<ColorCycle>();		
	}
	

	/// <summary> Refreshes textures, highlights, locked/unlocked state, etc ready for the menu to appear </summary>
	public void SetInitialMenuState()
	{
		// Set colour, collider, enable/disable, etc
		eLockStates lockState = GetLockState();
		switch (lockState)
		{
			case eLockStates.Locked:
				GetComponent<Renderer>().material.color = gLockedColor;
				if (gChildTextObject != null) { gChildTextObject.color = gLockedColor; }
				if (gTextureOff != null) { gChildTextObject.GetComponent<Renderer>().material.mainTexture = gTextureOff; }
				enabled = GetComponent<Collider>().enabled = false;
				break;
				
			case eLockStates.Unlocked:
				GetComponent<Renderer>().material.color = gUnlockedColor;
				if (gChildTextObject != null) { gChildTextObject.color = gUnlockedColor; }
				if (gTextureOn != null) { gTextureObject.GetComponent<Renderer>().material.mainTexture = gTextureOn; }
				enabled = GetComponent<Collider>().enabled = true;
				break;
				
			default:
				throw new UnityException("Unhandled lock state "+GetLockState());
		}
	
		MatchChildTextColor();
		
		// Enable/disable colour pulsing
		if (gColorCycleScript != null)
		{
			gColorCycleScript.enabled = (lockState != eLockStates.Locked);
		}
		
		// Turn off highlight object
		gPressEffect.SetActive(false);
	}
	
	
	/// <summary> Called once per frame </summary>
	protected virtual void Update()
	{
		// Pressed fading anim playing?
		if (gPressEffect.activeSelf)
		{
			gHighlightColor.a -= gPressedAnimRate * Time.deltaTime;
			if (gHighlightColor.a <= 0.0f)
			{
				gHighlightColor.a = 0.0f;
				gPressEffect.SetActive(false);
				if (!gImmediateReaction)
				{
					PerformAction();
				}
			}
			else
			{
				gPressEffect.GetComponent<Renderer>().material.color = gHighlightColor;
			}
			
			// Sync spin to alpha (if necessary)
			if (gPressedEffectType == eEffectTypes.PulseAndSpin)
			{
				transform.localEulerAngles = new Vector3((360.0f * gHighlightColor.a), 0.0f, 0.0f);
			}
		}
		else if ((TowerCamera.gInstance.gGameObjectJustTapped == gameObject))
		{
			ButtonPressed();
		}
	}

	/// <summary> Called when the button was tapped </summary>
	public void ButtonPressed()
	{
		if (gImmediateReaction)
		{
			PerformAction();
		}

		// Play audio if there's any attached
		if ((gAudioSelect != null) && BackgroundMusic.gInstance.IsSoundEnabled()) { Tower.gInstance.GetComponent<AudioSource>().PlayOneShot(gAudioSelect); }

		// React to being pressed
		gPressEffect.SetActive(true);
		gHighlightColor.a = 1.0f;
		PrepareForSelectAnim();
	}

	/// <summary> Sets the on/off material + child plane's texture </summary>
	/// <param name='enabled'> True for "on", false for "off" </param>
	public void SetMaterialOnOff(bool enabled)
	{
		if (gTextureObject == null) { throw new UnityException("Cannot toggle material, texture object not set"); }

		if (enabled)
		{
			gTextureObject.GetComponent<Renderer>().material.mainTexture = gTextureOn;
			SetColor(Color.white);
		}
		else
		{
			gTextureObject.GetComponent<Renderer>().material.mainTexture = gTextureOff;
			SetColor(Color.grey);
		}
	}
}
