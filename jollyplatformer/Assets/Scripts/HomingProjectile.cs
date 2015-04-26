using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Jolly;

public class HomingProjectile : MonoBehaviour
{
	public Hero OwnerHero;
	public Hero TargetHero;
	public float Velocity;

	void Start ()
	{
		List<Hero> targets = new List<Hero>();
				Debug.Log("START");

		Hero[] heroes = FindObjectsOfType(typeof(Hero)) as Hero[];
		foreach (Hero hero in heroes)
		{
			if (hero == this.OwnerHero)
			{
				Debug.Log("OUT");
				continue;
			}

			if (!hero.IsAlive())
			{
				Debug.Log("Dead");
				continue;
			}

			targets.Add(hero);
		}
Debug.Log(string.Format("TEST {0}", targets.Count));
		if (targets.Count > 0)
		{
			this.OwnerHero = targets[0];
		}
	}

	void Update ()
	{
	}

	void LateUpdate ()
	{
		if (!this.TargetHero)
		{
			return;
		}

		if (!this.TargetHero.IsAlive())
		{
			Destroy(this);
		}

		Vector3 direction = (this.TargetHero.transform.position - this.transform.position).normalized;
		this.transform.Translate (direction * this.Velocity * Time.deltaTime);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		Hero hero = collision.gameObject.GetComponent<Hero>();
		if (hero == this.OwnerHero)
		{
			return;
		}

		if (hero != null)
		{
			hero.Hit(this.OwnerHero);
			SoundFX.Instance.OnHeroHit(hero);
		}

		Destroy(this.gameObject);
    }
}
