using UnityEngine;
using System.Collections;
using Jolly;


public class Hero : MonoBehaviour
{
	public float MaxSpeed;
	public float MoveForce;
	public float JumpForce;
	public GameObject GroundDetector;
	public GameObject ScreenEdgeDetector;
	public GameObject ProjectileEmitLocator;
	public GameObject Projectile;
	public Camera RenderingCamera;

	private HeroController HeroController;

	public Vector2 ProjectileLaunchForce;
	public float ProjectileDelay;
	private float TimeUntilNextProjectile = 0.0f;

	private bool ShouldJump = false;
	private bool AtEdgeOfScreen = false;
	private bool FacingRight = true;

	void Start ()
	{
		this.HeroController = this.GetComponent<HeroController>();

		JollyDebug.Watch (this, "FacingRight", delegate ()
		{
			return this.FacingRight;
		});
	}

	void Update ()
	{
		bool grounded = Physics2D.Linecast(this.transform.position, this.GroundDetector.transform.position, 1 << LayerMask.NameToLayer ("Ground"));
		JollyDebug.Watch (this, "Grounded", grounded);
		if (this.HeroController.Jump && grounded)
		{
			this.ShouldJump = true;
		}

		float viewportPointOfEdgeDetector = this.RenderingCamera.WorldToViewportPoint(this.ScreenEdgeDetector.transform.position).x;
		this.AtEdgeOfScreen = viewportPointOfEdgeDetector < 0.0f || viewportPointOfEdgeDetector >= 1.0f;

		this.TimeUntilNextProjectile -= Time.deltaTime;
	}

	void FixedUpdate ()
	{
		float horizontal = this.HeroController.HorizontalMovementAxis;

		bool movingIntoScreenEdge = (horizontal > 0 && this.FacingRight) || (horizontal < 0 && !this.FacingRight);
		if (this.AtEdgeOfScreen && movingIntoScreenEdge)
		{
			this.GetComponent<Rigidbody2D>().velocity = new Vector2(0, this.GetComponent<Rigidbody2D>().velocity.y);
			horizontal = 0.0f;
		}

		if (horizontal * this.GetComponent<Rigidbody2D>().velocity.x < this.MaxSpeed)
		{
			this.GetComponent<Rigidbody2D>().AddForce (Vector2.right * horizontal * MoveForce);
		}

		float maxSpeed = Mathf.Abs (this.MaxSpeed * horizontal);
		if (Mathf.Abs(this.GetComponent<Rigidbody2D>().velocity.x) > maxSpeed)
		{
			this.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign (this.GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, this.GetComponent<Rigidbody2D>().velocity.y);
		}

		if (this.ShouldJump)
		{
			this.GetComponent<Rigidbody2D>().AddForce (Vector2.up * JumpForce);
			this.ShouldJump = false;
		}

		if ((horizontal > 0 && !this.FacingRight) || (horizontal < 0 && this.FacingRight))
		{
			this.Flip();
		}

		if (this.HeroController.Shooting && this.TimeUntilNextProjectile < 0.0f)
		{
			this.TimeUntilNextProjectile = this.ProjectileDelay;
			GameObject projectile = (GameObject)GameObject.Instantiate(this.Projectile, this.ProjectileEmitLocator.transform.position, Quaternion.identity);
			Vector2 launchForce = this.ProjectileLaunchForce;
			if (!this.FacingRight)
			{
				launchForce = new Vector2(launchForce.x * -1.0f, launchForce.y);
			}
			projectile.GetComponent<Rigidbody2D>().AddForce(launchForce);
		}

	}

	void Flip ()
	{
		this.FacingRight = !this.FacingRight;
		this.transform.localScale = this.transform.localScale.SetX(this.FacingRight ? 1.0f : -1.0f);
	}
}
