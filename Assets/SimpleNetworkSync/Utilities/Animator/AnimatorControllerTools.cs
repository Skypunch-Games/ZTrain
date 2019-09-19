using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace emotitron
{

	/// <summary>
	/// Storage type for AnimatorController cached transistion data, which is a bit different than basic state hashes
	/// </summary>
	[System.Serializable]
	public class TransitionInfo
	{
		public int index;
		public int hash;
		public int state;
		public int destination;
		public float duration;
		public bool durationIsFixed;
		public TransitionInfo(int index, int hash, int state, int destination, float duration, bool durationIsFixed)
		{
			this.index = index;
			this.hash = hash;
			this.state = state;
			this.destination = destination;
			this.duration = duration;
			this.durationIsFixed = durationIsFixed;
		}
	}

#if UNITY_EDITOR

	public static class AnimatorControllerTools
	{
		public static AnimatorController GetController(this Animator a)
		{
			RuntimeAnimatorController rac = a.runtimeAnimatorController;
			AnimatorOverrideController overrideController = rac as AnimatorOverrideController;
			// recurse until no override controller is found
			while (overrideController != null)
			{
				rac = overrideController.runtimeAnimatorController;
				overrideController = rac as AnimatorOverrideController;
			}

			return rac as AnimatorController;
		}


		public static void GetTriggerNames(this AnimatorController ctr, List<int> hashlist)
		{
			hashlist.Clear();

			foreach (var p in ctr.parameters)
				if (p.type == AnimatorControllerParameterType.Trigger)
				{
					hashlist.Add(Animator.StringToHash(p.name));
					//Debug.Log(p.name + " trig " + Animator.StringToHash(p.name));

				}
		}

		public static void GetStatesNames(this AnimatorController ctr, List<int> hashlist)
		{
			hashlist.Clear();

			foreach (var l in ctr.layers)
				foreach (var st in l.stateMachine.states)
				{
					int hash = Animator.StringToHash(st.state.name);
					int layrhash = Animator.StringToHash(l.name + "." + st.state.name);
					if (!hashlist.Contains(hash))
					{
						hashlist.Add(hash);
						//Debug.Log(st.state.name + "  state " + Animator.StringToHash(st.state.name));
					}
					if (hashlist.Contains(layrhash))
					{
						Debug.LogWarning("Idential State Name <i>'" + st.state.name + "'</i> Found.  Check animator on '" + ctr.name + "' for repeated State names as they cannot be used nor networked.");
					}
					else
						hashlist.Add(Animator.StringToHash(l.name + "." + st.state.name));
					//Debug.Log(l.name + "." + st.state.name + "  state " + Animator.StringToHash(l.name + "." + st.state.name));
				}
		}

		public static void GetTransitionNames(this AnimatorController ctr, List<TransitionInfo> transInfo)
		{
			transInfo.Clear();

			transInfo.Add(new TransitionInfo(0, 0, 0, 0, 0, false));

			foreach (var l in ctr.layers)
			{
				int index = 0;
				foreach (var st in l.stateMachine.states)
				{
					string sname = l.name + "." + st.state.name;
					int shash = Animator.StringToHash(sname);

					foreach (var t in st.state.transitions)
					{
						string dname = l.name + "." + t.destinationState.name;
						int dhash = Animator.StringToHash(dname);
						int hash = Animator.StringToHash(sname + " -> " + dname);
						TransitionInfo ti = new TransitionInfo(index, hash, shash, dhash, t.duration, t.hasFixedDuration);
						transInfo.Add(ti);
						//Debug.Log(sname + " -> " + dname + "   " + Animator.StringToHash(sname + " -> " + dname));
					}
					index++;
				}
			}
		}
	}
#endif

}

