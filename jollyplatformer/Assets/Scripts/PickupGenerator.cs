using UnityEngine;
using System.Collections;
using Jolly;

public class PickupGenerator : MonoBehaviour
{
	public GameObject[] Pickups;
	private Vector2[] SpawnPoints;

	public float TimeBetween;
	public int MaxSimultaneous;
	private float timeUntilNext;
	public float TimeToLive;
	public float TimeToBlink;

	void Start ()
	{
		this.timeUntilNext = this.TimeBetween;
		var t = this.gameObject.transform;
		this.SpawnPoints = new Vector2[t.childCount];
		for (int i = 0; i < t.childCount; ++i)
		{
			var child = t.GetChild (i);
			this.SpawnPoints[i] = new Vector2 (child.transform.position.x, child.transform.position.y);
		}
	}

	private GameObject randomPickup
	{
		get
		{
			return RandomExt.Pick (this.Pickups);
		}
	}

	void FixedUpdate ()
	{
		GameObject[] activePickups = GameObject.FindGameObjectsWithTag ("Pickup");
		if (activePickups.Length < this.MaxSimultaneous)
		{
			this.timeUntilNext -= Time.fixedDeltaTime;
			if (this.timeUntilNext < 0)
			{
				this.timeUntilNext = this.TimeBetween;
				var go = GameObject.Instantiate (this.randomPickup, RandomExt.Pick (this.SpawnPoints), Quaternion.identity) as GameObject;
				var pickup = go.GetComponent<Pickup>();
				pickup.ExpirationTime = Time.time + this.TimeToLive;
				pickup.StartBlinkTime = pickup.ExpirationTime - this.TimeToBlink;
			}
		}
	}
}
