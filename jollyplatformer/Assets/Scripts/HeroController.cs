using UnityEngine;
using System.Collections;
using InControl;

public class HeroController : MonoBehaviour
{
	private int playerNumber;

	void Start ()
	{
		string playerNumberString = this.name.Substring(this.name.Length-2, 1);
		Debug.Log (playerNumberString);
		this.playerNumber = int.Parse(playerNumberString);
	}

	public InputDevice InputDevice
	{
		get
		{
			if (this.playerNumber >= InputManager.Devices.Count)
			{
				return null;
			}
			return InputManager.Devices[this.playerNumber];
		}
	}

	public float HorizontalMovementAxis
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? 0.0f : inputDevice.LeftStickX;
		}
	}

	public bool Jump
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? false : inputDevice.Action1.WasPressed;
		}
	}

	public bool Shooting
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? false : inputDevice.Action2.WasPressed;
		}
	}

	public bool GetBiggerStart
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? false : inputDevice.Action3.WasPressed;
		}
	}

	public bool GetBiggerHold
	{
		get
		{
			return this.GetComponent<GameControllerInput>().GetButton (GameControllerInput.Button.B);
		}
	}

	public bool GetBiggerEnd
	{
		get
		{
			return this.GetComponent<GameControllerInput>().GetButtonUp (GameControllerInput.Button.B);
		}
	}
}
