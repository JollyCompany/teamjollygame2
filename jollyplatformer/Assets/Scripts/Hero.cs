using UnityEngine;
using System;
using System.Collections;
using Jolly;

public class Hero : MonoBehaviour
{
	public float StompForce;
	public float ScaleAdjustment;
	public int ScaleIterations;
	public Vector2 HUDPosition
	{
		get
		{
			switch (this.PlayerIndex)
			{
			case 1: return new Vector2 (15, 35);
			case 2: return new Vector2 (495, 35);
			case 3: return new Vector2 (975, 35);
			case 4: return new Vector2 (1455, 35);
			}
			return Vector2.zero;
		}
	}
	public GameObject GroundDetector;
	public GameObject ProjectileEmitLocator;
	public GameObject ChannelLocator;
	public GameObject CounterLocator;
	public GameObject Projectile;
	public GameObject StunVisual;
	public GameObject ChannelVisual;
	public GameObject MaxGrowthVisual;
	public float ChannelTime;
	public float RespawnTime;
	public float StunTime;
	public int PlayerIndex
	{
		get
		{
			return 1+this.HeroController.PlayerNumber;
		}
	}
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
	private float TimeLeftStunned = 0.0f;
	private float TimeSpentChanneling = 0.0f;
	private bool IsChanneling = false;
	private GameObject ChannelVisualInstance;
	private GameObject MaxVisualInstance;
	private GameObject StunVisualInstance;
	private bool CanDoubleJump;
	private bool GroundedLastFrame;
	private float StartScale;

	public Sprite[] BodySprites;
	public Sprite[] ProjectileSprites;
	public Sprite ProjectileSprite;

	void Start ()
	{
		this.HeroController = this.GetComponent<HeroController>();
		JollyDebug.Log ("Player Number = {0}", this.HeroController.PlayerNumber);
		this.GetComponentInChildren<SpriteRenderer>().sprite = this.BodySprites[this.HeroController.PlayerNumber];
		this.ProjectileSprite = this.ProjectileSprites[this.HeroController.PlayerNumber];
		this.StartScale = this.scale;

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
			string displayString = string.Format("Back in {0}s!", ((int)Math.Ceiling(this.RespawnTimeLeft)).ToString());
			this.DrawOutlineText(new Rect((position.x + iconSizeWidth * 1.25f) / 1920.0f * Screen.width, 0, Screen.width, Screen.height), displayString, style, Color.black, Color.white, 1);
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

		bool canMove = !this.IsChanneling && !this.Stomping;
		bool canAct = !this.IsChanneling && !this.Stomping;

		if (this.grounded)
		{
			this.CanDoubleJump = true;
		}

		if (canMove)
		{
			this.velocity = new Vector2 (this.HeroController.HorizontalMovementAxis * this.MaxNewSpeed, this.velocity.y);
		}
		else
		{
			this.velocity = new Vector2 (0.0f, this.velocity.y);
		}

		if (canAct)
		{
			if (this.HeroController.Jump)
			{
				if (this.grounded || this.CanDoubleJump)
				{
					bool doubleJumped = false;

					if (!this.grounded)
					{
						this.CanDoubleJump = false;
						doubleJumped = true;
					}
					this.velocity = new Vector2 (this.velocity.x, this.Jump);

					if (doubleJumped)
					{
						SoundFX.Instance.OnHeroDoubleJumped(this);
					}
					else
					{
						SoundFX.Instance.OnHeroJumped(this);
					}
				}
			}
		}

		{
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
					this.velocity = new Vector2 (0.0f, this.velocity.y);
				}
			}
		}
	}

	void OldUpdate ()
	{

		bool grounded = true;
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
	}

	public float StaticMargin = 0.2f;
	public float FallingMargin = 0.5f;
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
	private bool wasGrounded = false;
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
			Vector3 startPoint = new Vector3(this.box.xMin + this.StaticMargin, this.box.yMin + this.StaticMargin, this.transform.position.z);
			Vector3 endPoint   = new Vector3(this.box.xMax - this.StaticMargin, startPoint.y, startPoint.z);

			float distance = this.StaticMargin + (this.grounded ? this.StaticMargin : Mathf.Abs (this.velocity.y * Time.fixedDeltaTime));

			for (int i = 0; i < this.VerticalRays; ++i)
			{
				Vector2 origin = Vector2.Lerp (startPoint, endPoint, (float)i / (float)(VerticalRays - 1));

				//raycastHit = Physics2D.Linecast(this.transform.position, this.GroundDetector.transform.position, 1 << this.groundMask);
				raycastHit = Physics2D.Linecast(origin, origin - new Vector2(0.0f, distance), 1 << this.groundMask);

				if (raycastHit.collider != null)
				{
					hitSomething = true;
					if (!grounded)
					{
						SoundFX.Instance.OnHeroLanded(this);
					}
					grounded = true;
					if (falling)
					{
						this.transform.Translate (Vector3.down * (raycastHit.distance - this.StaticMargin));
					}
					falling = false;
					velocity = new Vector2 (velocity.x, Mathf.Max (0.0f, velocity.y));
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

	/*
	void OldFixedUpdate ()
	{

		if (this.TimeLeftStunned > 0.0f)
		{
			this.TimeLeftStunned -= Time.deltaTime;

			if (this.TimeLeftStunned <= 0.0f)
			{
				this.StopStun();
			}
		}

		if (this.HeroController.GetBiggerEnd)

		bool canMove = !this.IsChanneling && !this.Stomping && !this.IsStunned();
		bool canAct = !this.IsChanneling && !this.Stomping && !this.IsStunned();

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
						else
						{
							SoundFX.Instance.OnHeroStompLandStun(this);
							hero.Stun(this);
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

			if (this.ShouldStomp)
			{
				this.Stomping = true;

				Rigidbody2D rigidBody = this.GetComponent<Rigidbody2D>();
				rigidBody.velocity = new Vector2(0, 0);
				this.GetComponent<Rigidbody2D>().AddForce (-Vector2.up * StompForce * 1/this.scale);
				this.ShouldStomp = false;
				SoundFX.Instance.OnHeroStompStart(this);
			}

		}

	}
*/

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
		if (this == attackingHero || !this.IsAlive())
		{
			return;
		}

		JollyDebug.Log ("GETING HIT");

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

	bool IsStunned()
	{
		return this.TimeLeftStunned > 0.0f;
	}

	void Stun(Hero attackingHero)
	{
		this.TimeLeftStunned = this.StunTime;

		if (this.StunVisualInstance == null)
		{
			this.StunVisualInstance = (GameObject)GameObject.Instantiate(this.StunVisual, this.ChannelLocator.transform.position, Quaternion.identity);
			this.StunVisualInstance.GetComponent<StunVisual>().Hero = this;
			this.StunVisualInstance.transform.localScale = new Vector3(this.StunVisualInstance.transform.localScale.x * this.scale, this.StunVisualInstance.transform.localScale.y * this.scale, this.StunVisualInstance.transform.localScale.z * this.scale);
			this.StunVisualInstance.transform.parent = this.transform;
		}
	}

	void StopStun()
	{
		this.TimeLeftStunned = 0.0f;

		if (this.StunVisualInstance)
		{
			Destroy(this.StunVisualInstance);
		}
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
		return this.IsAlive() && this.GetGrowStage() < this.ScaleIterations && this.grounded && !this.IsStunned ();
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
		this.scale = (this.ScaleAdjustment * growStage * this.StartScale) + this.StartScale;
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		rb.mass = (this.StartScale / this.scale);
	}

	public int GetGrowStage()
	{
		return (int)((this.scale - this.StartScale) / (ScaleAdjustment * this.StartScale));
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
