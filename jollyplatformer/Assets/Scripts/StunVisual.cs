using UnityEngine;
using System.Collections;
using Jolly;

public class StunVisual : MonoBehaviour
{
	public Hero Hero;
	public float RotationTime = 1.0f;
	private float TimeSpun = 0;
	private GameObject soundObject;

	void Start ()
	{
		this.soundObject = SoundFX.Instance.OnHeroStunned(this.Hero);
	}

	public void Stop()
	{
		AudioSourceExt.StopClipOnObject(this.soundObject);
		Destroy(this.soundObject);
		Destroy(this.gameObject);
	}

	void Update ()
	{
		this.TimeSpun += Time.deltaTime;
		this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 360.0f * ((this.TimeSpun % this.RotationTime) / this.RotationTime)));
	}
}
