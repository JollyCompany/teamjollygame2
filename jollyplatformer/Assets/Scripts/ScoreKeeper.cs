using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Jolly;

public class ScoreKeeper : MonoBehaviour
{
	public float TimeToWin;

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

		if (highestSize > 0 && highestPlayers.Count == 1)
		{
			return highestPlayers[0];
		}

		return null;
	}

	void Update()
	{
		Hero hero = this.FindWinningPlayer();
		if (hero != null)
		{
			hero.TimeAtMaxSize += Time.deltaTime;
		}
	}

	void OnGUI()
	{
		int[] scoreLocations = { 242, 702, 1202, 1682 };
		Hero[] heroes = FindObjectsOfType(typeof(Hero)) as Hero[];
		foreach (Hero hero in heroes)
		{
			float textWidth = 100;

			GUIStyle style = new GUIStyle("label");
			style.font = hero.HUDText.font;
			style.fontSize = 20;
			style.alignment = TextAnchor.UpperLeft;

			string displayString = Math.Min(100, (hero.TimeAtMaxSize / this.TimeToWin * 100.0f)).ToString("00.0") + "%";

			this.DrawOutlineText(new Rect(scoreLocations[hero.PlayerIndex - 1] / 1920.0f * Screen.width, 0, textWidth, 40), displayString, style, Color.black, Color.white, 1);
		}
	}
}
