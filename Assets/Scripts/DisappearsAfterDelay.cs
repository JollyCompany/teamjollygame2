using UnityEngine;
using System.Collections;

public class DisappearsAfterDelay : MonoBehaviour
{
	public void OnCollisionEnter2D (Collision2D c)
	{
		StartCoroutine ("FadeCoroutine");
	}

	public IEnumerator FadeCoroutine ()
	{
		bool on = true;
		for (int i = 0; i < 6; ++i)
		{
			on = !on;
			this.GetComponent<SpriteRenderer>().color = on ? Color.white : new Color (1.0f, 1.0f, 1.0f, 0.5f);
			yield return new WaitForSeconds (0.25f);
		}
		GameObject.Destroy (this.gameObject);
	}
}
