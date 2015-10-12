using UnityEngine;
using System.Collections;
using Jolly;

public class SoundFX : MonoBehaviour
{
	private static SoundFX _instance = null;
	public static SoundFX Instance
	{
		get
		{
			if (null == SoundFX ._instance)
			{
				SoundFX._instance = GameObject.FindObjectOfType<SoundFX>();
				if (null == SoundFX._instance)
				{
					Debug.LogError ("No SoundFX were found in the scene. Create this object in the project's first scene!");
				}
			}

			return SoundFX._instance;
		}
	}

	public AudioClip[] Jump;
	public AudioClip[] DoubleJump;
	public AudioClip[] HitHead;
	public AudioClip[] Land;
	public AudioClip[] GrowChannel;
	public AudioClip[] GrowComplete;
	public AudioClip[] Fire;
	public AudioClip[] HeroHit;
	public AudioClip[] HeroDies;
	public AudioClip[] Respawn;
	public AudioClip[] StompStart;
	public AudioClip[] StompLandStun;
	public AudioClip[] StompLandSquish;
	public AudioClip[] StompLand;
	public AudioClip[] Stunned;
	public AudioClip[] MatchWon;
	public AudioClip[] HeroReachedMaxSize;
	public AudioClip[] HeroAboutToWin;

	public GameObject OnHeroJumped(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.Jump, hero.xyz());
	}

	public GameObject OnHeroDoubleJumped(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.DoubleJump, hero.xyz());
	}

	public GameObject OnHeroHitHead(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.HitHead, hero.xyz());
	}

	public GameObject OnHeroLanded(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.Land, hero.xyz());
	}

	public GameObject OnHeroGrowChannel(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.GrowChannel, hero.xyz());
	}

	public GameObject OnHeroGrowComplete(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.GrowComplete, hero.xyz());
	}

	public GameObject OnHeroFire(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.Fire, hero.xyz());
	}

	public GameObject OnHeroHit(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.HeroHit, hero.xyz());
	}

	public GameObject OnHeroDies(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.HeroDies, hero.xyz());
	}

	public GameObject OnHeroRespawn(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.Respawn, hero.xyz());
	}

	public GameObject OnHeroStompStart(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.StompStart, hero.xyz());
	}

	public GameObject OnHeroStompLandStun(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.StompLandStun, hero.xyz());
	}

	public GameObject OnHeroStompLandSquish(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.StompLandSquish, hero.xyz());
	}

	public GameObject OnHeroStompLand(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.StompLand, hero.xyz());
	}

	public GameObject OnHeroStunned(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.Stunned, hero.xyz());
	}

	public GameObject OnMatchWon(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.MatchWon, hero.xyz());
	}

	public GameObject OnHeroAboutToWin(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.HeroAboutToWin, hero.xyz());
	}

	public GameObject OnHeroReachedMaxSize(Hero hero)
	{
		return AudioSourceExt.PlayRandomClipAtPoint (this.HeroReachedMaxSize, hero.xyz());
	}

	public void StartMusic()
	{
		GameObject backgroundMusic = GameObject.Find("BackgroundMusic");
		AudioSource audioSource = backgroundMusic.GetComponent<AudioSource>();
		audioSource.UnPause();
	}

	public void StopMusic()
	{
		GameObject backgroundMusic = GameObject.Find("BackgroundMusic");
		AudioSource audioSource = backgroundMusic.GetComponent<AudioSource>();
		audioSource.Pause();
	}
}