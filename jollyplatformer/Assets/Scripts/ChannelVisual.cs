using UnityEngine;
using System.Collections;
using Jolly;

public class ChannelVisual : MonoBehaviour
{
	public float ChannelTime;
	public Hero Hero;
	private float TimeRemaining;
	private GameObject soundObject;

	void Start ()
	{
		this.TimeRemaining = this.ChannelTime;
		GameObject soundObject = SoundFX.Instance.OnHeroGrowChannel(this.Hero);
	}

	void Update ()
	{
		this.TimeRemaining -= Time.deltaTime;
		this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 360.0f * (this.TimeRemaining / this.ChannelTime)));

		if (this.TimeRemaining < 0.0f)
		{
			AudioSourceExt.StopClipOnObject(this.soundObject);
			Destroy (this.gameObject);
		}
	}
}
