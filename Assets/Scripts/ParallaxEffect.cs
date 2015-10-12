using UnityEngine;
using System.Collections;
using Jolly;

public class ParallaxEffect : MonoBehaviour
{
	public Camera CameraToFollow;
	public float ParallaxFactor;

	private Vector3 OriginalPosition;

	void Start ()
	{
		this.OriginalPosition = this.transform.position;
	}

	private Vector3 LerpToParallaxFactorUnclamped (Vector3 from, Vector3 to)
	{
		float value = this.ParallaxFactor;
		return (1.0f - value)*from + value*to;
	}

	void Update ()
	{
		this.transform.position = this.LerpToParallaxFactorUnclamped (this.OriginalPosition, this.transform.position.SetX(this.CameraToFollow.transform.position.x));
	}
}
