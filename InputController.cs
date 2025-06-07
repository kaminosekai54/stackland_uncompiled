using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Users;

public class InputController : MonoBehaviour
{
	private class UserInput
	{
		public Vector2 ScreenPosition;

		public Vector2 DeltaPosition = Vector2.zero;

		public Vector2 StartPosition;

		public float StartTime;

		public int MouseId = -1;

		public int TouchId = -1;

		public int PenId = -1;

		public bool JustStarted = true;

		public bool JustEnded;

		public bool UpdatedThisFrame;

		public override string ToString()
		{
			return this.MouseId + " " + this.TouchId + this.ScreenPosition.ToString() + " " + this.DeltaPosition.ToString() + " " + this.JustStarted;
		}
	}

	public static InputController instance;

	public PlayerInput PlayerInput;

	public bool DisableAllInput;

	private string inputString;

	public string ActiveScheme;

	private List<UserInput> Inputs = new List<UserInput>();

	private List<UserInput> inputsToRemove = new List<UserInput>();

	public ControllerVibrator Vibrator;

	private InputAction cancel;

	private InputAction submit;

	private InputAction time_pause;

	private InputAction pause;

	private InputAction move;

	private InputAction snap_cards;

	private InputAction time_1;

	private InputAction time_2;

	private InputAction time_3;

	private InputAction zoom;

	private InputAction panel_collapse;

	private InputAction activate_ui;

	private InputAction time_toggle;

	private InputAction sell;

	private InputAction toggle_inventory;

	private InputAction toggle_view;

	private InputAction grab;

	private InputAction snap_move;

	private bool mouseIsDragging;

	private Vector2 lastMove;

	private float lastGrab;

	private Vector2 lastSnapMove;

	private ControlScheme lastControlScheme;

	public int LastInputCount;

	private Dictionary<string, string> bindingDisplayCache = new Dictionary<string, string>();

	public ControlScheme? SchemeOverride;

	private ControlScheme? _currentScheme;

	private InputDevice lastDevice;

	public string InputString => this.inputString;

	public bool IsUsingMouse
	{
		get
		{
			if (this.CurrentSchemeIsController || this.CurrentSchemeIsTouch)
			{
				return false;
			}
			return Mouse.current != null;
		}
	}

	public bool MouseIsDragging => this.mouseIsDragging;

	private float dragDistanceThreshold => (float)Screen.height * 0.025f;

	private float dragTimeThreshold => 0.4f;

	private bool TouchesEnabled => true;

	public int InputCount => this.Inputs.Count;

	public ControlScheme CurrentScheme
	{
		get
		{
			if (this.SchemeOverride.HasValue)
			{
				return this.SchemeOverride.Value;
			}
			if (!this._currentScheme.HasValue)
			{
				this._currentScheme = this.GetSchemeFromName(this.PlayerInput.currentControlScheme);
			}
			return this._currentScheme.Value;
		}
	}

	public bool CurrentSchemeIsController => this.CurrentScheme == ControlScheme.Controller;

	public bool CurrentSchemeIsMouseKeyboard => this.CurrentScheme == ControlScheme.KeyboardMouse;

	public bool CurrentSchemeIsTouch => this.CurrentScheme == ControlScheme.Touch;

	public event Action<ControlScheme> ControlSchemeChanged;

	private void Awake()
	{
		InputController.instance = this;
		this.SetupInputActions();
		EnhancedTouchSupport.Enable();
		this.Vibrator = new ControllerVibrator();
		InputSystem.pollingFrequency = 120f;
		InputUser.onChange += InputUser_onChange;
		InputSystem.onActionChange += delegate(object obj, InputActionChange change)
		{
			if (change == InputActionChange.ActionPerformed)
			{
				this.lastDevice = ((InputAction)obj).activeControl.device;
			}
		};
		if (Keyboard.current != null)
		{
			Keyboard.current.onTextInput += OnTextInput;
		}
	}

	private void OnTextInput(char c)
	{
		this.inputString += c;
	}

	private void SetupInputActions()
	{
		this.cancel = this.PlayerInput.actions["cancel"];
		this.submit = this.PlayerInput.actions["submit"];
		this.time_pause = this.PlayerInput.actions["time_pause"];
		this.pause = this.PlayerInput.actions["pause"];
		this.move = this.PlayerInput.actions["move"];
		this.snap_cards = this.PlayerInput.actions["snap_cards"];
		this.time_1 = this.PlayerInput.actions["time_1"];
		this.time_2 = this.PlayerInput.actions["time_2"];
		this.time_3 = this.PlayerInput.actions["time_3"];
		this.zoom = this.PlayerInput.actions["zoom"];
		this.panel_collapse = this.PlayerInput.actions["panel_collapse"];
		this.activate_ui = this.PlayerInput.actions["activate_ui"];
		this.time_toggle = this.PlayerInput.actions["time_toggle"];
		this.sell = this.PlayerInput.actions["sell"];
		this.toggle_inventory = this.PlayerInput.actions["toggle_inventory"];
		this.toggle_view = this.PlayerInput.actions["toggle_view"];
		this.grab = this.PlayerInput.actions["grab"];
		this.snap_move = this.PlayerInput.actions["snap_move"];
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			if (Keyboard.current != null)
			{
				InputSystem.ResetDevice(Keyboard.current);
			}
			this.ClearInputs();
			if (this.Vibrator != null)
			{
				this.Vibrator.StopVibrate();
			}
		}
	}

	private void OnDestroy()
	{
		if (this.Vibrator != null)
		{
			this.Vibrator.StopVibrate();
		}
		InputUser.onChange -= InputUser_onChange;
	}

	public void ClearInputs()
	{
		this.Inputs.Clear();
	}

	public void LogCurrentState()
	{
		string text = "Input controller state log\n";
		foreach (UserInput input in this.Inputs)
		{
			text = text + input.ToString() + "\n";
		}
		text += $"Active touches report: {Touch.activeTouches.Count} touches!";
		Debug.Log(text);
	}

	private ButtonControl GetMouseButton(int buttonId)
	{
		return buttonId switch
		{
			0 => Mouse.current.leftButton, 
			1 => Mouse.current.rightButton, 
			_ => Mouse.current.middleButton, 
		};
	}

	public Vector2 ClampedMousePosition()
	{
		if (Mouse.current == null)
		{
			return new Vector2(Screen.width, Screen.height) * 0.5f;
		}
		Vector2 result = Mouse.current.position.ReadValue();
		result.x = Mathf.Clamp(result.x, 0f, Screen.width);
		result.y = Mathf.Clamp(result.y, 0f, Screen.height);
		return result;
	}

	private bool MousePositionIsInScreen()
	{
		Vector2 vector = Mouse.current.position.ReadValue();
		if (vector.x > 0f && vector.x < (float)Screen.width && vector.y > 0f)
		{
			return vector.y < (float)Screen.height;
		}
		return false;
	}

	public Vector2 GetSafeTouchPosition(int i)
	{
		if (!this.GetInput(i))
		{
			return new Vector2(Screen.width, Screen.height) * 0.5f;
		}
		return this.GetInputPosition(i);
	}

	private void Update()
	{
		InputSystem.Update();
		this.Vibrator.UpdateVibrate(Time.unscaledDeltaTime);
		foreach (UserInput item in this.inputsToRemove)
		{
			this.Inputs.Remove(item);
		}
		this.inputsToRemove.Clear();
		foreach (UserInput input in this.Inputs)
		{
			input.JustStarted = false;
			input.UpdatedThisFrame = false;
		}
		if (Mouse.current != null)
		{
			int num = 0;
			if (AccessibilityScreen.ClickToDragEnabled)
			{
				num = 1;
			}
			int mouseId;
			for (mouseId = 0; mouseId <= num; mouseId++)
			{
				ButtonControl mouseButton = this.GetMouseButton(mouseId);
				if (mouseButton.wasPressedThisFrame)
				{
					if (this.MousePositionIsInScreen() && this.Inputs.Count <= 0)
					{
						this.mouseIsDragging = false;
						this.Inputs.Add(new UserInput
						{
							ScreenPosition = this.ClampedMousePosition(),
							MouseId = mouseId,
							JustStarted = true,
							StartPosition = this.ClampedMousePosition(),
							StartTime = Time.time,
							UpdatedThisFrame = true
						});
					}
					continue;
				}
				if (mouseButton.isPressed)
				{
					UserInput userInput = this.Inputs.FirstOrDefault((UserInput x) => x.MouseId == mouseId);
					if (userInput != null)
					{
						userInput.DeltaPosition = this.ClampedMousePosition() - userInput.ScreenPosition;
						userInput.ScreenPosition = this.ClampedMousePosition();
						userInput.UpdatedThisFrame = true;
						if (!this.mouseIsDragging && ((userInput.StartPosition - userInput.ScreenPosition).magnitude > this.dragDistanceThreshold || Time.time - userInput.StartTime >= this.dragTimeThreshold))
						{
							this.mouseIsDragging = true;
						}
					}
					continue;
				}
				for (int i = 0; i < this.Inputs.Count; i++)
				{
					if (this.Inputs[i].MouseId == mouseId)
					{
						this.Inputs[i].JustEnded = true;
						this.Inputs[i].UpdatedThisFrame = true;
						this.inputsToRemove.Add(this.Inputs[i]);
					}
				}
			}
		}
		else
		{
			this.mouseIsDragging = false;
		}
		if (this.Inputs.Count == 0)
		{
			this.mouseIsDragging = false;
		}
		if (this.TouchesEnabled)
		{
			for (int j = 0; j < Touch.activeTouches.Count; j++)
			{
				Touch touch = Touch.activeTouches[j];
				if (!touch.valid)
				{
					Debug.Log("Invalid touch in active touches!");
					continue;
				}
				if (touch.phase == TouchPhase.Began)
				{
					this.Inputs.Add(new UserInput
					{
						ScreenPosition = touch.screenPosition,
						TouchId = touch.touchId,
						JustStarted = true,
						StartPosition = touch.screenPosition,
						StartTime = Time.time,
						UpdatedThisFrame = true
					});
					continue;
				}
				if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
				{
					UserInput userInput2 = this.Inputs.FirstOrDefault((UserInput x) => x.TouchId == touch.touchId);
					if (userInput2 != null)
					{
						userInput2.DeltaPosition = touch.delta;
						userInput2.ScreenPosition = touch.screenPosition;
						userInput2.UpdatedThisFrame = true;
					}
					continue;
				}
				for (int k = 0; k < this.Inputs.Count; k++)
				{
					if (this.Inputs[k].TouchId == touch.touchId)
					{
						this.Inputs[k].JustEnded = true;
						this.Inputs[k].UpdatedThisFrame = true;
						this.inputsToRemove.Add(this.Inputs[k]);
					}
				}
			}
		}
		this.UpdatePen();
		for (int num2 = this.Inputs.Count - 1; num2 >= 0; num2--)
		{
			if (!this.Inputs[num2].UpdatedThisFrame)
			{
				Debug.Log("Removed a non-updated input!");
				this.Inputs.RemoveAt(num2);
			}
		}
		Cursor.visible = !this.CurrentSchemeIsController;
		if (this.lastControlScheme != this.CurrentScheme)
		{
			this.ClearBindingDisplayCache();
		}
		this.ActiveScheme = this.PlayerInput.currentControlScheme;
		this.lastControlScheme = this.CurrentScheme;
	}

	private void UpdatePen()
	{
		if (Pen.current == null)
		{
			return;
		}
		ButtonControl tip = Pen.current.tip;
		Vector2Control position = Pen.current.position;
		Vector2 vector = new Vector2(position.x.ReadValue(), position.y.ReadValue());
		if (tip.wasPressedThisFrame)
		{
			this.Inputs.Add(new UserInput
			{
				ScreenPosition = vector,
				PenId = Pen.current.deviceId,
				JustStarted = true,
				StartPosition = vector,
				StartTime = Time.time,
				UpdatedThisFrame = true
			});
			return;
		}
		if (tip.isPressed)
		{
			UserInput userInput = this.Inputs.FirstOrDefault((UserInput x) => x.PenId == Pen.current.deviceId);
			if (userInput != null)
			{
				userInput.DeltaPosition = vector - userInput.ScreenPosition;
				userInput.ScreenPosition = vector;
				userInput.UpdatedThisFrame = true;
			}
			return;
		}
		for (int i = 0; i < this.Inputs.Count; i++)
		{
			if (this.Inputs[i].PenId == Pen.current.deviceId)
			{
				this.Inputs[i].JustEnded = true;
				this.Inputs[i].UpdatedThisFrame = true;
				this.inputsToRemove.Add(this.Inputs[i]);
			}
		}
	}

	private void LateUpdate()
	{
		this.lastGrab = this.GetGrab();
		this.lastMove = this.GetMove();
		this.lastSnapMove = this.GetSnapMove();
		this.inputString = "";
		this.LastInputCount = this.InputCount;
		this._currentScheme = null;
	}

	public Vector2 AllDeltaPos()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		if (this.InputCount == 0)
		{
			return Vector2.zero;
		}
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < this.InputCount; i++)
		{
			zero += this.GetDeltaPosition(i);
		}
		return zero / this.InputCount;
	}

	public bool GetInputBegan(int i)
	{
		if (this.GetInput(i))
		{
			return this.Inputs[i].JustStarted;
		}
		return false;
	}

	public bool GetRightMouseBegan()
	{
		if (this.GetInput(0) && this.Inputs[0].JustStarted)
		{
			return this.Inputs[0].MouseId == 1;
		}
		return false;
	}

	public bool GetLeftMouseBegan()
	{
		if (this.GetInput(0) && this.Inputs[0].JustStarted)
		{
			return this.Inputs[0].MouseId == 0;
		}
		return false;
	}

	public bool GetRightMouseEnded()
	{
		if (this.GetInput(0) && this.Inputs[0].JustEnded)
		{
			return this.Inputs[0].MouseId == 1;
		}
		return false;
	}

	public bool GetLeftMouseEnded()
	{
		if (this.GetInput(0) && this.Inputs[0].JustEnded)
		{
			return this.Inputs[0].MouseId == 0;
		}
		return false;
	}

	public bool GetInputTapped(int i)
	{
		UserInput userInput = this.Inputs[i];
		if (userInput.JustEnded && Time.time - userInput.StartTime <= 0.15f)
		{
			return (userInput.ScreenPosition - userInput.StartPosition).magnitude < (float)Screen.width * 0.05f;
		}
		return false;
	}

	public bool IsNotRightClick(int i)
	{
		return this.Inputs[i].MouseId != 1;
	}

	public bool GetInputEnded(int i)
	{
		if (this.GetInput(i))
		{
			return this.Inputs[i].JustEnded;
		}
		return false;
	}

	public bool GetInput(int i)
	{
		return this.Inputs.Count > i;
	}

	public bool GetInputMoving(int i)
	{
		if (this.Inputs.Count > i)
		{
			return this.Inputs[i].DeltaPosition != Vector2.zero;
		}
		return false;
	}

	public Vector2 GetDeltaPosition(int i)
	{
		return this.Inputs[i].DeltaPosition;
	}

	public Vector2 GetDeltaPositionSinceStart(int i)
	{
		return this.Inputs[i].ScreenPosition - this.Inputs[i].StartPosition;
	}

	public Vector2 GetInputPosition(int i)
	{
		return this.Inputs[i].ScreenPosition;
	}

	public Vector2 GetStartPosition(int i)
	{
		return this.Inputs[i].StartPosition;
	}

	public bool GetStickHorizontal()
	{
		if (!((double)this.move.ReadValue<Vector2>().x > 0.3))
		{
			return (double)this.move.ReadValue<Vector2>().x < -0.3;
		}
		return true;
	}

	public bool CancelTriggered()
	{
		return this.cancel.triggered;
	}

	public bool SubmitTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.submit.triggered;
	}

	public bool TimePauseTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.time_pause.triggered;
	}

	public bool PauseTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.pause.triggered;
	}

	public bool SnapCardsTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.snap_cards.triggered;
	}

	public bool Time1_Triggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.time_1.triggered;
	}

	public bool Time2_Triggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.time_2.triggered;
	}

	public bool Time3_Triggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.time_3.triggered;
	}

	public float GetZoom()
	{
		if (this.DisableAllInput)
		{
			return 0f;
		}
		return this.zoom.ReadValue<float>();
	}

	public bool PanelCollapse_Triggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.panel_collapse.triggered;
	}

	public bool ActivateUI_Triggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.activate_ui.triggered;
	}

	public bool TimeToggleTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.time_toggle.triggered;
	}

	public bool SellTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.sell.triggered;
	}

	public bool ToggleInventoryTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.toggle_inventory.triggered;
	}

	public bool ToggleViewTriggered()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.toggle_view.triggered;
	}

	public Vector2 GetMove()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		return this.move.ReadValue<Vector2>();
	}

	public Vector2 GetSnapMovePressed()
	{
		Vector2 snapMove = this.GetSnapMove();
		if (snapMove.magnitude == 0f)
		{
			return Vector2.zero;
		}
		return (snapMove - this.lastSnapMove).normalized;
	}

	public Vector2 GetSnapMove()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		return this.snap_move.ReadValue<Vector2>();
	}

	public float GetGrab()
	{
		if (this.DisableAllInput)
		{
			return 0f;
		}
		return this.grab.ReadValue<float>();
	}

	public Vector2 GetDeltaMove()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		return this.GetMove() - this.lastMove;
	}

	public bool StartedGrabbing()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		return this.grab.triggered;
	}

	public bool StoppedGrabbing()
	{
		if (this.DisableAllInput)
		{
			return false;
		}
		if (this.GetGrab() < 0.5f)
		{
			return this.lastGrab > 0.5f;
		}
		return false;
	}

	public string GetActionDisplayString(string name)
	{
		if (!this.bindingDisplayCache.ContainsKey(name))
		{
			this.bindingDisplayCache[name] = "[" + this.PlayerInput.actions[name].GetBindingDisplayString() + "]";
		}
		return this.bindingDisplayCache[name];
	}

	public void ClearBindingDisplayCache()
	{
		this.bindingDisplayCache.Clear();
	}

	public bool GetKeyDown(Key key)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current[key].wasPressedThisFrame;
	}

	public bool GetKey(Key key)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current[key].isPressed;
	}

	public bool AnyInputDone()
	{
		if (this.InputCount > 0 && this.GetInputTapped(0))
		{
			return true;
		}
		if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
		{
			return true;
		}
		if (this.SubmitTriggered())
		{
			return true;
		}
		return false;
	}

	private ControlScheme GetSchemeFromName(string scheme)
	{
		if (scheme == "Keyboard&Mouse")
		{
			return ControlScheme.KeyboardMouse;
		}
		if (scheme == "Gamepad")
		{
			return ControlScheme.Controller;
		}
		return ControlScheme.Touch;
	}

	private void InputUser_onChange(InputUser user, InputUserChange change, InputDevice device)
	{
		if (change == InputUserChange.ControlSchemeChanged)
		{
			ControlScheme schemeFromName = this.GetSchemeFromName(user.controlScheme.Value.name);
			this.ControlSchemeChanged?.Invoke(schemeFromName);
		}
	}

	public void LogDevices()
	{
		for (int i = 0; i < InputSystem.devices.Count; i++)
		{
			InputDevice inputDevice = InputSystem.devices[i];
			Debug.Log($"Device {i}\n" + "Display name: " + inputDevice.displayName + "\nInterface name: " + inputDevice.description.interfaceName + "\nDevice class: " + inputDevice.description.deviceClass + "\nProduct: " + inputDevice.description.product + "\n");
		}
	}
}
