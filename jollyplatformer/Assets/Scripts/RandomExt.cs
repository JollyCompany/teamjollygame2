using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Jolly
{
	public class RandomExt
	{
		public static T Pick<T>(T[] array)
		{
			if (array.Length == 0)
			{
				return default(T);
			}
			return array[RandomExt.RangeInclusive(0, array.Length-1)];
		}

		public static T Pick<T>(List<T> list)
		{
			if (list.Count == 0)
			{
				return default(T);
			}
			return list[RandomExt.RangeInclusive(0, list.Count-1)];
		}

		public static void Shuffle<T>(ref T[] array)
		{
			for (int i = array.Length - 1; i > 0; --i)
			{
				int j = RandomExt.RangeExclusive (0, i);
				T swap = array[i];
				array[i] = array[j];
				array[j] = swap;
			}
		}

		public static int RangeInclusive(int min, int max)
		{
			JollyDebug.Assert (min <= max, "Random.RangeInclusive({0},{1}) - Invalid range", min, max);
			return Random.Range (min, max+1);
		}

		public static int RangeExclusive(int min, int max)
		{
			JollyDebug.Assert (min < max, "Random.RangeExclusive({0},{1}) - Invalid range", min, max);
			return Random.Range (min, max);
		}

		public static float RangeInclusive(float min, float max)
		{
			JollyDebug.Assert (min <= max, "Random.RangeInclusive({0},{1}) - Invalid range", min, max);
			return Random.Range (min, max);
		}
	}
}