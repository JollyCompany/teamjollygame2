using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Jolly;

public class ScoreKeeper : MonoBehaviour
{
	public float TimeToWin;
	public Hero WinningHero;
	public Hero HeroAboutToWin;
	public GameObject HeroAboutToWinSound;
	public GameObject GameWonSound;

	Hero FindWinningPlayer()
	{
		int highestSize = 0;
		List<Hero> highestPlayers = new List<Hero>();

		Hero[] heroes = FindObjectsOfType(typeof(Hero)) as Hero[];
		foreach (Hero hero in heroes)
		{
			if (!hero.IsAlive())
			{
				continue;
			}

			int growStage = hero.GetGrowStage();

			if (growStage > highestSize)
			{
				highestPlayers = new List<Hero>();
				highestPlayers.Add(hero);
				highestSize = growStage;
			}
			else if (growStage > 0 && growStage == highestSize)
			{
				highestPlayers.Add(hero);
			}
		}

		if (highestPlayers.Count == 1)
		{
			if (((Hero)highestPlayers[0]).ScaleIterations == highestSize)
			{
				return highestPlayers[0];
			}
		}

		return null;
	}

	void StartHeroAboutToWinSound(Hero hero)
	{
		SoundFX.Instance.StopMusic();
		this.HeroAboutToWinSound = SoundFX.Instance.OnHeroAboutToWin(hero);
	}

	void StopHeroAboutToWinSound()
	{
		SoundFX.Instance.StartMusic();
		AudioSourceExt.StopClipOnObject(this.HeroAboutToWinSound);
		Destroy(this.HeroAboutToWinSound);
	}

	void PlayVictorySound(Hero hero)
	{
		this.GameWonSound = SoundFX.Instance.OnMatchWon(hero);
		SoundFX.Instance.StopMusic();
	}

	void StopVictorySound()
	{
		AudioSourceExt.StopClipOnObject(this.GameWonSound);
		Destroy(this.GameWonSound);
	}

	void Update()
	{
		if (this.WinningHero != null)
		{
			return;
		}

		Hero hero = this.FindWinningPlayer();
		if (hero != null)
		{
			if (this.HeroAboutToWin != hero)
			{
				this.StopHeroAboutToWinSound();
				this.StartHeroAboutToWinSound(hero);
			}

			this.HeroAboutToWin = hero;
			hero.TimeAtMaxSize += Time.deltaTime;
			if (hero.TimeAtMaxSize >= this.TimeToWin)
			{
				this.WinningHero = hero;
				this.StopHeroAboutToWinSound();
				this.PlayVictorySound(this.WinningHero);
			}
		}
		else
		{
			this.StopHeroAboutToWinSound();
		}
	}

	public void ResetGame()
	{
		this.StopHeroAboutToWinSound();
		this.StopVictorySound();
		SoundFX.Instance.StartMusic();
		this.WinningHero = null;
		this.HeroAboutToWin = null;

		Hero[] heroes = FindObjectsOfType(typeof(Hero)) as Hero[];
		foreach (Hero hero in heroes)
		{
			hero.Reset();
		}
	}

	void OnGUI()
	{
		GUIStyle style = new GUIStyle("label");

		if (this.WinningHero != null)
		{
			string[] names = { "GREEN", "BLUE", "RED", "YELLOW" };

			style.font = this.WinningHero.HUDText.font;
			style.fontSize = (int)(Screen.width * 0.1f);
			style.alignment = TextAnchor.MiddleCenter;
			string winningText = string.Format("{0} PLAYER WINS!", names[this.WinningHero.PlayerIndex - 1]);
			this.DrawOutlineText(new Rect(0, 0, Screen.width, Screen.height), winningText, style, Color.black, Color.white, 1);
		}

		Hero hero = this.FindWinningPlayer();
		if (hero == null)
		{
			return;
		}

		style.font = hero.HUDText.font;
		style.fontSize = (int)(Screen.width * 0.035);
		style.alignment = TextAnchor.UpperCenter;
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(hero.CounterLocator.transform.position);

		int timeLeft = (int)Math.Ceiling(this.TimeToWin - hero.TimeAtMaxSize);
		if (timeLeft != 0)
		{
			string text = timeLeft.ToString();
			hero.DrawOutlineText(new Rect(screenPosition.x - 50, Screen.height - screenPosition.y, 100, Screen.height), text, style, Color.black, Color.white, 1);
		}
	}
}
