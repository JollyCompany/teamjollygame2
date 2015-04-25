using UnityEngine;
using System.Collections;

public class ChannelVisual : MonoBehaviour
{
	public float ChannelTime;
	private float TimeRemaining;

	void Start ()
	{
		this.TimeRemaining = this.ChannelTime;
	}

	void Update ()
	{
		this.TimeRemaining -= Time.deltaTime;
		this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 360.0f * (this.TimeRemaining / this.ChannelTime)));

		if (this.TimeRemaining < 0.0f)
		{
			Destroy (this.gameObject);
		}
	}
}
