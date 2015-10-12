using UnityEngine;
using System.Collections;
using Jolly;

public class Pickup : MonoBehaviour
{
	public enum Type
	{
		GrowInstantly,
		Shield
	}

	public Type PickupType;

	public float ExpirationTime;
	public float StartBlinkTime;

	void Update ()
	{
		if (Time.time > this.ExpirationTime)
		{
			GameObject.Destroy (this.gameObject);
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
		var hero = other.gameObject.GetComponent<Hero>();
		if (null == hero)
		{
			return;
		}
		if (this.PickupType == Type.Shield)
		{
			ShieldBuff.AddToHero (hero);
		}
		else
		{
			hero.Grow(true);
		}
		GameObject.Destroy (this.gameObject);
	}
}
