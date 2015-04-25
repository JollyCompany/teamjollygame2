using UnityEngine;
using System.Collections;

public class RotateInPlace : MonoBehaviour
{
	public float RotationSpeed;

	void Update ()
	{
		this.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, Time.time * this.RotationSpeed));
	}
}
