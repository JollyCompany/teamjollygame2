using UnityEngine;
using System.Collections;

public class GameControllerInput : MonoBehaviour
{
	public int ControllerJoystickNumber;

	public enum Controller
	{
		XboxOne,
		PlayStation4
	};

	public Controller ControllerKind;

	public enum Axis
	{
		TriggerLeft,
		TriggerRight,
		AnalogLeftHorizontal,
		AnalogLeftVertical,
		AnalogRightHorizontal,
		AnalogRightVertical
	};

	public enum Button
	{
		ShoulderLeft,
		ShoulderRight,
		AnalogStickClickLeft,
		AnalogStickClickRight,
		A,
		B,
		X,
		Y,
		DPadUp,
		DPadDown,
		DPadLeft,
		DPadRight,
		Start,
		Select
	};

	private static string[] controllerCodeRef = { "XB1", "PS4" };
	private static string[] buttonCodeRef = { "SL", "SR", "LAC", "RAC", "A", "B", "X", "Y", "DU", "DD", "DL", "DR", "STA", "SEL" };
	private static string[] axisCodeRef = { "LT", "RT", "LAH", "LAV", "RAH", "RAV" };

	private string controllerCode
	{
		get
		{
			return controllerCodeRef[(int)this.ControllerKind];
		}
	}

	public bool GetButton (Button button)
	{
		string name = string.Format("J{0}{1}{2}", this.ControllerJoystickNumber, this.controllerCode, buttonCodeRef[(int)button]);
		if (this.ControllerKind == Controller.PlayStation4 &&
		    (button == Button.DPadUp || button == Button.DPadDown || button == Button.DPadLeft || button == Button.DPadRight))
		{
			return Input.GetAxis (name) > 0.5f;
		}
		return Input.GetButton (name);
	}

	public bool GetButtonDown (Button button)
	{
		string name = string.Format("J{0}{1}{2}", this.ControllerJoystickNumber, this.controllerCode, buttonCodeRef[(int)button]);
		if (this.ControllerKind == Controller.PlayStation4 &&
		    (button == Button.DPadUp || button == Button.DPadDown || button == Button.DPadLeft || button == Button.DPadRight))
		{
			throw new System.NotImplementedException();
		}
		return Input.GetButtonDown (name);
	}

	public bool GetButtonUp (Button button)
	{
		string name = string.Format("J{0}{1}{2}", this.ControllerJoystickNumber, this.controllerCode, buttonCodeRef[(int)button]);
		if (this.ControllerKind == Controller.PlayStation4 &&
		    (button == Button.DPadUp || button == Button.DPadDown || button == Button.DPadLeft || button == Button.DPadRight))
		{
			throw new System.NotImplementedException();
		}
		return Input.GetButtonUp (name);
	}

	public float GetAxis (Axis axis)
	{
		string name = string.Format("J{0}{1}{2}", this.ControllerJoystickNumber, this.controllerCode, axisCodeRef[(int)axis]);
		return Input.GetAxis (name);
	}
}
