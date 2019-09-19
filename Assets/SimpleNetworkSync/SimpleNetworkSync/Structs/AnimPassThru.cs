
namespace emotitron.Networking
{
	public enum LocalApplyTiming { Never, Immediately, OnSend }
	public enum PassThruType { Trigger, ResetTrigger, Play, PlayFixed, CrossFade, CrossFadeFixed }
	/// <summary>
	/// A generic passthrough used to store Play/Crossfade/SetTrigger calls for deferment
	/// </summary>
	public struct AnimPassThru
	{
		public PassThruType passThruType;
		public int hash;
		public float time;
		public float otherTime;
		public int layer;
		public LocalApplyTiming localApplyTiming;

		public AnimPassThru(PassThruType triggerType, int hash, float normTime)
		{
			this.passThruType = triggerType;
			this.hash = hash;
			this.time = normTime;
			this.otherTime = 0;
			this.layer = -1;
			this.localApplyTiming = LocalApplyTiming.Never;
		}
		public AnimPassThru(PassThruType triggerType, int hash, float normTime, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			this.passThruType = triggerType;
			this.hash = hash;
			this.time = normTime;
			this.otherTime = 0;
			this.layer = -1;
			this.localApplyTiming = localApplyTiming;
		}
		public AnimPassThru(PassThruType triggerType, int hash, float normTime, int layer,  LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			this.passThruType = triggerType;
			this.hash = hash;
			this.time = normTime;
			this.otherTime = 0;
			this.layer = layer;
			this.localApplyTiming = localApplyTiming;
		}
		public AnimPassThru(PassThruType triggerType, int hash, int layer, float otherTime, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			this.passThruType = triggerType;
			this.hash = hash;
			this.time = 0;
			this.otherTime = otherTime;
			this.layer = layer;
			this.localApplyTiming = localApplyTiming;
		}
		public AnimPassThru(PassThruType triggerType, int hash, float normTime, int layer, float otherTime, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
		{
			this.passThruType = triggerType;
			this.hash = hash;
			this.time = normTime;
			this.otherTime = otherTime;
			this.layer = layer;
			this.localApplyTiming = localApplyTiming;
		}
	}
}
