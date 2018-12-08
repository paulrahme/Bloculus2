using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SuperButton))]
public class SuperButton_Editor : Editor
{
	GUIStyle labelStyle = new GUIStyle();
	GUIContent interactableLabel = new GUIContent("Interactable");
	GUIContent transformLabel = new GUIContent("Transform To Animate");
	GUIContent animTypeLabel = new GUIContent("Animation Type");
	GUIContent animCurveLabel = new GUIContent("Anim Curve");
	GUIContent animDurationLabel = new GUIContent("Anim Duration");
	GUIContent moveDistanceLabel = new GUIContent("Move Distance");
	GUIContent highlightCanvasGroupLabel = new GUIContent("Highlight Canvas Group");
	GUIContent disableTypeLabel = new GUIContent("Disable Type");
	GUIContent disableCanvasGroupLabel = new GUIContent("Canvas Group To Alpha Out");
	GUIContent disableAlphaLabel = new GUIContent("Alpha Amount");
	GUIContent audioLabel = new GUIContent("Audio");
	GUIContent audioOnAnimStartLabel = new GUIContent("Audio On Anim Start");
	GUIContent lockOthersDuringAnimLabel = new GUIContent("Lock Others During Anim");
	GUIContent lockCooldownAfterAnimLabel = new GUIContent("Lock Cooldown After Anim");
	SuperButton button;

	public override void OnInspectorGUI()
	{
		if (button == null)
		{
			button = (SuperButton)target;
			labelStyle.fontStyle = FontStyle.Bold;
			labelStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
			button.transition = Selectable.Transition.None;
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Button", labelStyle);

		bool interactable;
		EditorGUI.BeginChangeCheck();
		{
			interactable = EditorGUILayout.Toggle(interactableLabel, button.interactable);
		}
		if (EditorGUI.EndChangeCheck())	// Avoids it refreshing on every frame even when it hasn't changed
			button.interactable = interactable;			

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Animation", labelStyle);
		button.transformToAnimate = (RectTransform)EditorGUILayout.ObjectField(transformLabel, button.transformToAnimate, typeof(RectTransform), true);
		button.animationType = (SuperButton.AnimationTypes)EditorGUILayout.EnumPopup(animTypeLabel, button.animationType);
		switch (button.animationType)
		{
			case SuperButton.AnimationTypes.Scale:
				button.animCurveScale = EditorGUILayout.CurveField(animCurveLabel, button.animCurveScale);
				break;

			case SuperButton.AnimationTypes.MoveUpDown:
			case SuperButton.AnimationTypes.MoveLeftRight:
				button.animCurveMove = EditorGUILayout.CurveField(animCurveLabel, button.animCurveMove);
				break;
		}
		button.animDuration = EditorGUILayout.FloatField(animDurationLabel, button.animDuration);
		if ((button.animationType == SuperButton.AnimationTypes.MoveLeftRight) || (button.animationType == SuperButton.AnimationTypes.MoveUpDown))
			button.moveDistance = EditorGUILayout.FloatField(moveDistanceLabel, button.moveDistance);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Highlight (Optional)", labelStyle);
		button.highlightCanvasGroup = (CanvasGroup)EditorGUILayout.ObjectField(highlightCanvasGroupLabel, button.highlightCanvasGroup, typeof(CanvasGroup), true);
		button.animCurveHighlight = EditorGUILayout.CurveField(animCurveLabel, button.animCurveHighlight);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Disabled Effect", labelStyle);
		button.disableType = (SuperButton.DisableTypes)EditorGUILayout.EnumPopup(disableTypeLabel, button.disableType);
		switch (button.disableType)
		{
			case SuperButton.DisableTypes.CanvasGroupAlpha:
				button.disableCanvasGroup = (CanvasGroup)EditorGUILayout.ObjectField(disableCanvasGroupLabel, button.disableCanvasGroup, typeof(CanvasGroup), true);
				button.disableCanvasGroupAlpha = EditorGUILayout.FloatField(disableAlphaLabel, button.disableCanvasGroupAlpha);
				break;

			case SuperButton.DisableTypes.HierarchySwap:
				SerializedProperty enableWhenInteractable = serializedObject.FindProperty("enableWhenInteractable");
				EditorGUILayout.PropertyField(enableWhenInteractable, true);
				SerializedProperty enableWhenNotInteractable = serializedObject.FindProperty("enableWhenNotInteractable");
				EditorGUILayout.PropertyField(enableWhenNotInteractable, true);
				break;
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField(audioLabel, labelStyle);
		button.audioOnAnimStart = (UIMaster.AudioClips)EditorGUILayout.EnumPopup(audioOnAnimStartLabel, button.audioOnAnimStart);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Cooldown (Locks Out Other Buttons)", labelStyle);

		button.lockOtherButtonsDuringAnim = EditorGUILayout.Toggle(lockOthersDuringAnimLabel, button.lockOtherButtonsDuringAnim);
		button.lockCooldownAfterAnim = EditorGUILayout.FloatField(lockCooldownAfterAnimLabel, button.lockCooldownAfterAnim);

		SerializedProperty onAnimStart = serializedObject.FindProperty("onAnimStart");
		EditorGUILayout.PropertyField(onAnimStart);

		if (button.animationType != SuperButton.AnimationTypes.None)
		{
			SerializedProperty onAnimFinish = serializedObject.FindProperty("onAnimFinish");
			EditorGUILayout.PropertyField(onAnimFinish);
		}

		if (GUI.changed)
			serializedObject.ApplyModifiedProperties();
	}
}
#endif

public class SuperButton : Button
{
	public enum AnimationTypes { None, Scale, MoveUpDown, MoveLeftRight };
	public enum DisableTypes { None, CanvasGroupAlpha, HierarchySwap };
	static readonly Vector2 vec2Up = Vector2.up;
	static readonly Vector2 vec2Right = Vector2.right;

	#region Inspector variables

	/// <summary> Wrapper for Button's "interactable" property, which also refreshes elements </summary>
	public new bool interactable
	{
		get { return base.interactable; }
		set
		{
			base.interactable = value;
			RefreshInteractable(value);
		}
	}

	[Header("Animation")]
	public RectTransform transformToAnimate = null;
	public AnimationTypes animationType = AnimationTypes.Scale;
	public AnimationCurve animCurveScale = new AnimationCurve(new Keyframe(0f, 1f, 1f, 1f), new Keyframe(0.1f, 1.2f), new Keyframe(1f, 1f, -0.5f, -0.5f));
	public AnimationCurve animCurveMove = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(0.1f, 1f), new Keyframe(1f, 0f, -0.5f, -0.5f));
	public float animDuration = 0.2f;
	public float moveDistance = 10f;

	[Header("Highlight (Optional)")]
	public CanvasGroup highlightCanvasGroup = null;
	public AnimationCurve animCurveHighlight = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0f, -0.5f, 0f), new Keyframe(1f, 0f));

	[Header("Disabling")]
	public DisableTypes disableType = DisableTypes.CanvasGroupAlpha;
	public CanvasGroup disableCanvasGroup = null;
	[Range(0, 1)] public float disableCanvasGroupAlpha = 0.5f;
	public GameObject[] enableWhenInteractable = null;
	public GameObject[] enableWhenNotInteractable = null;

	[Header("Audio")]
	public UIMaster.AudioClips audioOnAnimStart = UIMaster.AudioClips.Select;

	[Header("Events")]
	public UnityEvent onAnimStart = null;
	public UnityEvent onAnimFinish = null;

	[Header("Cooldown")]
	public bool lockOtherButtonsDuringAnim = true;
	public float lockCooldownAfterAnim = 0.3f;

	#endregion // Inspector variables

	float animSpeed;
	static float cooldownEndTime;
	Vector3 origScale = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);	// default to ridiculous value to know if it's been set/used
	Vector2 origAnchoredPos = new Vector2(float.MaxValue, float.MaxValue);              // default to ridiculous value to know if it's been set/used
	Coroutine animCoroutine;

	protected override void OnEnable()
	{
		base.OnEnable();

		RefreshInteractable(interactable);
	}

	/// <summary> Called when object/script is disabled in the hierarchy </summary>
	protected override void OnDisable()
	{
		base.OnDisable();

		if (animCoroutine != null)
		{
			StopCoroutine(animCoroutine);
			animCoroutine = null;

			// Reset elements to default pos/scale/etc
			if (origScale.x < float.MaxValue - 0.1f)
				transformToAnimate.localScale = origScale;
			if (origAnchoredPos.x < float.MaxValue - 0.1f)
				transformToAnimate.anchoredPosition = origAnchoredPos;
			if (highlightCanvasGroup != null)
				highlightCanvasGroup.alpha = animCurveHighlight.Evaluate(animCurveHighlight.keys[animCurveHighlight.length - 1].time);
		}
	}

	/// <summary> Refreshes elements for interactable/disabled state </summary>
	/// <param name="_interactable"> True if button's interactable, false to show disabled state </param>
	void RefreshInteractable(bool _interactable)
	{
		switch (disableType)
		{
			case DisableTypes.CanvasGroupAlpha:
				if (disableCanvasGroup != null)
					disableCanvasGroup.alpha = (_interactable ? 1.0f : disableCanvasGroupAlpha);
#if UNITY_EDITOR
				else if (Application.isPlaying)
				{
					Debug.LogWarning("SuperButton '" + Helpers.GetFullName(transform) + "' has 'Disable Type' set to 'Canvas Group Alpha', but 'Disable Canvas Group' is not set! Switching it back to 'None'.");
					disableType = DisableTypes.None;
				}
#endif
				break;

			case DisableTypes.HierarchySwap:
				for (int i = 0; i < enableWhenInteractable.Length; ++i)
					enableWhenInteractable[i].SetActive(_interactable);
				for (int i = 0; i < enableWhenNotInteractable.Length; ++i)
					enableWhenNotInteractable[i].SetActive(!_interactable);
				break;
		}
	}

	/// <summary> Wrapper for Button.OnPointerClick </summary>
	public override void OnPointerClick(PointerEventData eventData)
	{
		OnClick();
	}

	/// <summary> Wrapper for Button.OnClick </summary>
	public void OnClick()
	{
		if (!interactable || (Time.time < cooldownEndTime))
			return;

		onAnimStart?.Invoke();

		if (audioOnAnimStart != UIMaster.AudioClips.None)
			UIMaster.instance.PlayAudio(audioOnAnimStart);

		animSpeed = 1f / animDuration;

		if (lockOtherButtonsDuringAnim)
			cooldownEndTime = Time.time + animDuration + 0.05f;

		// Start anim if necessary
		switch (animationType)
		{
			case AnimationTypes.None:
				onAnimFinish?.Invoke();
				break;

			case AnimationTypes.Scale:
				animCoroutine = StartCoroutine(StartAnimScale());
				break;

			case AnimationTypes.MoveUpDown:
			case AnimationTypes.MoveLeftRight:
				animCoroutine = StartCoroutine(StartAnimMove());
				break;
		}
	}

	/// <summary> Called when animation has completed </summary>
	public void OnAnimComplete()
	{
		cooldownEndTime = Time.time + lockCooldownAfterAnim;

		if (onAnimFinish != null)
			onAnimFinish.Invoke();
	}

	/// <summary> Performs the button's animation, triggering its callback when complete </summary>
	IEnumerator StartAnimScale()
	{
		origScale = transformToAnimate.localScale;

		float progress = 0f;
		while (progress < 1f)
		{
			progress += animSpeed * Time.deltaTime;
			transformToAnimate.localScale = origScale * animCurveScale.Evaluate(progress);

			if (highlightCanvasGroup != null)
				highlightCanvasGroup.alpha = animCurveHighlight.Evaluate(progress);

			yield return null;
		}

		OnAnimComplete();
	}

	/// <summary> Performs the button's animation, triggering its callback when complete </summary>
	IEnumerator StartAnimMove()
	{
		origAnchoredPos = transformToAnimate.anchoredPosition;
		Vector2 direction = moveDistance * ((animationType == AnimationTypes.MoveUpDown) ? vec2Up : vec2Right);

		float progress = 0f;
		while (progress < 1f)
		{
			progress += animSpeed * Time.deltaTime;
			transformToAnimate.localPosition = origAnchoredPos + (direction * animCurveMove.Evaluate(progress));

			if (highlightCanvasGroup != null)
				highlightCanvasGroup.alpha = animCurveHighlight.Evaluate(progress);

			yield return null;
		}

		OnAnimComplete();
	}
}
