using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Networking
{
	/// <summary>
	/// Extend this base class for derived SyncObjects to include networked variables.
	/// </summary>
	public abstract class FrameBase
	{
		public int frameId;
		public bool hasChanged;

		public FrameBase(int frameId)
		{
			this.frameId = frameId;
		}

		public abstract void CopyFrom(FrameBase sourceFrame);
		public abstract bool Compare(FrameBase frame, FrameBase holdframe);
	}
}
