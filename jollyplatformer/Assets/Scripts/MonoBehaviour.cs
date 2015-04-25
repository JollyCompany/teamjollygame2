using UnityEngine;
using System.Collections;

namespace Jolly
{
	public static class MonoBehaviourExtensions
	{
		public static Vector3 xyz(this MonoBehaviour self)
		{
			return self.gameObject.transform.position;
		}

		public static Vector2 xy(this MonoBehaviour self)
		{
			Vector3 xyz = self.xyz ();
			return new Vector2(xyz.x, xyz.y);
		}

		public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
		{
			T component = self.gameObject.GetComponent<T>();
			if (component == null)
			{
				component = self.gameObject.AddComponent<T>();
			}
			return component;
		}

		public static void DrawOutlineText(this MonoBehaviour self, Rect position, string text, GUIStyle style, Color outColor, Color inColor, int outlineSize = 1)
		{
		    GUIStyle backupStyle = style;
		    style.normal.textColor = outColor;
		    position.x -= outlineSize;
		    GUI.Label(position, text, style);
		    position.x += (outlineSize * 2);
		    GUI.Label(position, text, style);
		    position.x -= outlineSize;
		    position.y -= outlineSize;
		    GUI.Label(position, text, style);
		    position.y += (outlineSize * 2);
		    GUI.Label(position, text, style);
		    position.y -= outlineSize;
		    style.normal.textColor = inColor;
		    GUI.Label(position, text, style);
		    style = backupStyle;
		}

		public static void RenderTextureColliderRelative(this MonoBehaviour self, Texture texture, Vector2 size, Vector2 offset)
		{
			if (null == self.GetComponent<Collider>())
			{
				return;
			}

			Camera c = GameObject.Find ("CameraObject").GetComponent<Camera>();

			Vector3 extents = new Vector3(self.GetComponent<Collider>().bounds.extents.x, self.GetComponent<Collider>().bounds.extents.y, self.GetComponent<Collider>().bounds.extents.z);
			Vector3 trnoz = self.GetComponent<Collider>().bounds.center + extents;
			Vector3 blnoz = self.GetComponent<Collider>().bounds.center - extents;
			blnoz.z=0;
			trnoz.z=0;

			Vector3 bl = c.WorldToScreenPoint(blnoz);
			Vector3 tr = c.WorldToScreenPoint(trnoz);

			float w = tr.x - bl.x;
			float h = tr.y - bl.y;

			bl.y = c.pixelHeight - bl.y;

			size = new Vector2(size.x * w, size.y * h);
			offset = new Vector3 (offset.x * w, offset.y * h, 0.0f);

			GUI.DrawTexture (new Rect(bl.x + (w * 0.5f) - (size.x * 0.5f) + offset.x, bl.y - (h * 0.5f) - (size.y * 0.5f) + offset.y, size.x, size.y), texture);
		}

		public static void RenderTexture(this MonoBehaviour self, Texture texture, Vector2 size, Vector2 offset)
		{
			Camera c = GameObject.Find ("CameraObject").GetComponent<Camera>();
			Vector3 trnoz = self.transform.position;
			Vector3 blnoz = self.transform.position;
			blnoz.z=0;
			trnoz.z=0;

			Vector3 bl = c.WorldToScreenPoint(blnoz);
			Vector3 tr = c.WorldToScreenPoint(trnoz);
			float w = 0;
			float h = 0;

			if (self.GetComponent<Collider>() != null)
			{
				Vector3 extents = new Vector3(self.GetComponent<Collider>().bounds.extents.x, self.GetComponent<Collider>().bounds.extents.y, self.GetComponent<Collider>().bounds.extents.z);
				trnoz = self.GetComponent<Collider>().bounds.center + extents;
				blnoz = self.GetComponent<Collider>().bounds.center - extents;
				blnoz.z=0;
				trnoz.z=0;

				bl = c.WorldToScreenPoint(blnoz);
				tr = c.WorldToScreenPoint(trnoz);

				w = tr.x - bl.x;
				h = tr.y - bl.y;
			}

			bl.y = c.pixelHeight - bl.y;

			Vector3 finalOffset = new Vector3 (offset.x * w, offset.y * h, 0.0f);

			size = size * Screen.width / 1920.0f;

			GUI.DrawTexture (new Rect(bl.x + (w * 0.5f) - (size.x * 0.5f) + finalOffset.x, bl.y - (h * 0.5f) - (size.y * 0.5f) + finalOffset.y, size.x, size.y), texture);
		}
	}
}