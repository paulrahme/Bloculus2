using System;
using UnityEngine;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
	public enum PopupTypes { Default, YesNo }
	enum States { Idle, PoppingUp, PopupShown, Dismissing }

	public class PopupInfo
	{
		public PopupTypes _popupType = PopupTypes.Default;
		public string title = String.Empty;
		public string messageBody = String.Empty;
		public string confirmText = "OK";
		public string cancelText = "Cancel";
		public Action confirmCallback = null;
		public Action cancelCallback = null;
	}

	States state = States.Idle;
	Dictionary<PopupTypes, UI_Popup> popups = new Dictionary<PopupTypes, UI_Popup>();
	UI_Popup currentPopup;
	PopupInfo currentPopupInfo;
	float animSpeed, animProgress;
	Action dismissCallback;
	Queue<PopupInfo> queuedPopups = new Queue<PopupInfo>();

	/// <summary> Called once per frame </summary>
	void Update()
	{
		switch (state)
		{
			case States.Idle:
				gameObject.SetActive(false);
				break;

			case States.PoppingUp:
				animProgress += animSpeed * Time.deltaTime;
				if (animProgress > 1f)
				{
					animProgress = 1f;
					state = States.PopupShown;
				}
				currentPopup.UpdateShowAnim(animProgress);
				break;

			case States.PopupShown:
				break;

			case States.Dismissing:
				animProgress += animSpeed * Time.deltaTime;
				if (animProgress < 1f)
					currentPopup.UpdateHideAnim(animProgress);
				else
				{
					if (dismissCallback != null)
						dismissCallback();

					currentPopup.gameObject.SetActive(false);
					currentPopup = null;
					if (queuedPopups.Count > 0)
						ShowNextPopup();
					else
						state = States.Idle;
				}
				break;

			default:
				throw new UnityException("Unhandled state " + state);
		}
	}

	/// <summary> Prepares dictionary of child popups </summary>
	void InitChildPopups()
	{
		UI_Popup[] childPopups = GetComponentsInChildren<UI_Popup>(true);
		for (int i = 0; i < childPopups.Length; ++i)
		{
			UI_Popup popup = childPopups[i];
			popup.parentManager = this;
			popups.Add(popup.popupType, popup);
			popup.gameObject.SetActive(false);
		}
	}

	#region Public interface

	/// <summary> Populates & shows the specified popup </summary>
	/// <param name="_popupInfo"> Info of popup to show and its contents </param>
	public void ShowOrEnqueue(PopupInfo _popupInfo)
	{
		if (popups.Keys.Count == 0)
			InitChildPopups();

		queuedPopups.Enqueue(_popupInfo);

		if (state == States.Idle)
			ShowNextPopup();
	}

	/// <summary> Populates the next popup and starts it animating in </summary>
	void ShowNextPopup()
	{
		currentPopupInfo = queuedPopups.Dequeue();
		currentPopup = popups[currentPopupInfo._popupType];
		currentPopup.StartShowing(currentPopupInfo);
		gameObject.SetActive(true);

		// Set state
		animSpeed = currentPopup.ShowAnimSpeed;
		animProgress = 0f;
		state = States.PoppingUp;
	}

	/// <summary> Called from the child popup being dismissed </summary>
	public void PopupDismissed(Action _dismissCallback)
	{
		dismissCallback = _dismissCallback;

		// Set state
		animSpeed = currentPopup.HideAnimSpeed;
		animProgress = 0f;
		state = States.Dismissing;
	}

	public void PopupConfirmed() { PopupDismissed(currentPopupInfo.confirmCallback); }
	public void PopupCancelled() { PopupDismissed(currentPopupInfo.cancelCallback); }

	#endregion // Public interface
}
