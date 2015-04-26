using UnityEngine;
using System.Collections;
using Jolly;

public class DestroyAfter : MonoBehaviour
{
	public float Duration = 5.0f;

	void Update()
	{
		this.Duration -= Time.deltaTime;
		if (this.Duration < 0.0f)
		{
			GameObject.Destroy (this.gameObject);
		}
	}
}
