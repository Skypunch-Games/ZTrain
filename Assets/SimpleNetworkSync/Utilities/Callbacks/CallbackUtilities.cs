using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Utilities.CallbackUtils
{
	public static class CallbackUtilities
	{
		public static void RegisterInterface<T>(List<T> i, object c, bool register) where T : class
		{
			var iface = (c as T);
			if (ReferenceEquals(iface, null))
				return;

			if (register)
			{
				if (!i.Contains(iface))
					i.Add(iface);
			}
			else
			{
				if (i.Contains(iface))
					i.Remove(iface);
			}
		}
	}
}


