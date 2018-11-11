using System;
using UnityEngine;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
	public enum PopupTypes { Default }
	enum States { Idle, PoppingUp, PopupShown, Dismissing }

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

	[Header("Popup Animation")]
	[SerializeField] AnimationCurve showAnimCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1.1f), new Keyframe(0.8f, 0.95f), new Keyframe(1f, 1f));
	[SerializeField] float showAnimDuration = 0.35f;
	[SerializeField] AnimationCurve hideAnimCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.2f, 1.2f), new Keyframe(1f, 0f));
	[SerializeField] float hideAnimDuration = 0.25f;

	#endregion // Inspector variables

	States state = States.Idle;
	Dictionary<PopupTypes, UI_Popup> popups = new Dictionary<PopupTypes, UI_Popup>();
	UI_Popup currentPopup;
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
				currentPopup.UpdateAnim(showAnimCurve.Evaluate(animProgress));
				break;

			case States.PopupShown:
				break;

			case States.Dismissing:
				animProgress += animSpeed * Time.deltaTime;
				if (animProgress < 1f)
					currentPopup.UpdateAnim(hideAnimCurve.Evaluate(animProgress));
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

	void ShowNextPopup()
	{
		PopupInfo popupInfo = queuedPopups.Dequeue();
		currentPopup = popups[popupInfo._popupType];
		currentPopup.StartShowing(popupInfo);
		gameObject.SetActive(true);

		// Set state
		animSpeed = 1f / showAnimDuration;
		animProgress = 0f;
		state = States.PoppingUp;
	}

	/// <summary> Called from the child popup being dismissed </summary>
	public void PopupDismissed(Action _dismissCallback)
	{
		dismissCallback = _dismissCallback;

		// Set state
		animSpeed = 1f / hideAnimDuration;
		animProgress = 0f;
		state = States.Dismissing;
	}

	#endregion // Public interface
}
