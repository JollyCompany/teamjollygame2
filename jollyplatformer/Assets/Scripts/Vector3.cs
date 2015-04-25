using UnityEngine;
using System.Collections;


namespace Jolly
{
	public static class Vector3Ext
	{
		public static Vector3 SetX(this Vector3 self, float value)
		{
			return new Vector3(value, self.y, self.z);
		}

		public static Vector3 SetY(this Vector3 self, float value)
		{
			return new Vector3(self.x, value, self.z);
		}
			                   
		public static Vector3 SetZ(this Vector3 self, float value)
		{
			return new Vector3(self.x, self.y, value);
		}

		public static Vector3 SetXY(this Vector3 self, float xValue, float yValue)
		{
			return new Vector3(xValue, yValue, self.z);
		}
		
		public static Vector2 xy(this Vector3 self)
		{
			return new Vector2(self.x, self.y);
		}
	}
}

