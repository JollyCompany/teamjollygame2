using UnityEngine;
using System.Collections;

public class PopAndLandOnColliderBelow : MonoBehaviour
{
	public float InitialSpeed;
	public float Gravity;
	public float MaxFall;

	private Vector2 velocity;
	private float endY;

	void Start ()
	{
		this.velocity = new Vector2 (0.0f, this.InitialSpeed);
		RaycastHit2D raycast = Physics2D.Linecast (this.transform.position, this.transform.position - new Vector3(0.0f, 100.0f), (1 << LayerMask.NameToLayer ("Ground")));
		this.endY = this.transform.position.y - raycast.distance + this.GetComponent<Collider2D>().bounds.size.y;
	}
	
	void FixedUpdate ()
	{
		this.velocity = new Vector2(this.velocity.x, Mathf.Max (this.velocity.y - this.Gravity, -this.MaxFall));
		this.transform.Translate (this.velocity * Time.fixedDeltaTime);
		if (this.transform.position.y < this.endY)
		{
			this.transform.position = new Vector3 (this.transform.position.x, this.endY);
			GameObject.Destroy (this);
		}
	}
}
