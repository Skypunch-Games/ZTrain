//Copyright 2018, Davin Carten, All rights reserved

using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace emotitron.Utilities
{
	public static class BitArrayExt
	{
		public enum RichColors : uint
		{
			aqua = 0x00ffffff,
			black = 0x000000ff,
			blue = 0x0000ffff,
			brown = 0xa52a2aff,
			cyan = 0x00ffffff,
			darkblue = 0x0000a0ff,
			fuchsia = 0xff00ffff,
			green = 0x008000ff,
			grey = 0x808080ff,
			lightblue = 0xadd8e6ff,
			lime = 0x00ff00ff,
			magenta = 0xff00ffff,
			maroon = 0x800000ff,
			navy = 0x000080ff,
			olive = 0x808000ff,
			orange = 0xffa500ff,
			purple = 0x800080ff,
			red = 0xff0000ff,
			silver = 0xc0c0c0ff,
			teal = 0x008080ff,
			white = 0xffffffff,
			yellow = 0xffff00ff
		}

#if DEBUG || UNITY_EDITOR || DEVELOPMENT_BUILD


		private static StringBuilder str = new StringBuilder(512);

		public static StringBuilder PrintMask(this BitArray ba, int greenbit = -1, int redbit = -1)
		{
			str.Length = 0;
			str.Append("[");
			for (int i = ba.Count - 1; i >= 0; --i)
			{

				if (i == greenbit)
					str.Append("<b><color=#00ff00>" + (ba[i] ? 1 : 0) + "</color></b>");
				else if (i == redbit)
					str.Append("<b><color=#ff0000>" + (ba[i] ? 1 : 0) + "</color></b>");
				else
					str.Append(ba[i] ? "1" : "<color=#0f0f0f>0</color>");

				if (i % 32 == 0)
					str.Append((i == 0) ? "]" : "] [");
				else if (i % 8 == 0 && i != 0)
					str.Append(":");
			}

			return str;
		}

		public static StringBuilder PrintMask(this BitArray ba, int greenbit = -1, bool[] redbits = null)
		{
			str.Length = 0;
			str.Append("[");
			for (int i = ba.Count - 1; i >= 0; --i)
			{
				if (i == greenbit)
					str.Append("<b><color=#00ff00>" + (ba[i] ? 1 : 0) + "</color></b>");
				else if (redbits != null && i < redbits.Length && redbits[i])
					str.Append("<b><color=#ff0000>" + (ba[i] ? 1 : 0) + "</color></b>");
				else
					str.Append(ba[i] ? "1" : "<color=#0f0f0f>0</color>");

				if (i % 32 == 0)
					str.Append((i == 0) ? "]" : "] [");
				else if (i % 8 == 0 && i != 0)
					str.Append(":");
			}

			return str;
		}

		public static StringBuilder PrintMask(this BitArray ba, StringBuilder[] colorbits = null)
		{
			str.Length = 0;
			str.Append("[");
			for (int i = ba.Count - 1; i >= 0; --i)
			{

				if (colorbits != null && i < colorbits.Length && colorbits[i] != null && colorbits[i].ToString() != "")
					str.Append("<b><color="+ colorbits[i].ToString() +">" + (ba[i] ? 1 : 0) + "</color></b>");
				else
					str.Append(ba[i] ? "1" : "<color=#0f0f0f>0</color>");

				if (i % 32 == 0)
					str.Append((i == 0) ? "]" : "] [");
				else if (i % 8 == 0 && i != 0)
					str.Append(":");
			}

			return str;
		}
#else
	public static StringBuilder PrintMask(this BitArray ba, int hiliteBit = -1)
	{
		return null;
	}

#endif

		public static int CountTrue(this BitArray ba)
		{
			int truecount = 0;
			int cnt = ba.Count;
			for (int i = 0; i < cnt; ++i)
				if (ba[i])
					truecount++;

			return truecount;
		}

		public static int CountFalse(this BitArray ba)
		{
			int falsecount = 0;
			int cnt = ba.Count;
			for (int i = 0; i < cnt; ++i)
				if (!ba[i])
					falsecount++;

			return falsecount;
		}

		/// <summary>
		/// Inclusively get relative distance to most future true bit in the range.
		/// </summary>
		public static int CountValidRange(this BitArray ba, int start, int lookahead)
		{
			int len = ba.Length;

			for (int i = lookahead; i >= 0; --i)
			{
				int b = start + i;
				if (b >= len)
					b -= len;

				if (ba[b] == true)
					return i + 1;
			}

			return 0;
		}

		/// <summary>
		/// Non-inclusive clearning of X bits working back from start.
		/// </summary>
		/// <param name=""></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public static void ClearBitsBefore(this BitArray ba, int start, int count)
		{
			int len = ba.Length;

			int cnt = start - count; //  5 - 1 = 4
			for (int i = start - 1; i >= cnt; --i)
			{
				int b = (i < 0) ? i + len: i;
				ba[b] = false;
			}
		}
	}

}
