using UnityEngine;
using System.Collections;

namespace Jolly
{
	public class AudioSourceExt
	{
		public static GameObject PlayClip(AudioClip clip)
		{
			if (clip == null || Camera.main == null)
			{
				return null;
			}
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling(clip, Camera.main.transform.position);
		}

		public static GameObject PlayClip(AudioClip clip, float volume)
		{
			if (clip == null || Camera.main == null)
			{
				return null;
			}
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling(clip, Camera.main.transform.position, volume);
		}

		public static GameObject PlayRandomClip(AudioClip[] clips)
		{
			if (clips == null || Camera.main == null)
			{
				return null;
			}
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling(RandomExt.Pick<AudioClip>(clips), Camera.main.transform.position);
		}

		public static GameObject PlayRandomClip(AudioClip[] clips, float volume)
		{
			if (clips == null || Camera.main == null)
			{
				return null;
			}
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling(RandomExt.Pick<AudioClip>(clips), Camera.main.transform.position, volume);
		}

		public static GameObject PlayRandomClipAtPoint(AudioClip[] clips, Vector3 position)
		{
			if (clips.Length == 0)
			{
				return null;
			}
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling(RandomExt.Pick<AudioClip>(clips), position);
		}

		public static GameObject PlayRandomClipAtPoint(AudioClip[] clips, Vector3 position, float volume)
		{
			if (clips.Length == 0)
			{
				return null;
			}
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling(RandomExt.Pick<AudioClip>(clips), position, volume);
		}

		public static GameObject PlayClipAtPointImmuneToTimeScaling(AudioClip clip, Vector3 pos)
		{
			return AudioSourceExt.PlayClipAtPointImmuneToTimeScaling (clip, pos, 1.0f);
		}

		public static GameObject PlayClipAtPointImmuneToTimeScaling(AudioClip clip, Vector3 pos, float volume)
		{
			GameObject temp = new GameObject("Jolly Audio Clip");
			temp.transform.position = pos;
			var audioS = temp.AddComponent<AudioSource>();
			audioS.clip = clip;
			audioS.volume = volume;
			int defaultPriority = 128;
			audioS.priority = defaultPriority;
			audioS.Play();
			GameObject.Destroy(temp, clip.length / Time.timeScale);

			return temp;
		}

		public static void StopClipOnObject(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return;
			}
			AudioSource audioSource = gameObject.GetComponent<AudioSource>();
			if (audioSource == null)
			{
				return;
			}

			audioSource.volume = 0;
			audioSource.Stop();
		}
	}
}
