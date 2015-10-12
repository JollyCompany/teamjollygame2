using UnityEngine;
using System.Collections;

public class WrapProjectilesAroundWorld : MonoBehaviour
{
	public GameObject LeftEdge;
	public GameObject RightEdge;
	public float Border;


	void Start ()
	{
	}

	void Update ()
	{
		var left = Camera.main.ViewportToWorldPoint (new Vector3 (0.0f, 0.5f));
		this.LeftEdge.transform.position = new Vector3(left.x, 0.0f);

		var right = Camera.main.ViewportToWorldPoint (new Vector3 (1.0f, 0.5f));
		this.RightEdge.transform.position = new Vector3(right.x, 0.0f);

   		Projectile[] projectiles = FindObjectsOfType(typeof(Projectile)) as Projectile[];
        foreach (Projectile projectile in projectiles)
        {
			var p = projectile.gameObject.transform.position;
			var lx = this.LeftEdge.transform.position.x;
			var rx = this.RightEdge.transform.position.x;
			if (p.x < lx)
			{
				projectile.gameObject.transform.position = new Vector3 (rx - this.Border, p.y, p.z);
			}
			else if (p.x > rx)
			{
				projectile.gameObject.transform.position = new Vector3 (lx + this.Border, p.y, p.z);
			}
		}

	}
}
