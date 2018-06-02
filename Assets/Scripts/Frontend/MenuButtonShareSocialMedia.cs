using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuButtonShareSocialMedia : MenuButton
{
	// Public variables
	public string				giOSURL = "http://";					// URL for sharing high scores etc
	public string				gAndroidURL = "http://";				// URL for sharing high scores etc


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

		Application.OpenURL(urlToUse.Replace("XXXX", Tower.gInstance.gHighScore.ToString()).Replace("YYYY", Tower.gInstance.gLevelInt.ToString()).Replace("ZZZZ", Tower.gInstance.gGameMode.ToString()));
	}
}
