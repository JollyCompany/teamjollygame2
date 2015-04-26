using UnityEngine;
using System.Collections;
using Jolly;

public class ShieldBuff : MonoBehaviour {

	public GameObject EffectRenderer;
	private GameObject effectInstance;

	void Start ()
	{
		this.enabled = false;
	}

	void OnEnable ()
	{
		this.EffectRenderer.GetComponent<SpriteRenderer>().enabled = true;
	}

	void OnDisable ()
	{
		this.EffectRenderer.GetComponent<SpriteRenderer>().enabled = false;
	}

	public static void AddToHero (Hero hero)
	{
		var sb = hero.GetComponent <ShieldBuff> ();
		sb.enabled = true;
	}
}
