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
	public GameObject ScreenEdgeDetector;
	public GameObject ProjectileEmitLocator;
	public GameObject ChannelLocator;
	public GameObject CounterLocator;
	public GameObject Projectile;
	public GameObject ChannelVisual;
	public GameObject MaxGrowthVisual;
	public Camera RenderingCamera;
	public float ChannelTime;
	public float RespawnTime;
	public int PlayerIndex;
	public GUIText HUDText;
	public float TimeAtMaxSize;

	private HeroController HeroController;

	public Vector2 ProjectileLaunchForce;
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
	private GameObject MaxVisualInstance;
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

		float textWidth = 100;

		float xPosition = position.x;

		Texture badge = (Texture)Resources.Load(string.Format("p{0}_badge", this.PlayerIndex), typeof(Texture));
		GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - iconSizeWidth * 0.5f) / 1080.0f * Screen.height, iconSizeWidth / 1920.0f * Screen.width, iconSizeWidth / 1920.0f * Screen.width), badge);
		xPosition += (iconSizeWidth * 1.5f);

		bool drawHearts = false;
		if (drawHearts)
		{
			Texture heart = (Texture)Resources.Load("heart_full", typeof(Texture));
			GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - heartSizeWidth * 0.5f) / 1080.0f * Screen.height, heartSizeWidth / 1920.0f * Screen.width, heartSizeWidth / 1920.0f * Screen.width), heart);
			xPosition += (heartSizeWidth * 1.1f);

			GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - heartSizeWidth * 0.5f) / 1080.0f * Screen.height, heartSizeWidth / 1920.0f * Screen.width, heartSizeWidth / 1920.0f * Screen.width), heart);
			xPosition += (heartSizeWidth * 1.1f);

			GUI.DrawTexture(new Rect(xPosition / 1920.0f * Screen.width, (position.y - heartSizeWidth * 0.5f) / 1080.0f * Screen.height, heartSizeWidth / 1920.0f * Screen.width, heartSizeWidth / 1920.0f * Screen.width), heart);
			xPosition += (iconSizeWidth * 1.5f);
		}

		GUIStyle style = new GUIStyle("label");
		style.font = this.HUDText.font;
		style.fontSize = (int)(Screen.width * 0.027027f);
		style.alignment = TextAnchor.UpperLeft;

		if (this.RespawnTimeLeft > 0)
		{
			string displayString = ((int)Math.Ceiling(this.RespawnTimeLeft)).ToString();
			this.DrawOutlineText(new Rect((position.x + iconSizeWidth * 1.25f) / 1920.0f * Screen.width, 0, textWidth, 40), displayString, style, Color.black, Color.white, 1);
		}
	}

	void Update ()
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

	void FixedUpdate ()
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

			if (this.HeroController.Shooting && this.TimeUntilNextProjectile < 0.0f)
			{
				this.TimeUntilNextProjectile = this.ProjectileDelay;
				GameObject projectile = (GameObject)GameObject.Instantiate(this.Projectile, this.ProjectileEmitLocator.transform.position, Quaternion.identity);
				projectile.GetComponent<SpriteRenderer>().sprite = this.ProjectileSprite;
				projectile.GetComponent<Projectile>().OwnerHero = this;
				projectile.transform.localScale = this.transform.localScale;
				Vector2 launchForce = this.ProjectileLaunchForce;
				if (!this.FacingRight)
				{
					launchForce = new Vector2(launchForce.x * -1.0f, launchForce.y);
				}
				projectile.GetComponent<Rigidbody2D>().AddForce(launchForce);

				SoundFX.Instance.OnHeroFire(this);
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

	void Die(Hero attackingHero)
	{
		SoundFX.Instance.OnHeroDies(this);
		this.RespawnTimeLeft = this.RespawnTime;
		this.SetGrowStage(0);
		this.StopChannelGrow();
		this.Stomping = false;
		this.ShouldStomp = false;
		this.ShouldJump = false;

		this.TimeAtMaxSize = 0;
		this.RemoveMaxSizeVisual();
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

		if (this.ChannelVisualInstance)
		{
			this.ChannelVisualInstance.GetComponent<ChannelVisual>().Stop();
			Destroy(this.ChannelVisualInstance);
		}
	}

	void AddMaxSizeVisual()
	{
		if (this.MaxGrowthVisual == null)
		{
			return;
		}

		this.MaxVisualInstance = (GameObject)GameObject.Instantiate(this.MaxGrowthVisual, this.ChannelLocator.transform.position, Quaternion.identity);
		this.MaxVisualInstance.transform.localScale = new Vector3(this.MaxVisualInstance.transform.localScale.x * this.scale, this.MaxVisualInstance.transform.localScale.y * this.scale, this.MaxVisualInstance.transform.localScale.z * this.scale);
		this.MaxVisualInstance.transform.parent = this.transform;
	}

	void RemoveMaxSizeVisual()
	{
		Destroy(this.MaxVisualInstance);
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

			if (this.GetGrowStage() == this.ScaleIterations)
			{
				this.AddMaxSizeVisual();
			}
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
