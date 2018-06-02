using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;

public class BundleVersionBuildPostprocessor
{
	[PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		BundleVersionChecker.RefreshNow();
	}
}

/// <summary> Compares the version in the class to the bundle identifier in the Project Settings, and regenerates the class if necessary </summary>
[InitializeOnLoad]
public class BundleVersionChecker
{
	[MenuItem("Versioning/Refresh Build Version + Date")]
	public static void RefreshNow()
	{
		if (CreateNewBuildVersionClassFile(PlayerSettings.bundleVersion))
			Debug.Log("Refreshed BundleVersion (" + PlayerSettings.bundleVersion + ") and Date (" + DateTime.Now.ToString("yyyy-MM-dd") + ").");	// Would prefer to use CurrentBundleVersion values but Unity hasn't refreshed it yet
		else
			Debug.LogError("Error: BundleVersion not updated.");
	}

	/// <summary> Set to true to refresh whenever it gets a chance. BuildDate will always be correct, but the file will also be changed daily. </summary>
	static bool AlwaysRefresh = false;

	/// <summary> Auto-generated class name & file name </summary>
	const string ClassName = "CurrentBundleVersion";
	const string TargetCodeFile = "Assets/Scripts/" + ClassName + ".cs";

	/// <summary> Constructor </summary>
	static BundleVersionChecker()
	{
		string bundleVersion = PlayerSettings.bundleVersion;

		#if FINAL
			AlwaysRefresh = true;
		#endif

		string lastVersion = CurrentBundleVersion.Version;
		if (AlwaysRefresh || (lastVersion != bundleVersion))
		{
			Debug.Log("Found new bundle version " + bundleVersion + " (previous version was " + lastVersion +"). Regenerating file \"" + TargetCodeFile + "\"");
			CreateNewBuildVersionClassFile(bundleVersion);
		}
	}


	/// <summary> Generates a new class file with the speficied version string </summary>
	/// <param name="bundleVersion"> Bundle version </param>
	/// <returns> The new generated class's filename </returns>
	static bool CreateNewBuildVersionClassFile(string bundleVersion)
	{
		using (StreamWriter writer = new StreamWriter(TargetCodeFile, false))
		{
			try
			{
				string code = GenerateCode(bundleVersion);
				writer.WriteLine("{0}", code);
			}
			catch (System.Exception ex)
			{
				string msg = " threw:\n" + ex.ToString ();
				Debug.LogError(msg);
				EditorUtility.DisplayDialog("Error when trying to regenerate class", msg, "OK");
				return false;
			}
		}
		return true;
	}
	

	/// <summary> Regenerates (and replaces) the code for ClassName with new bundle version id </summary>
	/// <param name='bundleVersion'> New bundle version </param>
	/// <returns> Code to write to file </returns>
	static string GenerateCode(string bundleVersion)
	{
		string code = "// This file is autogenerated in the Unity Editor, by BundleVersionChecker\n";
		code += "public static class " + ClassName + "\n";
		code += "{\n";
		code += System.String.Format("\tpublic static readonly string Version = \"{0}\";\n", bundleVersion);
		code += System.String.Format("\tpublic static readonly string BuildDate = \"{0}\";\n", DateTime.Now.ToString("yyyy-MM-dd"));
		code += "}\n";
		return code;
	}
}
