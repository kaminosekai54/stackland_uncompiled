using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Selectable, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
	public Image Image;

	public bool EnableUnderline = true;

	public bool ButtonEnabled = true;

	private RectTransform rectTransform;

	private bool isDown;

	public Func<bool> IsSelectableAction;

	public SokScreen parentScreen;

	public bool SetColor = true;

	public AudioClip CustomSound;

	private Color startColor;

	public string TooltipText;

	private ScrollRect parentScrollRect;

	private bool? _isSelected;

	private bool triedFindingTmPro;

	private TextMeshProUGUI tmPro;

	private PointerEventData lastEventData;

	private float HorizontalStickTimer;

	public bool IsHovered;

	public bool ScrollToInRect = true;

	private bool tryFindParentScrollRect = true;

	public RectTransform RectTransform => (RectTransform)base.transform;

	public bool SelectableWithController
	{
		get
		{
			if (!this.ButtonEnabled)
			{
				return false;
			}
			if (this.parentScreen != null && (GameCanvas.instance.ModalIsOpen || (TransitionScreen.InTransition && !TransitionScreen.instance.IsLeaving)))
			{
				return false;
			}
			if (this.IsSelectableAction == null)
			{
				return true;
			}
			return this.IsSelectableAction();
		}
	}

	public bool IsSelected
	{
		get
		{
			if (!this._isSelected.HasValue)
			{
				if (InputController.instance.CurrentSchemeIsController && base.currentSelectionState == SelectionState.Selected)
				{
					this._isSelected = true;
				}
				else
				{
					this._isSelected = false;
				}
			}
			return this._isSelected.Value;
		}
	}

	public TextMeshProUGUI TextMeshPro
	{
		get
		{
			if (!this.triedFindingTmPro)
			{
				this.tmPro = base.GetComponent<TextMeshProUGUI>();
				if (this.tmPro == null)
				{
					this.tmPro = base.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
				}
				this.triedFindingTmPro = true;
			}
			return this.tmPro;
		}
	}

	private bool canBeClicked => !(this.parentScreen != null) || !GameCanvas.instance.ModalIsOpen;

	private Camera cam => null;

	public bool WasRightClick
	{
		get
		{
			if (this.lastEventData != null)
			{
				return this.lastEventData.button == PointerEventData.InputButton.Right;
			}
			return false;
		}
	}

	public bool IsClicked
	{
		get
		{
			bool result = false;
			if (this.isDown && InputController.instance.InputCount > 0 && RectTransformUtility.RectangleContainsScreenPoint(this.rectTransform, InputController.instance.GetInputPosition(0), this.cam))
			{
				result = true;
			}
			if (TransitionScreen.InTransition || !this.ButtonEnabled || !GameCanvas.instance.ScreenIsInteractable(this.parentScreen) || (InputController.instance.IsUsingMouse && InputController.instance.MouseIsDragging))
			{
				result = false;
			}
			return result;
		}
	}

	[HideInInspector]
	public event Action Clicked;

	[HideInInspector]
	public event Action<Vector2> StartDragging;

	public event Func<CustomButton, Navigation, Navigation> ExplicitNavigationChanged;

	protected override void Awake()
	{
		if (Application.isPlaying)
		{
			this.Image = base.GetComponent<Image>();
			this.rectTransform = base.GetComponent<RectTransform>();
			this.startColor = this.Image.color;
			base.Awake();
		}
	}

	protected override void Start()
	{
		if (Application.isPlaying)
		{
			this.parentScreen = GameCanvas.instance.GetParentScreen(this.rectTransform);
			if (this.parentScreen == null && Application.isEditor)
			{
				Debug.LogWarning("No parent screen found for " + base.name);
			}
			base.Start();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (this.canBeClicked)
		{
			this.isDown = true;
		}
		base.OnPointerDown(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (this.isDown)
		{
			this.lastEventData = eventData;
			if (RectTransformUtility.RectangleContainsScreenPoint(this.rectTransform, eventData.position, this.cam) && this.canBeClicked)
			{
				this.SubmitClick();
			}
			this.isDown = false;
		}
		base.OnPointerUp(eventData);
	}

	private void HorizontalStick()
	{
		Slider componentInChildren = base.GetComponentInChildren<Slider>();
		if (!(componentInChildren == null))
		{
			if (this.HorizontalStickTimer == 0f)
			{
				componentInChildren.value += ((InputController.instance.GetMove().x > 0f) ? 0.05f : (-0.05f));
			}
			this.HorizontalStickTimer += Time.deltaTime;
			if (this.HorizontalStickTimer > 1.15f - Mathf.Abs(InputController.instance.GetMove().x))
			{
				this.HorizontalStickTimer = 0f;
			}
		}
	}

	private void SubmitClick()
	{
		if (this.parentScreen == null)
		{
			this.parentScreen = GameCanvas.instance.GetParentScreen(this.rectTransform);
		}
		bool flag = this.parentScreen == null || GameCanvas.instance.ScreenIsInteractable(this.parentScreen);
		bool flag2 = InputController.instance.IsUsingMouse && InputController.instance.MouseIsDragging;
		if (this.Clicked != null && !TransitionScreen.InTransition && flag && !flag2 && this.ButtonEnabled)
		{
			this.Clicked();
			if (this.CustomSound == null)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.Click, 1f, 0.1f);
				return;
			}
			AudioManager.me.PlaySound2D(new List<AudioClip> { this.CustomSound }, 1f, 0.1f);
		}
	}

	protected override void OnDisable()
	{
		this.isDown = false;
		base.OnDisable();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.tryFindParentScrollRect)
		{
			this.tryFindParentScrollRect = false;
			this.parentScrollRect = base.GetComponentInParent<ScrollRect>();
		}
		if (this.ButtonEnabled && this.IsSelected && InputController.instance != null && InputController.instance.CurrentSchemeIsController && this.SelectableWithController)
		{
			if (InputController.instance.SubmitTriggered())
			{
				this.SubmitClick();
			}
			if (InputController.instance.GetStickHorizontal())
			{
				this.HorizontalStick();
			}
			else
			{
				this.HorizontalStickTimer = 0f;
			}
			if (this.parentScrollRect != null && this.ScrollToInRect)
			{
				this.ScrollToMe();
			}
		}
		if (this.IsHovered)
		{
			Tooltip.Text = this.TooltipText;
		}
		Color color = (this.SetColor ? ColorManager.instance.ButtonColor : this.startColor);
		if ((this.IsHovered || this.IsSelected) && this.ButtonEnabled && this.SetColor)
		{
			color = ColorManager.instance.HoverButtonColor;
		}
		if (this.TextMeshPro != null)
		{
			if (!this.ButtonEnabled)
			{
				this.TextMeshPro.color = ColorManager.instance.DisabledButtonTextColor;
			}
			else
			{
				this.TextMeshPro.color = ColorManager.instance.ButtonTextColor;
			}
			FontStyles fontStyle = this.TextMeshPro.fontStyle;
			fontStyle = (((!this.IsHovered && !this.IsSelected) || !this.ButtonEnabled || !this.EnableUnderline) ? (fontStyle & ~FontStyles.Underline) : (fontStyle | FontStyles.Underline));
			this.TextMeshPro.fontStyle = fontStyle;
		}
		if (this.SetColor && this.Image != null)
		{
			this.Image.color = color;
		}
		base.interactable = this.ButtonEnabled;
		if (!InputController.instance.CurrentSchemeIsController)
		{
			return;
		}
		if (!this.SelectableWithController)
		{
			Navigation navigation = base.navigation;
			navigation.mode = Navigation.Mode.None;
			base.navigation = navigation;
			return;
		}
		Navigation arg = base.navigation;
		if (this.ExplicitNavigationChanged != null && this.IsSelected)
		{
			arg.mode = Navigation.Mode.Explicit;
			arg.selectOnLeft = base.FindSelectable(Vector3.left);
			arg.selectOnRight = base.FindSelectable(Vector3.right);
			arg.selectOnUp = base.FindSelectable(Vector3.up);
			arg.selectOnDown = base.FindSelectable(Vector3.down);
			arg = this.ExplicitNavigationChanged(this, arg);
		}
		else
		{
			arg.mode = Navigation.Mode.Automatic;
		}
		base.navigation = arg;
	}

	public void ScrollToMe()
	{
		GameCanvas.SetScrollRectPosition(this.parentScrollRect, this.rectTransform, centerInView: true);
	}

	private void LateUpdate()
	{
		if (Application.isPlaying)
		{
			if (this.IsSelected && (!this.SelectableWithController || !this.ButtonEnabled) && InputController.instance.CurrentSchemeIsController)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			this._isSelected = null;
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (this.StartDragging != null)
		{
			this.StartDragging(eventData.position);
			return;
		}
		this.isDown = false;
		base.GetComponentInParent<ScrollRect>()?.OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		base.GetComponentInParent<ScrollRect>()?.OnDrag(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		base.GetComponentInParent<ScrollRect>()?.OnEndDrag(eventData);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
	}

	public void HardSetText(string text)
	{
		base.GetComponentInChildren<TextMeshProUGUI>().text = text;
	}
}
