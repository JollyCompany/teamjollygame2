using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Jolly
{
	public static class RandomExt
	{
		static System.Random SharedRandom = new System.Random();
	
		public static T Pick<T>(T[] array)
		{
			return SharedRandom.Pick(array);
		}
	
		public static T Pick<T>(List<T> list)
		{
			return SharedRandom.Pick(list);
		}
	
		public static void Shuffle<T>(ref T[] array)
		{
			SharedRandom.Shuffle(ref array);
		}
	
		public static int RangeInclusive(int min, int max)
		{
			return SharedRandom.RangeInclusive(min, max);
		}
	
		public static int RangeExclusive(int min, int max)
		{
			return SharedRandom.RangeExclusive(min, max);
		}
	
		public static float RangeInclusive(float min, float max)
		{
			return SharedRandom.RangeInclusive(min, max);
		}
		
		public static int RangeExclusive (this System.Random self, int min, int max)
		{
			return self.Next (min, max);
		}
		
		public static int RangeInclusive (this System.Random self, int min, int max)
		{
			return self.Next (min, max+1);
		}
		
		public static float RangeInclusive (this System.Random self, float min, float max)
		{
			return (float)(self.NextDouble () * (max - min) + min);
		}
		
		public static void Shuffle<T> (this System.Random self, ref T[] array)
		{
			for (int i = array.Length - 1; i > 0; --i)
			{
				int j = self.RangeExclusive (0, i);
				T swap = array[i];
				array[i] = array[j];
				array[j] = swap;
			}
		}
		
		public static float NextFloat (this System.Random self)
		{
			return (float)self.NextDouble();
		}
	
		public static T Pick<T>(this System.Random self, T[] array)
		{
			if (array.Length == 0)
			{
				return default(T);
			}
			return array[self.RangeExclusive(0, array.Length)];
		}
	
		public static T Pick<T>(this System.Random self, List<T> list)
		{
			if (list.Count == 0)
			{
				return default(T);
			}
			return list[self.RangeExclusive(0, list.Count)];
		}
		
		public static Vector2 PointInRect (Rect rect)
		{
			return SharedRandom.PointInRect (rect);
		}
	
		public static Vector2 PointInRect (this System.Random self, Rect rect)
		{
			return new Vector2 (self.RangeInclusive (rect.xMin, rect.xMax), self.RangeInclusive (rect.yMin, rect.yMax));
		}
		
		public static bool Boolean (this System.Random self)
		{
			return self.NextDouble() < 0.5;
		}
	}
}