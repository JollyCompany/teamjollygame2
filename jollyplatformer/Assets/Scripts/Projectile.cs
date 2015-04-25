using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
	public float Lifetime;
	private float LifetimeRemaining;

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
}
