using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace emotitron.Debugging
{
	public class UIConsole : MonoBehaviour
	{
		public int maxSize = 3000;

		public bool logToDebug = true;

		public readonly static StringBuilder strb = new StringBuilder();

		public static UIConsole single;
		private static Text uitext;

		// Start is called before the first frame update
		void Awake()
		{
			single = this;
			uitext = GetComponent<Text>();
			uitext.text = strb.ToString();
		}

		public static void Log(string str)
		{
			if (!single)
				return;

			if (strb.Length > single.maxSize)
				strb.Length = 0;

			if (uitext != null)
			{
				strb.Append(str).Append("\n");
				uitext.text = strb.ToString();
			}

			if (single.logToDebug)
				Debug.Log(str);
		}

		public static void Refresh()
		{
			if (!single)
				return;

			if (strb.Length > single.maxSize)
				strb.Length = 0;

			if (uitext != null)
			{
				uitext.text = strb.ToString();
			}
		}
		
		public static void Clear()
		{
			strb.Length = 0;

			if (uitext)
				uitext.text = strb.ToString();
		}

	}
}

