using UnityEngine;

public class UIMaster : MonoBehaviour
{
	#region Inspector variables
	public UI_MainMenu mainMenu;
	#endregion	// Inspector variables

	/// <summary> Singleton instance </summary>
	public static UIMaster instance;

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		if (instance != null)
			throw new UnityException("Singleton instance already exists");
		instance = this;
	}

	/// <summary> Called before first Update() </summary>
	void Start()
	{
		mainMenu.gameObject.SetActive(true);
	}
}
