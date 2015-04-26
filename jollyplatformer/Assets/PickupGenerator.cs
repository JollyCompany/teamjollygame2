using UnityEngine;
using System.Collections;
using Jolly;

public class PickupGenerator : MonoBehaviour
{
	public GameObject[] Pickups;
	private Vector3[] SpawnPoints;

	public float TimeBetween;
	public int MaxSimultaneous;
	private float TimeUntilNext;
	public float TimeToLive;
	public float TimeToBlink;

	void Start ()
	{
		var t = this.gameObject.transform;
		this.SpawnPoints = new Vector3[t.childCount];
		for (int i = 0; i < t.childCount; ++i)
		{
			var child = t.GetChild (i);
			this.SpawnPoints[i] = child.transform.position;
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
			this.TimeUntilNext -= Time.fixedDeltaTime;
			if (this.TimeUntilNext < 0)
			{
				this.TimeUntilNext = this.TimeBetween;
				var go = GameObject.Instantiate (this.randomPickup, RandomExt.Pick (this.SpawnPoints), Quaternion.identity) as GameObject;
				var pickup = go.GetComponent<Pickup>();
				pickup.ExpirationTime = Time.time + this.TimeToLive;
				pickup.StartBlinkTime = pickup.ExpirationTime - this.TimeToBlink;
			}
		}
	}
}
