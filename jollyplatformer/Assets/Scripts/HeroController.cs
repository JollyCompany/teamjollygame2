using UnityEngine;
using System.Collections;

public class HeroController : MonoBehaviour
{
	public int PlayerNumber;

	public float HorizontalMovementAxis
	{
		get
		{
			return this.GetComponent<GameControllerInput>().GetAxis (GameControllerInput.Axis.AnalogLeftHorizontal);
			//return Input.GetAxis (string.Format ("Horizontal[{0}]", this.PlayerNumber));
		}
	}

	public bool Jump
	{
		get
		{
			return this.GetComponent<GameControllerInput>().GetButtonDown (GameControllerInput.Button.A);
			//return Input.GetButtonDown(string.Format ("Jump[{0}]", this.PlayerNumber));
		}
	}

	public bool Shooting
	{
		get
		{
			return this.GetComponent<GameControllerInput>().GetButton (GameControllerInput.Button.X);
			//return Input.GetButton(string.Format ("Fire[{0}]", this.PlayerNumber));
		}
	}

	public bool GetBiggerStart
	{
		get
		{
			return this.GetComponent<GameControllerInput>().GetButtonDown (GameControllerInput.Button.B);
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
