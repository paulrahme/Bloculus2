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

	[MenuItem("Bloculus/Scenes/Load 'UI' Scene")]
	public static void LoadSceneUI()
	{
		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			EditorSceneManager.OpenScene("Assets/Scenes/UI.unity");
	}

	[MenuItem("Bloculus/Scenes/Load 'Game' Scene")]
	public static void LoadSceneGame()
	{
		if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
	}
}
