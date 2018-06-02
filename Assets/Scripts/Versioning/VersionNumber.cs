using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class VersionNumber : MonoBehaviour
{
	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		Text text = GetComponent<Text>();
		if (text == null)
			throw new UnityException("Could not find Text component attached to '" + gameObject.name + "'");

		// If you receieve error: "The name `CurrentBundleVersion' does not exist in the current context"
		// Then: Import DefaultAutogenScripts.package, located in External/PlaySide/ dir
		text.text = "v" + CurrentBundleVersion.Version + " (" + CurrentBundleVersion.BuildDate + ")";
	}
}
