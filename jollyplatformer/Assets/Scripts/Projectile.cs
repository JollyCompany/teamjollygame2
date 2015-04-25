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

		float scale = this.LifetimeRemaining / this.Lifetime;
		this.transform.localScale = new Vector3(scale, scale, scale);

		if (this.LifetimeRemaining < 0.0f)
		{
			Destroy (this.gameObject);
		}
	}
}
