using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.Helpers
{
	public static class JoinHelper
	{
		public static string Implode<T>(this IEnumerable<T> source, Func<T, string> projection, string separator)
		{
			if (source.Count() == 0)
			{
				return "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(projection(source.First()));
			foreach (T item in source.Skip(1))
			{
				stringBuilder.Append(separator);
				stringBuilder.Append(projection(item));
			}
			return stringBuilder.ToString();
		}

		public static string Implode<T>(this IEnumerable<T> source, string separator)
		{
			return source.Implode((T t) => t.ToString(), separator);
		}
	}
}
