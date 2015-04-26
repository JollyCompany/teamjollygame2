using UnityEngine;
using System;
using System.Collections;
using Jolly;

public class Hero : MonoBehaviour
{
	public float MaxSpeed;
	public float MoveForce;
	public float JumpForce;
	public float StompForce;
	public float ScaleAdjustment;
	public int ScaleIterations;
	public Vector2 HUDPosition;
	public GameObject GroundDetector;
	public GameObject ProjectileEmitLocator;
	public GameObject ChannelLocator;
	public GameObject Projectile;
	public GameObject ChannelVisual;
	public Camera RenderingCamera;
	public float ChannelTime;
	public int PlayerIndex;
	public GUIText HUDText;
	public float TimeAtMaxSize;

	private HeroController HeroController;

	public float ProjectileLaunchVelocity;
	public float ProjectileDelay;
	private float TimeUntilNextProjectile = 0.0f;

	private bool ShouldJump = false;
	private bool ShouldStomp = false;
	private bool FacingRight = true;

	private bool Stomping = false;
	private float RespawnTimeLeft = 0.0f;
	private float TimeSpentChanneling = 0.0f;
	private bool IsChanneling = false;
	private GameObject ChannelVisualInstance;
	private bool CanDoubleJump;
	private bool GroundedLastFrame;

	public Sprite ProjectileSprite;

	void Start ()
	{
		this.HeroController = this.GetComponent<HeroController>();

		JollyDebug.Watch (this, "FacingRight", delegate ()
		{
			return this.FacingRight;
		});

		this.groundMask = LayerMask.NameToLayer ("Ground");
	}

	private float scale
	{
		set
		{
			this.transform.localScale = new Vector3((this.FacingRight ? 1.0f : -1.0f) * value, value, 1.0f);
		}
		get
		{
			return this.transform.localScale.y;
		}
	}

	bool IsGrounded()
	{
		return Physics2D.Linecast(this.transform.position, this.GroundDetector.transform.position, 1 << LayerMask.NameToLayer ("Ground"));;
	}

	void OnGUI()
	{
		this.DrawHUD(this.HUDPosition);
	}

	void DrawHUD(Vector2 position)
	{
		float iconSizeWidth = 50;
		float heartSizeWidth = 35;

		float xPosition = position.x;

		Texture badge = (Texture)Resources.Load(string.Format("p{0}_badge", this.PlayerIndex), typeof(Texture));
		GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - iconSizeWidth * 0.5f) / 1080.0f * Screen.height, iconSizeWidth / 1920.0f * Screen.width, iconSizeWidth / 1920.0f * Screen.width), badge);
		xPosition += (iconSizeWidth * 1.5f);


		Texture heart = (Texture)Resources.Load("heart_full", typeof(Texture));
		GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - heartSizeWidth * 0.5f) / 1080.0f * Screen.height, heartSizeWidth / 1920.0f * Screen.width, heartSizeWidth / 1920.0f * Screen.width), heart);
		xPosition += (heartSizeWidth * 1.1f);

		GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - heartSizeWidth * 0.5f) / 1080.0f * Screen.height, heartSizeWidth / 1920.0f * Screen.width, heartSizeWidth / 1920.0f * Screen.width), heart);
		xPosition += (heartSizeWidth * 1.1f);

		GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - heartSizeWidth * 0.5f) / 1080.0f * Screen.height, heartSizeWidth / 1920.0f * Screen.width, heartSizeWidth / 1920.0f * Screen.width), heart);
		xPosition += (iconSizeWidth * 1.5f);

		float textWidth = 100;

		GUIStyle style = new GUIStyle("label");
		style.font = this.HUDText.font;
		style.fontSize = 20;
		style.alignment = TextAnchor.UpperLeft;

		if (this.RespawnTimeLeft > 0)
		{
			string displayString = ((int)Math.Ceiling(this.RespawnTimeLeft)).ToString();
			this.DrawOutlineText(new Rect((position.x + iconSizeWidth * 0.25f) / 1920.0f * Screen.width, 0, textWidth, 40), displayString, style, Color.black, Color.white, 1);
		}
	}

	void Update ()
	{
		if (this.grounded)
		{
			this.CanDoubleJump = true;
		}

		if (this.HeroController.Jump)
		{
			bool doubleJumped = false;

			if (this.grounded || this.CanDoubleJump)
			{
				if (!this.grounded)
				{
					this.CanDoubleJump = false;
				}
				this.velocity = new Vector2 (this.velocity.x, this.Jump);
			}

			if (doubleJumped)
			{
				SoundFX.Instance.OnHeroDoubleJumped(this);
			}
			else
			{
				SoundFX.Instance.OnHeroJumped(this);
			}
		}


		this.velocity = new Vector2 (this.HeroController.HorizontalMovementAxis * this.MaxNewSpeed, this.velocity.y);
	}

	void OldUpdate ()
	{

		if (this.RespawnTimeLeft > 0.0f)
		{
			this.transform.position = new Vector3(0.0f, -20.0f, 0.0f);

			this.RespawnTimeLeft -= Time.deltaTime;
			if (this.RespawnTimeLeft <= 0.0f)
			{
				this.transform.position = new Vector3(0,0,0);
			SoundFX.Instance.OnHeroRespawn(this);
			}
		}

		bool grounded = this.IsGrounded();
		bool justLanded = (grounded && !this.GroundedLastFrame);
		this.GroundedLastFrame = grounded;
		JollyDebug.Watch (this, "Grounded", grounded);
		bool canJump = ((grounded || this.CanDoubleJump) && !this.Stomping);

		if (justLanded)
		{
			if (this.Stomping)
			{
				SoundFX.Instance.OnHeroStompLand(this);
			}
			else
			{
				SoundFX.Instance.OnHeroLanded(this);
			}
		}

		if (this.HeroController.Jump && canJump)
		{
			this.ShouldJump = true;
		}

		if (grounded)
		{
			this.CanDoubleJump = true;
			this.Stomping = false;
		}

		JollyDebug.Watch (this, "Stomping", this.Stomping);

		if (this.HeroController.Stomping && !grounded)
		{
			this.ShouldStomp = true;
		}

		this.TimeUntilNextProjectile -= Time.deltaTime;

		if (this.HeroController.GetBiggerEnd)
		{
			this.StopChannelGrow();
		}
		else if (this.HeroController.GetBiggerHold)
		{
			if (this.IsChanneling)
			{
				this.TimeSpentChanneling += Time.deltaTime;

				if (this.TimeSpentChanneling > this.ChannelTime)
				{
					this.StopChannelGrow();
					this.Grow();
				}
			}
			else if (this.CanGrow ())
			{
				this.StartChannelGrow();
			}
		}
	}

	public float Margin = 0.2f;
	public float Gravity = 6.0f;
	public float MaxFall = 200.0f;
	public float Jump = 200.0f;
	public float Acceleration = 4.0f;
	public float MaxNewSpeed = 150.0f;
	public int VerticalRays;

	private Rect box;
	private Vector2 velocity = Vector2.zero;
	private bool falling = false;
	private bool grounded = false;
	private int groundMask;

	void FixedUpdate ()
	{
		var bounds = this.GetComponent<Collider2D>().bounds;
		this.box = Rect.MinMaxRect (bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);

		if (!this.grounded)
		{
			this.velocity = new Vector2(this.velocity.x, Mathf.Max (this.velocity.y - this.Gravity, -this.MaxFall));
		}

		this.falling = this.velocity.y < 0;

		bool hitSomething = false;
		RaycastHit2D raycastHit;
		if (grounded || falling)
		{
			Vector3 startPoint = new Vector3(this.box.xMin + this.Margin, this.box.center.y, this.transform.position.z);
			Vector3 endPoint   = new Vector3(this.box.xMax - this.Margin, this.box.center.y, this.transform.position.z);

			float distance = this.box.height / 2 + (this.grounded ? this.Margin : Mathf.Abs (this.velocity.y * Time.fixedDeltaTime));

			for (int i = 0; i < this.VerticalRays; ++i)
			{
				Vector2 origin = Vector2.Lerp (startPoint, endPoint, (float)i / (float)(VerticalRays - 1));

				raycastHit = Physics2D.Linecast(this.transform.position, this.GroundDetector.transform.position, 1 << this.groundMask);

				if (raycastHit.collider != null)
				{
					hitSomething = true;
					grounded = true;
					falling = false;
					this.transform.Translate (Vector3.down * (raycastHit.distance - this.box.height/2));
					velocity = new Vector2 (velocity.x, 0);
					break;
				}
			}
		}

		if (!hitSomething)
		{
			grounded = false;
		}

		if ((this.velocity.x > 0 && !this.FacingRight) || (this.velocity.x < 0 && this.FacingRight))
		{
			this.Flip();
		}

		this.TimeUntilNextProjectile -= Time.fixedDeltaTime;

		bool canAct = true;
		if (canAct)
		{
			JollyDebug.Watch (this, "TimeUntilNextProjectile", this.TimeUntilNextProjectile);
			if (this.HeroController.Shooting && this.TimeUntilNextProjectile < 0.0f)
			{
				this.TimeUntilNextProjectile = this.ProjectileDelay;
				GameObject projectile = (GameObject)GameObject.Instantiate(this.Projectile, this.ProjectileEmitLocator.transform.position, Quaternion.identity);
				projectile.GetComponent<SpriteRenderer>().sprite = this.ProjectileSprite;
				projectile.GetComponent<Projectile>().OwnerHero = this;
				projectile.transform.localScale = this.transform.localScale;
				float launchVelocity = (this.FacingRight ? 1.0f : -1.0f) * this.ProjectileLaunchVelocity;
				projectile.GetComponent<Projectile>().Velocity = new Vector2(launchVelocity, 0.0f);
				SoundFX.Instance.OnHeroFire(this);
			}
		}
	}


	void LateUpdate ()
	{
		this.transform.Translate (this.velocity * Time.deltaTime);
	}

	void OldFixedUpdate ()
	{


		bool canMove = !this.IsChanneling && !this.Stomping;
		bool canAct = !this.IsChanneling && !this.Stomping;

		JollyDebug.Watch (this, "CanMove", canMove);
		JollyDebug.Watch (this, "CanAct", canAct);

		float horizontal = this.HeroController.HorizontalMovementAxis;
		if (!canMove)
		{
			horizontal = 0;
		}

		var v = this.GetComponent<Rigidbody2D>().velocity;
		this.GetComponent<Rigidbody2D>().velocity = new Vector2 (horizontal * this.MaxSpeed, v.y);

		float maxSpeed = Mathf.Abs (this.MaxSpeed * horizontal);
		if (Mathf.Abs(this.GetComponent<Rigidbody2D>().velocity.x) > maxSpeed)
		{
			this.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign (this.GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, this.GetComponent<Rigidbody2D>().velocity.y);
		}

		if (this.Stomping)
		{
			Vector3 direction = this.GroundDetector.transform.position - this.transform.position;
			RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position, direction, direction.magnitude);
			Debug.Log(string.Format("Hits {0}", hits.Length));
			if (hits.Length > 0)
			{
				for (int i = 0; i < hits.Length; ++i)
				{
					Hero hero = hits[i].collider.gameObject.GetComponent<Hero>();
					if (hero && hero != this)
					{
						if (this.GetGrowStage() > hero.GetGrowStage())
						{
							SoundFX.Instance.OnHeroStompLandSquish(this);
							hero.Die(this);
						}
					}
				}
			}

			Rigidbody2D rigidBody = this.GetComponent<Rigidbody2D>();
			if (rigidBody.velocity.y > -0.5f)
			{
				this.Stomping = false;
			}
		}

		if (canAct)
		{
			if (this.ShouldJump)
			{
				bool doubleJumped = false;
				if (!this.IsGrounded())
				{
					this.CanDoubleJump = false;
					doubleJumped = true;
				}

				Rigidbody2D rigidBody = this.GetComponent<Rigidbody2D>();
				rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
				this.GetComponent<Rigidbody2D>().AddForce (Vector2.up * JumpForce * 1/this.scale);
				this.ShouldJump = false;

				if (doubleJumped)
				{
					SoundFX.Instance.OnHeroDoubleJumped(this);
				}
				else
				{
					SoundFX.Instance.OnHeroJumped(this);
				}
			}

			if (this.ShouldStomp)
			{
				this.Stomping = true;

				Rigidbody2D rigidBody = this.GetComponent<Rigidbody2D>();
				rigidBody.velocity = new Vector2(0, 0);
				this.GetComponent<Rigidbody2D>().AddForce (-Vector2.up * StompForce * 1/this.scale);
				this.ShouldStomp = false;
				SoundFX.Instance.OnHeroStompStart(this);
			}

			if ((horizontal > 0 && !this.FacingRight) || (horizontal < 0 && this.FacingRight))
			{
				this.Flip();
			}

		}

	}

	void Flip ()
	{
		this.FacingRight = !this.FacingRight;
		this.scale = this.scale;
	}

	public bool IsAlive()
	{
		return (this.RespawnTimeLeft <= 0.0f);
	}

	public void Hit (Hero attackingHero)
	{
		if (this == attackingHero)
		{
			return;
		}

		this.Die(attackingHero);
	}

	void Die (Hero attackingHero)
	{
		SoundFX.Instance.OnHeroDies(this);
		this.RespawnTimeLeft = 5.0f;
		this.SetGrowStage(0);
		this.StopChannelGrow();
		this.Stomping = false;
		this.ShouldStomp = false;
		this.ShouldJump = false;
	}

	void StartChannelGrow()
	{
		this.TimeSpentChanneling = 0.0f;
		this.IsChanneling = true;
		this.ChannelVisualInstance = (GameObject)GameObject.Instantiate(this.ChannelVisual, this.ChannelLocator.transform.position, Quaternion.identity);
		this.ChannelVisualInstance.GetComponent<ChannelVisual>().ChannelTime = this.ChannelTime;
		this.ChannelVisualInstance.GetComponent<ChannelVisual>().Hero = this;
		this.ChannelVisualInstance.transform.localScale = new Vector3(this.ChannelVisualInstance.transform.localScale.x * this.scale, this.ChannelVisualInstance.transform.localScale.y * this.scale, this.ChannelVisualInstance.transform.localScale.z * this.scale);
	}

	void StopChannelGrow()
	{
		this.TimeSpentChanneling = 0.0f;
		this.IsChanneling = false;
		Destroy(this.ChannelVisualInstance);
	}

	bool CanGrow()
	{
		return this.IsAlive() && this.GetGrowStage() < this.ScaleIterations && this.IsGrounded();
	}

	void Grow()
	{
		if (this.CanGrow())
		{
			SetGrowStage(this.GetGrowStage() + 1);
			SoundFX.Instance.OnHeroGrowComplete(this);
		}
	}

	void SetGrowStage(int growStage)
	{

		this.scale = (this.ScaleAdjustment * growStage) + 1.0f;
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		rb.mass = (1.0f / this.scale);
	}

	public int GetGrowStage()
	{
		return (int)((this.scale - 1.0f) / ScaleAdjustment);
	}

	/*
	void OnCollisionEnter2D(Collision2D coll)
	{
		Debug.Log ("Collision Enter 2D " + coll.collider.bounds.ToString() + " " + Random.value);
	}

	void OnCollisionExit2D(Collision2D coll)
	{
		Debug.Log ("Collision Exit 2D " + coll.collider.bounds.ToString() + " " + Random.value);
	}*/

	void OnTriggerEnter2D(Collider2D other)
	{
		this.gameObject.layer = LayerMask.NameToLayer ("IgnorePlatforms");
	}

	void OnTriggerExit2D(Collider2D other)
	{
		this.gameObject.layer = LayerMask.NameToLayer ("Default");
	}
}
