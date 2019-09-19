// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.


using UnityEngine;
using System.Collections.Generic;

using emotitron.Utilities.SmartVars;

#if UNITY_EDITOR
using emotitron;
using UnityEditor;
#endif

namespace emotitron.SyncAnimInternal
{
	public enum ParameterInterpolation { Default, Hold, Lerp, Advance }
	public enum ParameterMissingHold { Default, Hold }
	public enum ParameterExtrapolation { Default, Hold, Lerp }

	[System.Serializable]
	public class ParameterDefaults
	{
		public ParameterInterpolation interpolateFloats = ParameterInterpolation.Hold;
		public ParameterInterpolation interpolateInts = ParameterInterpolation.Hold;

		public ParameterExtrapolation extrapolateFloats = ParameterExtrapolation.Hold;
		public ParameterExtrapolation extrapolateInts = ParameterExtrapolation.Hold;
		public ParameterExtrapolation extrapolateBools = ParameterExtrapolation.Hold;
		public ParameterExtrapolation extrapolateTriggers = ParameterExtrapolation.Default;

		public bool includeFloats = true;
		public bool includeInts = true;
		public bool includeBools = true;
		public bool includeTriggers = false;

		public SmartVar defaultFloat = (float)0f;
		public SmartVar defaultInt = (int)0;
		public SmartVar defaultBool = (bool)false;
		public SmartVar defaultTrigger = (bool)false;
	}

	/// <summary>
	/// Settings for a single Animator parameter.
	/// </summary>
	[System.Serializable]
	public class ParameterSettings
	{
		// store paramtype so we can check in editor to see if name got reused for a different type
		public int hash;
		public AnimatorControllerParameterType paramType;
		public bool include;
		public ParameterInterpolation interpolate;
		public ParameterExtrapolation extrapolate;
		public SmartVar defaultValue;

		// Constructor
		public ParameterSettings(int hash, ParameterDefaults defs, ref int paramCount, AnimatorControllerParameterType paramType)
		{
			this.hash = hash;
			this.paramType = paramType;

			switch (paramType)
			{
				case AnimatorControllerParameterType.Float:
					include = defs.includeFloats;
					interpolate = defs.interpolateFloats;
					extrapolate = defs.extrapolateFloats;
					defaultValue = (float)defs.defaultFloat;
					break;

				case AnimatorControllerParameterType.Int:
					include = defs.includeInts;
					interpolate = defs.interpolateInts;
					extrapolate = defs.extrapolateInts;
					defaultValue = (int)defs.defaultInt;

					break;
				case AnimatorControllerParameterType.Bool:
					include = defs.includeBools;
					interpolate = ParameterInterpolation.Hold;
					extrapolate = ParameterExtrapolation.Hold;
					defaultValue = (bool)defs.defaultBool;
					break;

				case AnimatorControllerParameterType.Trigger:
					include = defs.includeTriggers;
					interpolate = ParameterInterpolation.Default;
					extrapolate = ParameterExtrapolation.Default;
					defaultValue = (bool)defs.defaultTrigger;
					break;

				default:
					break;
			}
		}

		private static readonly List<int> rebuiltHashes = new List<int>();
		private static readonly List<ParameterSettings> rebuiltSettings = new List<ParameterSettings>();
#if UNITY_EDITOR
		private static List<string> reusableNameList = new List<string>();
#endif
		public static List<string> RebuildParamSettings(Animator a, ref ParameterSettings[] paraSettings, ref int paramCount, ParameterDefaults defs)
		{

#if UNITY_EDITOR
			var ac = a.GetController();
			var parms = ac.parameters;
#else
			var parms = a.parameters;
#endif
			rebuiltHashes.Clear();
			rebuiltSettings.Clear();
#if UNITY_EDITOR
			reusableNameList.Clear();
#endif
			bool hasChanged = false;

			paramCount = parms.Length;
			for (int i = 0; i < paramCount; ++i)
			{
				var p = parms[i];
#if UNITY_EDITOR
				reusableNameList.Add(p.name);
#endif
				var hash = p.nameHash;
				int pos = GetHashIndex(paraSettings, hash);
				if (pos != i)
					hasChanged = true;
				/// remap existing switches for this hash, or if not found make a new one
				rebuiltHashes.Add(hash);
				rebuiltSettings.Add((pos == -1) ? new ParameterSettings(hash, defs, ref paramCount, p.type) : paraSettings[pos]);
			}

			if (hasChanged)
				paraSettings = rebuiltSettings.ToArray();

#if UNITY_EDITOR
			return reusableNameList;
#else
			return null;
#endif
		}

		private static int GetHashIndex(ParameterSettings[] ps, int lookfor)
		{
			int cnt = ps.Length;
			for (int i = 0; i < ps.Length; ++i)
				if (ps[i].hash == lookfor)
					return i;
			return -1;
		}

	}
}
