using System;
using UnityEngine;
using System.Collections.Generic;

public class UI_PopupManager : MonoBehaviour
{
	public enum PopupTypes { Default };

	public class PopupInfo
	{
		public PopupTypes _popupType = PopupTypes.Default;
		public string title = String.Empty;
		public string messageBody = String.Empty;
		public string confirmText = "OK";
		public string cancelText = "Cancel";
		public Action confirmCallback = null;
		public Action cancelCalback = null;
	}

	#region Inspector variables

//	[Header("Hierarchy")]
//	[SerializeField] UI_Popup popupDefault = null;

	#endregion // Inspector variables

	Dictionary<PopupTypes, UI_Popup> popups = new Dictionary<PopupTypes, UI_Popup>();

	/// <summary> Called when object/script activates </summary>
	void Awake()
	{
		UI_Popup[] childPopups = GetComponentsInChildren<UI_Popup>(true);
		for (int i = 0; i < childPopups.Length; ++i)
		{
			UI_Popup popup = childPopups[i];
			popup.parentManager = this;
			popups.Add(popup.popupType, popup);
			popup.Hide();
		}
	}

	/// <summary> Populates & shows the specified popup </summary>
	/// <param name="_popupInfo"> Info of popup to show and its contents </param>
	public void Show(PopupInfo _popupInfo)
	{
		UI_Popup popup = popups[_popupInfo._popupType];
		popup.Show(_popupInfo);
		gameObject.SetActive(true);
	}

	/// <summary> Called from the child popup being dismissed </summary>
	public void PopupDismissed()
	{
		gameObject.SetActive(false);
	}
}
