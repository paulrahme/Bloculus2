using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CustomMenuItems : MonoBehaviour
{
	[MenuItem("Bloculus/Clear PlayerPrefs")]
	private static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	[MenuItem("Bloculus/Reload Current Scene")]
	public static void ReloadCurrentScene()
	{
		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
	}
}
