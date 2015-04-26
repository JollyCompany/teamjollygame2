using UnityEngine;
using System.Collections;
using InControl;

public class HeroController : MonoBehaviour
{
	public int PlayerNumber;

	void Start ()
	{
		string playerNumberString = this.name.Substring(this.name.Length-2, 1);
		this.PlayerNumber = int.Parse(playerNumberString);
	}

	public InputDevice InputDevice
	{
		get
		{
			if (this.PlayerNumber >= InputManager.Devices.Count)
			{
				return null;
			}
			return InputManager.Devices[this.PlayerNumber];
		}
	}

	public float HorizontalMovementAxis
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetAxis ("Horizontal") : inputDevice.LeftStickX;
		}
	}

	public bool Jump
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetButtonDown ("Fire1") : inputDevice.Action1.WasPressed;
		}
	}

	public bool Shooting
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetButton ("Fire2") : inputDevice.Action2.WasPressed;
		}
	}

	public bool Stomping
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetButton ("Fire4") : inputDevice.Action4.WasPressed;
		}
	}

	public bool GetBiggerStart
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetButtonDown ("Fire3") : inputDevice.Action3.WasPressed;
		}
	}

	public bool GetBiggerHold
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetButton ("Fire3") : inputDevice.Action3.IsPressed;
		}
	}

	public bool GetBiggerEnd
	{
		get
		{
			InputDevice inputDevice = this.InputDevice;
			return inputDevice == null ? Input.GetButtonUp ("Fire3") : inputDevice.Action3.WasReleased;
		}
	}
}
