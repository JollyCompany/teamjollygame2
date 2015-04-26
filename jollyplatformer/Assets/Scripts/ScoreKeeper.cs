using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Jolly;

public class ScoreKeeper : MonoBehaviour
{
	public float TimeToWin;
	public Hero WinningHero;

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

	void Update()
	{
		if (this.WinningHero != null)
		{
			return;
		}

		Hero hero = this.FindWinningPlayer();
		if (hero != null)
		{
			hero.TimeAtMaxSize += Time.deltaTime;
			if (hero.TimeAtMaxSize >= this.TimeToWin)
			{
				this.WinningHero = hero;
			}
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
