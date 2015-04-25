using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
	public float Lifetime;
	private float LifetimeRemaining;
	public Hero OwnerHero;

	void Start ()
	{
		this.LifetimeRemaining = this.Lifetime;
	}

	void Update ()
	{
		this.LifetimeRemaining -= Time.deltaTime;

		if (this.LifetimeRemaining < 0.0f)
		{
			Destroy (this.gameObject);
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		Hero hero = collision.gameObject.GetComponent<Hero>();
		if (hero != null)
		{
			hero.Hit(this.OwnerHero);
		}
    }
}
