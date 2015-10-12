using UnityEngine;
using System;
using System.Collections;
using Jolly;

public class Hero : MonoBehaviour
{
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
	public float SpawnMagnitude;
	public GameObject GroundDetector;
	public GameObject ProjectileEmitLocator;
	public GameObject ChannelLocator;
	public GameObject CounterLocator;
	public GameObject Projectile;
	public GameObject ProjectileExplosion;
	public GameObject StunVisual;
	public GameObject ChannelVisual;
	public GameObject MaxGrowthVisual;
	public bool EnableDoubleJump;
	public float ChannelTime;
	public float RespawnTime;
	public float RespawnTimeIncreasePerDeath;
	public float StunTime;
	public float JumpForgivenessTimeAmount;
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

	private bool FacingRight = true;

	private bool Stomping = false;
	private float RespawnTimeCalculated = 0.0f;
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
	private float StartWidth;
	private float JumpForgivenessTimeLeft;
	private GameObject MaxSizeSound;
	private int NumDeaths;

	public Sprite[] BodySprites;
	public Sprite[] ProjectileSprites;
	public Sprite[] ProjectileExplosions;
	public Sprite ProjectileSprite;
	public Sprite ProjectileExplosionSprite;

	void Start ()
	{
		this.HeroController = this.GetComponent<HeroController>();
		this.GetComponentInChildren<SpriteRenderer>().sprite = this.BodySprites[this.HeroController.PlayerNumber];
		this.ProjectileSprite = this.ProjectileSprites[this.HeroController.PlayerNumber];
		this.ProjectileExplosionSprite = this.ProjectileExplosions[this.HeroController.PlayerNumber];
		this.StartScale = this.scale;
		this.StartWidth = this.GetComponent<Collider2D>().bounds.size.x;
		this.RespawnTimeCalculated = this.RespawnTime;

		this.groundMask = LayerMask.NameToLayer ("Ground");
	}

	private float scale
	{
		set
		{
			float minYOld = this.GetComponent<Collider2D>().bounds.min.y;
			this.transform.localScale = new Vector3((this.FacingRight ? 1.0f : -1.0f) * value, value, 1.0f);
			float minYNew = this.GetComponent<Collider2D>().bounds.min.y;
			Vector3 v = this.transform.position;
			this.transform.position = new Vector3(v.x, v.y + minYOld - minYNew, v.z);
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

	void SetDoubleJumpAllowed()
	{
		if (this.EnableDoubleJump)
		{
			this.CanDoubleJump = true;
		}
	}

	void DrawHUD(Vector2 position)
	{
		float iconSizeWidth = 50;
		float heartSizeWidth = 35;

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

		string displayString = "Flawless!";
		if (this.RespawnTimeLeft > 0)
		{
			displayString = string.Format("Back in {0}s!", ((int)Math.Ceiling(this.RespawnTimeLeft)).ToString());
		}
		else if (this.NumDeaths == 1)
		{
 			displayString = string.Format("{0} Death", 1);
		}
		else if (this.NumDeaths > 0)
		{
			displayString = string.Format("{0} Deaths", this.NumDeaths);
		}

		this.DrawOutlineText(new Rect((position.x + iconSizeWidth * 1.25f) / 1920.0f * Screen.width, 0, Screen.width, Screen.height), displayString, style, Color.black, Color.white, 1);
	}

	bool CanJumpOffGround()
	{
		return (this.grounded || this.JumpForgivenessTimeLeft > 0.0f);
	}

	void Update ()
	{
		if (this.RespawnTimeLeft > 0.0f)
		{
			this.transform.position = new Vector3(0.0f, -20.0f, 0.0f);

			this.RespawnTimeLeft -= Time.deltaTime;
			if (this.RespawnTimeLeft < 0.0)
			{
				this.Respawn ();
			}
		}


		this.JumpForgivenessTimeLeft -= Time.deltaTime;

		bool canAct = !this.IsChanneling && !this.Stomping && !this.IsStunned();
		if (canAct)
		{
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
				Physics2D.IgnoreCollision(projectile.GetComponent<Collider2D>(), this.GetComponent<Collider2D>());
			}


			bool controllerIssuedStomp = (this.HeroController.Jump && !this.CanDoubleJump);
			if (controllerIssuedStomp && !this.CanJumpOffGround() && this.canStomp)
			{
				this.canStomp = false;
				this.Stomping = true;
				this.velocity = new Vector2(0.0f, this.StompSpeed);
				SoundFX.Instance.OnHeroStompStart(this);
			}
		}

		if (this.HeroController.GetResetGame)
		{
			GameObject scoreKeeper = GameObject.Find("ScoreKeeper");
			scoreKeeper.GetComponent<ScoreKeeper>().ResetGame();
		}

		if (this.grounded)
		{
			this.SetDoubleJumpAllowed();
		}

		bool canMove = !this.IsChanneling && !this.Stomping && !this.IsStunned();

		if (canMove)
		{
			this.velocity = new Vector2 (this.HeroController.HorizontalMovementAxis * this.MaxNewSpeed, this.velocity.y);
		}
		else
		{
			this.velocity = new Vector2 (this.velocity.x * (1.0f - Mathf.Clamp01 (Time.deltaTime)), this.velocity.y);
		}

		if (canAct)
		{
			if (this.HeroController.Jump)
			{
				bool isJumpingOffGround = this.CanJumpOffGround();
				if (isJumpingOffGround || this.CanDoubleJump)
				{
					bool doubleJumped = false;

					if (!isJumpingOffGround)
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

		this.canChannelGrow = !this.falling && Physics2D.Linecast(this.transform.position, this.GroundDetector.transform.position, 1 << LayerMask.NameToLayer ("Ground"));

		if (this.canChannelGrow)
		{
			this.canStomp = true;
		}

		if (this.IsChanneling && (this.HeroController.GetBiggerEnd || !this.canChannelGrow))
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
			else if (canAct && this.CanGrow ())
			{
				this.StartChannelGrow();
				this.velocity = new Vector2 (0.0f, this.velocity.y);
			}
		}

	}

	public float StaticMargin = 0.2f;
	public float FallingMargin = 0.5f;
	public float Gravity = 6.0f;
	public float MaxFall = 200.0f;
	public float StompSpeed;
	public float StompGravity = 6.0f;
	public float MaxStompFall;
	public float Jump = 200.0f;
	public float Acceleration = 4.0f;
	public float MaxNewSpeed = 150.0f;
	public float GrowPopSpeed = 1.0f;
	private bool canChannelGrow;

	private Rect box;
	private Vector2 velocity = Vector2.zero;
	private bool falling = true;
	private bool grounded = false;
	private bool canStomp = true;
	private int groundMask;

	void FixedUpdate ()
	{
		var bounds = this.GetComponent<Collider2D>().bounds;
		this.box = Rect.MinMaxRect (bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);

		if (this.TimeLeftStunned > 0.0f)
		{
			this.TimeLeftStunned -= Time.fixedDeltaTime;

			if (this.TimeLeftStunned <= 0.0f)
            {
                this.StopStun();
            }
        }


		if (!this.grounded)
		{
			if (this.Stomping)
			{
				this.velocity = new Vector2(this.velocity.x, Mathf.Max (this.velocity.y - this.StompGravity, -this.MaxStompFall));
			}
			else
			{
				this.velocity = new Vector2(this.velocity.x, Mathf.Max (this.velocity.y - this.Gravity, -this.MaxFall));
			}
		}

		this.falling = this.velocity.y < 0;

		bool hitSomething = false;
		RaycastHit2D raycastHit;
		if (grounded || falling)
		{
			Vector3 startPoint = new Vector3(this.box.xMin + this.StaticMargin, this.box.yMin + this.StaticMargin, this.transform.position.z);
			Vector3 endPoint   = new Vector3(this.box.xMax - this.StaticMargin, startPoint.y, startPoint.z);

            float distance = this.StaticMargin + (this.grounded ? this.StaticMargin : Mathf.Abs (this.velocity.y * this.FallingMargin * Time.fixedDeltaTime));
			int verticalRays = Mathf.Max (3, Mathf.CeilToInt ((endPoint.x - startPoint.x) / this.StartWidth));

			for (int i = 0; i < verticalRays; ++i)
			{
				Vector2 origin = Vector2.Lerp (startPoint, endPoint, (float)i / (float)(verticalRays - 1));

				for (int mask = 0; mask < 2; ++mask)
				{
					if (mask == 0)
					{
						int oldLayer = this.gameObject.layer;
						this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
						raycastHit = Physics2D.Linecast(origin, origin - new Vector2(0.0f, distance), (1<< LayerMask.NameToLayer("Default")));
						this.gameObject.layer = oldLayer;
					}
					else
					{
						raycastHit = Physics2D.Linecast(origin, origin - new Vector2(0.0f, distance), (1 << this.groundMask));
					}


					if (raycastHit.collider != null)
					{
						bool bounce = false;
						hitSomething = true;
						if (!grounded)
						{
							Hero hero = raycastHit.collider.gameObject.GetComponent<Hero>();

							if (Stomping)
							{
								if (null == hero)
								{
									SoundFX.Instance.OnHeroStompLand(this);
								}
								else if (this.GetGrowStage() > hero.GetGrowStage())
								{
									SoundFX.Instance.OnHeroStompLandSquish(this);
									hero.Die(this);
								}
								else
								{
									SoundFX.Instance.OnHeroStompLandStun(this);
									hero.Stun(this);
									bounce = true;
								}
							}
							else
							{
								if (hero)
								{
									bounce = true;
									SoundFX.Instance.OnHeroJumped(this);
								}
								else
								{
									SoundFX.Instance.OnHeroLanded(this);
								}
							}
						}
						Stomping = false;
						this.JumpForgivenessTimeLeft = this.JumpForgivenessTimeAmount;
						grounded = true;
						if (falling)
						{
							this.transform.Translate (Vector3.down * (raycastHit.distance - this.StaticMargin));
						}
						falling = false;
						if (bounce)
						{
							this.CanDoubleJump = false;
							velocity = new Vector2 (velocity.x, this.Jump);
						}
						else
						{
							this.SetDoubleJumpAllowed();
							velocity = new Vector2 (velocity.x, Mathf.Max (0.0f, velocity.y));
						}

						i = verticalRays;
						break;
					}
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

		this.transform.Translate (this.velocity * Time.fixedDeltaTime);
    }


	void LateUpdate ()
	{
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
		if (this == attackingHero || !this.IsAlive())
		{
			return;
		}

		GameObject projectileExplosion = (GameObject)GameObject.Instantiate(attackingHero.ProjectileExplosion, this.transform.position, Quaternion.identity);
		projectileExplosion.GetComponent<SpriteRenderer>().sprite = attackingHero.ProjectileExplosionSprite;
		projectileExplosion.transform.localScale *= attackingHero.scale / attackingHero.StartScale;

		if (this.GetComponent<ShieldBuff>().enabled)
		{
			this.GetComponent<ShieldBuff>().enabled = false;
		}
		else
		{
			this.Die(attackingHero);
		}
	}

	void Die(Hero attackingHero)
	{
		if (!this.IsAlive())
		{
			return;
		}

		AudioSourceExt.StopClipOnObject(this.MaxSizeSound);
		Destroy(this.MaxSizeSound);

		SoundFX.Instance.OnHeroDies(this);
		this.RespawnTimeLeft = this.RespawnTimeCalculated;
		this.RespawnTimeCalculated += this.RespawnTimeIncreasePerDeath;
		this.NumDeaths++;

		this.SetGrowStage(0);
		this.StopChannelGrow();
		this.Stomping = false;

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

	bool CanGrowByPickup()
	{
		return this.IsAlive() && this.GetGrowStage() < this.ScaleIterations;
	}

	public void Grow(bool growByPickup = false)
	{
		if ((growByPickup && this.CanGrowByPickup()) || this.CanGrow())
		{
			SetGrowStage(this.GetGrowStage() + 1);
			if (!growByPickup)
			{
				this.velocity = new Vector2 (0.0f, this.GrowPopSpeed);
			}

			if (this.GetGrowStage() == this.ScaleIterations)
			{
				this.AddMaxSizeVisual();
				this.MaxSizeSound = SoundFX.Instance.OnHeroReachedMaxSize(this);
			}
			else
			{
				SoundFX.Instance.OnHeroGrowComplete(this);
			}
		}
	}

	public void Reset()
	{
		this.Die(null);
		this.Respawn();
		this.transform.localPosition = Vector3.zero;
		this.RespawnTimeCalculated = this.RespawnTime;
		this.NumDeaths = 0;
    }

	void Respawn()
	{
		this.transform.position = new Vector3(0,0,0);

		this.velocity = new Vector2(0.0f, 1.0f) * this.SpawnMagnitude;

		SoundFX.Instance.OnHeroRespawn(this);
		this.RespawnTimeLeft = -1.0f;
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

	void OnTriggerEnter2D(Collider2D other)
	{
		this.gameObject.layer = LayerMask.NameToLayer ("IgnorePlatforms");
	}

	void OnTriggerExit2D(Collider2D other)
	{
		this.gameObject.layer = LayerMask.NameToLayer ("Default");
	}
}
