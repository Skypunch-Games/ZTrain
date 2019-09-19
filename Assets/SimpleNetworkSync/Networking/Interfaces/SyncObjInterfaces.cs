
/// <summary>
/// A collection of interfaces SyncObjects can implement.
/// </summary>
namespace emotitron.Networking
{
	//public interface IOnNetSerialize { bool OnNetSerialize(int frameId, byte[] buffer, ref int bitposition); }
	public interface ISyncAnimator { }
	public interface ISyncTransform { }

	//public interface IOnNetDeserialize { void OnNetDeserialize(int sourceFrameId, int originFrameId, int localFrameId, byte[] buffer, ref int bitposition); }

	public interface IApplyOrder { int ApplyOrder { get; } }
	public interface IOnSnapshot {  void OnSnapshot(int newTargetFrameId, bool isActingAuthority, bool initialize); }
	public interface IOnInterpolate { void OnInterpolate(float t); }
	public interface IOnQuantize { void OnQuantize(int frameId, Realm realm); }
	public interface IOnCaptureCurrentValues { void OnCaptureCurrentValues(int frameId, bool amActingAuthority, Realm realm); } 
	public interface IOnChangeAuthority { void OnChangeAuthority(bool isMine); }
}
