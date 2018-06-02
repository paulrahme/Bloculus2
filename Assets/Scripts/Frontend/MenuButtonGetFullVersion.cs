using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonGetFullVersion : MenuButton
{
	// Public variables
	public string				giOSURL;							// Address to open on click
	public string				gAndroidURL;						// Address to open on click


	/// <summary> Called when object/script activates </summary>
	protected override void Awake()
	{
		base.Awake();
#if FREE_VERSION && !UNITY_WEBPLAYER
		gameObject.SetActive(true);
#else
		gameObject.SetActive(false);
#endif
	}


	/// <summary> What to do when tapped/clicked </summary>
	public override void PerformAction()
	{
		string urlToUse;
#if UNITY_IOS
		urlToUse = giOSURL;
#elif UNITY_ANDROID
		urlToUse = gAndroidURL;
#else
		urlToUse = string.Empty;
#endif

		Application.OpenURL(urlToUse);
	}
}
