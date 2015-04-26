using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour
{
	public enum Type
	{
		GrowInstantly
	}

	public Type PickupType;

	public float ExpirationTime;
	public float StartBlinkTime;

	void Update ()
	{
		if (Time.time > this.ExpirationTime)
		{
			GameObject.Destroy (this);
			return;
		}

		float dt = Time.time - this.StartBlinkTime;
		if (dt >= 0.0f)
		{
			this.GetComponent<SpriteRenderer>().enabled = (Mathf.FloorToInt (dt / 0.35f) % 2 == 0);
		}
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		var hero = other.GetComponent<Hero>();
		if (null == hero)
		{
			hero.Grow();
			return;
		}
		GameObject.Destroy (this);
	}
}
